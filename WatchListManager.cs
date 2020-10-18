using System;
using System.Collections.Generic;
using System.IO;

namespace AnimeDownloader
{
    class WatchListManager
    {
        private List<WatchListItem> WatchList { get; set; }
        private readonly string FileName = "animeListing.txt";
        private readonly string Seperator = new string('-', Console.WindowWidth - 1);

        public WatchListManager()
        {
            WatchList = new List<WatchListItem>();
            // Read a watchlist file
            ReadWatchListFile();
        }

        public int Count { get => WatchList.Count; }
        public void DisplayWatchList()
        {
            static string todayPrint()
            {
                Console.ForegroundColor = ConsoleColor.Green;
                return "Today";
            }

            Console.Clear();
            Console.ForegroundColor = ConsoleColor.Cyan;
            if (WatchList.Count == 0)
            {
                Console.WriteLine("There is no anime in here :(");
                Console.ResetColor();

                return;
            }
            Console.WriteLine(Seperator);

            for (int i = 0; i < WatchList.Count; i++)
            {
                var cachedDate = WatchList[i].ReleaseDay.AddDays(7);

                if (WatchList[i].FinishedAiring)
                {
                    Console.ForegroundColor = ConsoleColor.White;
                    if (!string.IsNullOrEmpty(WatchList[i].LatestEpisodeLink))
                    {
                        Console.WriteLine("{0,2}| {1,-80}| Last episode: {2,-5}| Next episode release date: Finished airing",
                        i,
                        WatchList[i].Title,
                        WatchList[i].LatestEpisode
                        );
                    }
                    else
                    {
                        Console.WriteLine("{0,2}| {1,-80}| Last episode: not found | Finished airing",
                            i,
                            WatchList[i].Title,
                            WatchList[i].LatestEpisode
                            );
                    }
                }
                else if (!WatchList[i].IsDownloaded) // Is NOT downloaded
                {
                    Console.ForegroundColor = ConsoleColor.Magenta;
                    Console.WriteLine("{0,2}| {1,-80}| Latest Episode: {2,-3}| Next episode release date: {3} {4}:{5}",
                        i,
                        WatchList[i].Title,
                        WatchList[i].LatestEpisode,
                        cachedDate.Date == DateTime.Now.Date ? "Today" : cachedDate.ToShortDateString(),
                        WatchList[i].ReleaseDay.Hour,
                        WatchList[i].ReleaseDay.Minute < 30 ? "00" : "30"
                        );
                }
                else
                    Console.WriteLine("{0,2}| {1,-80}| Latest Episode: {2,-3}| Next episode release date: {3} {4}:{5}",
                        i,
                        WatchList[i].Title,
                        WatchList[i].LatestEpisode,
                        cachedDate.Date == DateTime.Now.Date ? todayPrint() : cachedDate.ToShortDateString(),
                        WatchList[i].ReleaseDay.Hour,
                        WatchList[i].ReleaseDay.Minute < 30 ? "00" : "30"
                        );
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine(Seperator);
            }
            Console.ResetColor();
        }
        public void AddToWatchList(string[] animesIndexes, List<AnimeItem> animeList)
        {
            foreach (var item in animesIndexes)
            {
                if (!int.TryParse(item, out int index) || index >= animeList.Count || index < 0)
                {
                    Program.DisplayError($"ERROR: INVALID INDEX: {item} PROVIDED, SO IT WON'T BE ADDED");
                    continue;
                }

                var title = animeList[index].Title.AsSpan();

                var animeToBeAdded = new WatchListItem()
                {
                    Title = Utility.GetCleanName(title),
                    LatestEpisode = 0,
                    IsDownloaded = false,
                    ReleaseDay = animeList[index].PubDate,
                    FinishedAiring = title.EndsWith("END")
                };

                // Check if it already exists in the watchlist
                if (WatchList.Contains(animeToBeAdded))
                {
                    Program.DisplayError($"Anime \"{animeToBeAdded.Title}\" already exists in the watchlist");
                    break;
                }

                // Add a new watchlist value
                WatchList.Add(animeToBeAdded);

            }
            WatchList.Sort();
        }
        public void RemoveMultipleEntriesFromWatchList(string[] indexes)
        {
            List<WatchListItem> tmp = new List<WatchListItem>(indexes.Length);

            foreach (var item in indexes)
            {
                if (int.TryParse(item, out int index) && index < WatchList.Count)
                {
                    tmp.Add(new WatchListItem()
                    {
                        Title = WatchList[index].Title,
                        LatestEpisode = WatchList[index].LatestEpisode,
                        IsDownloaded = WatchList[index].IsDownloaded
                    });
                }
                else
                {
                    Program.DisplayError($"ERROR: INDEX {item} DOES NOT EXIST, STOPPING THE REMOVAL");
                    return;
                }
            }

            foreach (var item in tmp)
            {
                WatchList.Remove(new WatchListItem()
                {
                    Title = item.Title,
                    LatestEpisode = item.LatestEpisode,
                    IsDownloaded = item.IsDownloaded
                });
            }
        }
        public bool ContainsInWatchList(AnimeItem anime)
        {
            foreach (var item in WatchList)
            {
                if (anime.Title.Contains(item.Title))
                {
                    int episodeNumber = Utility.GetAnimeEpisodeNumber(anime.Title);
                    if (episodeNumber > item.LatestEpisode)
                    {
                        // Newer version of the episode was found
                        item.LatestEpisodeLink = anime.Link;
                        item.LatestEpisode = episodeNumber;
                        item.IsDownloaded = false;
                        item.ReleaseDay = anime.PubDate;
                        item.FinishedAiring = anime.Title.EndsWith("END");
                    }
                    else if (episodeNumber == item.LatestEpisode)
                    {
                        item.LatestEpisodeLink = anime.Link;
                        item.FinishedAiring = anime.Title.EndsWith("END");
                    }
                    return true;
                }
            }
            return false;
        }
        public void DownloadSelectedWatchListAnime(int index)
        {
            if (index < WatchList.Count && index >= 0)
            {
                if (string.IsNullOrEmpty(WatchList[index].LatestEpisodeLink))
                {
                    Program.DisplayError("ERROR: This anime has no link");
                    return;
                }
                Utility.DownloadAnime(WatchList[index].LatestEpisodeLink);
                WatchList[index].IsDownloaded = true;
            }
            else
                Program.DisplayError($"ERROR: THE NUMBER PROVIDED IS TOO LARGE");
        }
        public void SetAnimeAsDownloadedByAnimeItem(AnimeItem anime)
        {
            foreach (var item in WatchList)
            {
                if (anime.Title.Contains(item.Title))
                {
                    int episodeNumber = Utility.GetAnimeEpisodeNumber(anime.Title);
                    if (episodeNumber == item.LatestEpisode)
                    {
                        item.IsDownloaded = true;
                        item.ReleaseDay = anime.PubDate;
                        return;
                    }
                }
            }
        } 
        public void SortWatchList() => WatchList.Sort();
        public void WriteWatchListFile()
        {
            if (WatchList.Count == 0)
                return;

            using var fs = new StreamWriter(FileName, false);
            foreach (var item in WatchList)
            {
                fs.WriteLine($"{item.Title};{item.LatestEpisode};{item.IsDownloaded};{item.ReleaseDay};{item.FinishedAiring}");
            }
        }
        private void ReadWatchListFile()
        {
            if (!File.Exists(FileName))
                return;

            using var fs = new StreamReader(FileName);
            while (!fs.EndOfStream)
            {
                //Format title;latest episode;is downloaded
                var line = fs.ReadLine().Split(';');
                if (line.Length != 5)
                {
                    Program.DisplayError("Error happened while reading the watchlist file, so it won't be loaded");
                    return;
                }
                WatchList.Add(new WatchListItem()
                {
                    Title = line[0],
                    LatestEpisode = int.Parse(line[1]),
                    IsDownloaded = bool.Parse(line[2]),
                    ReleaseDay = DateTime.Parse(line[3]),
                    FinishedAiring = bool.Parse(line[4])
                });
            }
        }
        
    }
}
