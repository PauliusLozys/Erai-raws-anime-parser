using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;

namespace AnimeDownloader
{
    class Program
    {
        static void Main(string[] args)
        {
            SetSettingsFromFile(out int linkIndex);
            string[] downloadLinks = new string[] {"https://www.erai-raws.info/rss-1080/",
                                                   "https://www.erai-raws.info/rss-720/",
                                                   "https://www.erai-raws.info/rss-480/" };
            string choice;

            WatchListManager watchList = new WatchListManager();
            AnimeManager anime = new AnimeManager(watchList);

            AppDomain.CurrentDomain.ProcessExit += ExitEvent;
            void ExitEvent(object sender, EventArgs e)
            {
                if (File.Exists(Utility.TempFile))
                    File.Delete(Utility.TempFile);
                watchList.WriteWatchListFile();
                SaveSettingsToFile(ref linkIndex);
            }

            anime.ParseItemsXml(ref downloadLinks[linkIndex]);

            while (true)
            {
                Console.Clear();
                Console.ForegroundColor = ConsoleColor.Green;
                Console.ResetColor();
                anime.DisplayAnimeList();
                Console.WriteLine("[Any other key - quit] [0-... - anime to be downloaded] [w - add to watch list (eg. 0 11 43 ...)] [dw - display watchlist] [sq - switch quality] [r - refresh]");
                Console.Write("Pick a choice: ");
                choice = Console.ReadLine().ToLower();

                if (int.TryParse(choice, out int result)) // Download anime
                {
                    anime.DownloadSelectedAnime(result);
                }
                else if (choice == "w") // Add to watch list
                {
                    Console.Write("Add animes to be added: ");
                    choice = Console.ReadLine();
                    if (string.IsNullOrEmpty(choice))
                        continue;
                    var values = choice.Split(null);
                    anime.AddAnimesToWatchList(values);
                }
                else if (choice == "r") // Refresh
                {
                    anime.ParseItemsXml(ref downloadLinks[linkIndex]);
                }
                else if (choice == "sq") // Switch quality
                {
                    Console.Write("0| 1080p\n1| 720p\n2| other\nPick quality: ");
                    choice = Console.ReadLine();
                    if (string.IsNullOrEmpty(choice))
                        continue;
                    if (int.TryParse(choice, out int index) && index < 3 && index >= 0)
                    {
                        if (linkIndex == index)
                            continue;
                        linkIndex = index;
                        anime.ParseItemsXml(ref downloadLinks[linkIndex]);
                    }
                    else
                        DisplayError($"ERROR: INVALID INDEX {choice} PROVIDED");
                }
                else if (choice == "dw") // Display watch list
                {
                    while (true)
                    {
                        watchList.DisplayWatchList();

                        Console.WriteLine("[q - go back to main window] [0-... - download anime] [r - multiple removal (eg. 1 5 10 30 ...)]");
                        Console.Write("Pick a choice: ");
                        choice = Console.ReadLine().ToLower();
                        if (int.TryParse(choice, out int index))
                        {
                            watchList.DownloadSelectedWatchListAnime(index);
                        }
                        else if (choice == "r")
                        {
                            if (watchList.Count == 0)
                            {
                                DisplayError($"ERROR: THERE ARE NO ENTRIES IN THE WATCHLIST");
                                continue;
                            }
                            Console.Write("Add animes to be removed: ");
                            choice = Console.ReadLine();

                            if (string.IsNullOrEmpty(choice))
                                continue;
                            var values = choice.Split(null);
                            watchList.RemoveMultipleEntriesFromWatchList(values);
                            anime.CheckIfContainsAnimesFromWatchList(); // Recheck the anime list
                        }
                        else if (choice == "q")
                            break;
                    }
                }
                else
                {
                    break;
                }

            }
        }
        public static void DisplayError(string infoText)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(infoText);
            Console.ResetColor();
            Console.WriteLine("Press any key to continue...");
            Console.ReadKey(true);
        }
        public static void SetSettingsFromFile(out int linkIndex)
        {
            if (!File.Exists("Settings.txt"))
            {
                linkIndex = 0; // Set default quality as highest
                return;
            }
            using var fs = new StreamReader("Settings.txt");
            var line = fs.ReadLine();
            var sizes = line.Split();
            if (sizes.Length == 3 && int.TryParse(sizes[0], out int Width) && int.TryParse(sizes[1], out int Height) && int.TryParse(sizes[2], out int LinkIndex))
            {
                Console.SetWindowSize(Width, Height);
                linkIndex = LinkIndex;
                return;
            }
            linkIndex = 0; // Set default quality as highest
        }
        public static void SaveSettingsToFile(ref int linkIndex)
        {
            using var fs = new StreamWriter("Settings.txt", false);
            fs.WriteLine($"{Console.WindowWidth} {Console.WindowHeight} {linkIndex}");
        }
    }
}
