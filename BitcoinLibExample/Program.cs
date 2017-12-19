using System;
using BitcoinLib.Services.Coins.Bitcoin;
using Newtonsoft.Json.Linq;

namespace BitcoinLibExample
{
    class Program
    {
        private static IBitcoinService _BitcoinManager;
        public static IBitcoinService BitcoinManager
        {
            get { return _BitcoinManager; }
            private set { }
        }

        static void Main(string[] args)
        {
            Console.WriteLine("Setting up!!");

            // Bitcoind
            _BitcoinManager = new BitcoinService(ApplicationSettings.GetEncryptedString("BTCNodeDaemonURL"),
                                                 ApplicationSettings.GetEncryptedString("BTCNodeJsonRPCUser"),
                                                 ApplicationSettings.GetEncryptedString("BTCNodeJsonRPCPass"),
                                                "", 30); // Wallet passphrase will be put up later..

            Console.WriteLine("Testing BTC Service...");

            Console.WriteLine("Bitcoin Difficulty: " + _BitcoinManager.GetDifficulty());
            //Console.WriteLine("" + _BitcoinManager.GetNetworkInfo().LocalServices);

            while (true) {
                Console.WriteLine();
                // Here's the prepared hash = de6605800010593850355721072bcc44bd2c5683a111a615c41aad3a05b8ac70
                //9c9dd22fc1a0222be84a756a19f8e1729941886b7b7e47f9eeb65780dbc88c62
                Console.WriteLine("Enter a raw transaction: ");
                string input = Console.ReadLine();

                if (input.Equals("quit")) {
                    Environment.Exit(0);
                }

                var rawTxResp = _BitcoinManager.GetRawTransaction(input);
                Console.WriteLine(JObject.FromObject(rawTxResp));
            }
        }
    }
}
