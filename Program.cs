using System;

namespace AnimeDownloader
{
    class Program
    {
        static void Main(string[] args)
        {
            Utility.SetSettingsFromFile();
            string[] downloadLinks = new string[] {"https://www.erai-raws.info/episodes/feed/?res=1080p&type=magnet&subs%5B0%5D=us",
                                                   "https://www.erai-raws.info/episodes/feed/?res=720p&type=magnet&subs%5B0%5D=us",
                                                   "https://www.erai-raws.info/episodes/feed/?res=SD&type=magnet&subs%5B0%5D=us"};
            string choice;

            WatchListManager watchList = new WatchListManager();
            AnimeManager anime = new AnimeManager(watchList);

            AppDomain.CurrentDomain.ProcessExit += (object sender, EventArgs e) => 
            {
                watchList.WriteWatchListFile();
                Utility.SaveSettingsToFile();
            };

            anime.ParseItemsXml(ref downloadLinks[Utility.AnimeQualityIndex]);

            while (true)
            {
                Console.Clear();
                Console.ForegroundColor = ConsoleColor.Green;
                Console.ResetColor();
                anime.DisplayAnimeList();
                Console.WriteLine("Type 'h' to get available commands");
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
                    anime.ParseItemsXml(ref downloadLinks[Utility.AnimeQualityIndex]);
                }
                else if (choice == "h")
                {
                    Utility.DisplayHelp(Utility.WindowHelp.MainWindowHelp);
                }
                else if (choice == "sq") // Switch quality
                {
                    Console.Write("0| 1080p\n1| 720p\n2| SD\nPick quality: ");
                    choice = Console.ReadLine();
                    if (string.IsNullOrEmpty(choice))
                        continue;
                    if (int.TryParse(choice, out int index) && index < 3 && index >= 0)
                    {
                        if (Utility.AnimeQualityIndex == index)
                            continue;
                        Utility.AnimeQualityIndex = index;
                        anime.ParseItemsXml(ref downloadLinks[Utility.AnimeQualityIndex]);
                    }
                    else
                        Utility.DisplayError($"ERROR: INVALID INDEX {choice} PROVIDED");
                }
                else if (choice == "dw") // Display watch list
                {
                    while (true)
                    {
                        watchList.DisplayWatchList();

                        Console.WriteLine("Type 'h' to get available commands");
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
                                Utility.DisplayError($"ERROR: THERE ARE NO ENTRIES IN THE WATCHLIST");
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
                        else if (choice == "h")
                        {
                            Utility.DisplayHelp(Utility.WindowHelp.WatchlistWindowHelp);
                        }
                        else if (choice == "x")
                        {
                            if (watchList.Count == 0)
                            {
                                Utility.DisplayError($"ERROR: THERE ARE NO ENTRIES IN THE WATCHLIST");
                                continue;
                            }
                            Console.Write("Mark animes as watched: ");
                            choice = Console.ReadLine();

                            if (string.IsNullOrEmpty(choice))
                                continue;
                            var values = choice.Split(null);
                            watchList.SetAnimeAsDownloadedByWatchlistIndexes(values);

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
    }
}
