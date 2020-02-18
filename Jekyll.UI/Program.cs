using JekyllLibrary.Library;
using System;

namespace Jekyll.UI
{
    class Program
    {
        /// <summary>
        /// Active Instance
        /// </summary>
        private static JekyllInstance Instance = new JekyllInstance();

        static void Main()
        {
            Console.WriteLine("Jekyll: Call of Duty Asset Exporter");
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine("https://github.com/EthanC/Jekyll\n");
            Console.ResetColor();

            var status = Instance.LoadGame();

            if (status == JekyllStatus.Success)
            {
                foreach (var asset in Instance.Assets)
                {
                    asset.AssetPool.Export(asset, Instance);
                }
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Failed to find a supported game");
                Console.ResetColor();
            }

            Console.WriteLine("\nPress any key to exit...");
            Console.ReadKey();
        }
    }
}