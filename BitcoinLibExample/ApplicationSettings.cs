using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Newtonsoft.Json.Linq;

namespace BitcoinLibExample
{
    /// <summary>
    /// A wrapper class for the Microsoft.Extensions.Configuration class
    /// that serves as a convenient way to acquire settings off appsettings.json
    /// </summary>
    public class ApplicationSettings
    {
        private static Dictionary<string, string> DataCache = new Dictionary<string, string>();

        #region Protected Settings Init
        private static bool IsProtectedSettingsLoaded = false;
        private static Object IsProtectedSettingsLoadedLock = new Object();

        /// <summary>
        /// Load application api/keys from sitekeyconfig.json and stores it in memory-encrypted.
        /// </summary>
        public static void InitializeProtectedAppSettings()
        {
            lock (IsProtectedSettingsLoadedLock)
            {
                if (!IsProtectedSettingsLoaded)
                {
                    IsProtectedSettingsLoaded = true;

                    const string resourceName = "config.json";

                    Assembly assembly = Assembly.GetExecutingAssembly();
                    string codeBase = assembly.CodeBase;
                    UriBuilder uri = new UriBuilder(codeBase);
                    string path = Path.Combine((Path.GetDirectoryName(uri.Path)), resourceName);

                    using (Stream stream = new FileStream(path, FileMode.OpenOrCreate))
                    {
                        using (StreamReader reader = new StreamReader(stream))
                        {
                            JObject JsonObject = JObject.Parse(reader.ReadToEnd());

                            RecursivelyLoadJsonSettings(JsonObject, null);
                        }
                    }
                }
            }
        }

        private static void RecursivelyLoadJsonSettings(JObject JsonObject, JProperty JsonProperty)
        {
            foreach (JToken token in JsonProperty == null ? JsonObject.Children() : JsonProperty.Children())
            {
                if (token.Type == JTokenType.Property)
                {
                    RecursivelyLoadJsonSettings(null, (JProperty)token);
                }
                else if (token.Type == JTokenType.Object)
                {
                    RecursivelyLoadJsonSettings((JObject)token, null);
                }
                else
                {
                    if (token.Type == JTokenType.String)
                    {
                        string path = token.Path.Replace(".", "/");
                        string value = (string)token;

                        DataCache.Add(SHA256Hash.Hash(path), AES256Hash.EncryptString(path, value)); // encrypt with real path independent of the hashed path key.
                    }
                }
            }
        }

        /// <summary>
        /// Gets an encrypted string settings from the sitekeyconfig.json file.
        /// 
        /// Prevents memory based attacks to extract any sensitive keys in cache.
        /// </summary>
        /// <param name="path"></param>
        /// <param name="defaultfallback">The default value if it does not exist</param>
        /// <returns></returns>
        public static string GetEncryptedString(string path, string defaultfallback = "")
        {
            InitializeProtectedAppSettings();

            // Check cache
            string hashedPath = SHA256Hash.Hash(path);
            if (DataCache.ContainsKey(hashedPath))
            {
                return AES256Hash.DecryptString(path, DataCache[hashedPath]); // decrypt with real path independent of the hashed path key.
            }
            return null;
        }
        #endregion
    }
}
