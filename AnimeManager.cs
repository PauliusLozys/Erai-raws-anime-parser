using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace AnimeDownloader
{
    class AnimeManager
    {
        private List<AnimeItem> AnimeList = new List<AnimeItem>();
        private readonly WatchListManager WatchList;

        public AnimeManager(WatchListManager watchlist)
        {
            WatchList = watchlist;
        }

        /// <summary>
        /// Returns the amount of animes stored
        /// </summary>
        public int Count { get => AnimeList.Count; }
        public void DisplayAnimeList()
        {
            Console.WriteLine("CURRENT ANIME LIST:");
            Console.ForegroundColor = ConsoleColor.Green;

            for (int i = 0; i < AnimeList.Count; i++)
            {
                if (AnimeList[i].IsInWatchList)
                {
                    Console.ForegroundColor = ConsoleColor.Cyan;
                    Console.WriteLine("{0,2}| {1,-90} {2}", i, AnimeList[i].Title, AnimeList[i].PubDate.ToShortDateString());
                    Console.ForegroundColor = ConsoleColor.Green;
                }
                else if (AnimeList[i].IsReleasedToday)
                {
                    Console.ForegroundColor = ConsoleColor.Magenta;
                    Console.WriteLine("{0,2}| {1,-90} {2}", i, AnimeList[i].Title, AnimeList[i].PubDate.ToShortDateString());
                    Console.ForegroundColor = ConsoleColor.Green;
                }
                else
                    Console.WriteLine("{0,2}| {1,-90} {2}", i, AnimeList[i].Title, AnimeList[i].PubDate.ToShortDateString());
            }
            Console.ResetColor();
        }
        public void AddAnimesToWatchList(string[] values)
        {
            WatchList.AddToWatchList(values, AnimeList);
            CheckIfContainsAnimesFromWatchList();
        }
        public void DownloadSelectedAnime(int index)
        {
            Utility.WClient.Headers["User-Agent"] = "Mozilla/4.0 (Compatible; Windows NT 5.1; MSIE 6.0) ";
            if (index < AnimeList.Count && index >= 0)
            {
                Utility.DownloadAnime(AnimeList[index].Link);
                WatchList.SetAnimeAsDownloadedByAnimeItem(AnimeList[index]);
            }
            else
                Utility.DisplayError($"ERROR: THE NUMBER PROVIDED IS TOO LARGE");
        }
        public void CheckIfContainsAnimesFromWatchList()
        {
            foreach (var anime in AnimeList)
            {
                anime.IsInWatchList = WatchList.ContainsInWatchList(anime);
            }
        }
        public void ParseItemsXml(ref string downloadUrl)
        {
            string downloadedXml;
            try
            {
                downloadedXml = Utility.WClient.DownloadString(downloadUrl);
            }
            catch (Exception e)
            {
                throw new Exception($"ERROR: COULD NOT DOWNLOAD RSS FILE: {downloadUrl}\n{e.Message}");
            }
            AnimeList.Clear();

            var tokens = downloadedXml.Split(new string[] { "<item>", "</channel>" }, StringSplitOptions.None);
            for (int i = 0; i < tokens.Length; i++)
            {
                tokens[i] = tokens[i].TrimEnd();
                // NOTE: are those still required?
                //tokens[i] = Regex.Replace(tokens[i], "&#8211;", "-");
                //tokens[i] = Regex.Replace(tokens[i], "&#8217;", "'");

                if (!tokens[i].EndsWith("</item>")) // Unneeded token, skipping it
                    continue;

                var tmpTitle = ExtractString(ref tokens[i], "<title>", "</title>");
                tmpTitle = Utility.RemoveBracketContentFromTitle(tmpTitle);
                var tmpDate = DateTime.Parse(ExtractString(ref tokens[i], "<pubDate>", "</pubDate>"));
                var tmpLink = ExtractString(ref tokens[i], "<link>", "</link>");
                AnimeList.Add(new AnimeItem()
                {
                    Title = tmpTitle,
                    Link = tmpLink,
                    PubDate = tmpDate,
                    IsInWatchList = WatchList.ContainsInWatchList(new AnimeItem { Title = tmpTitle, Link = tmpLink, PubDate = tmpDate }),
                    IsReleasedToday = tmpDate.Date == DateTime.Now.Date
                });
            }
            // Sort the list with possibly updated DateTime values
            WatchList.SortWatchList();
        }
        private static string ExtractString(ref string str, string startingTag, string endingTag)
        {
            StringBuilder builder = new StringBuilder();
            var _str = str.ToCharArray();
            var _startingTag = startingTag.ToCharArray();
            var _endingTag = endingTag.ToCharArray();

            for (int i = 0; i < _str.Length; i++)
            {
                bool foundStart = true;
                for (int j = 0; j < _startingTag.Length; j++)
                {
                    if (_str[i + j] != _startingTag[j])
                    {
                        foundStart = false;
                        break;
                    }
                }

                if (foundStart)
                {
                    for (int l = i + _startingTag.Length; l < str.Length; l++)
                    {
                        bool foundEnd = true;

                        for (int j = 0; j < _endingTag.Length; j++)
                        {
                            if (_str[l + j] != _endingTag[j])
                            {
                                foundEnd = false;
                                break;
                            }
                        }

                        if (!foundEnd)
                            builder.Append(_str[l]);
                        else
                        {
                            return builder.ToString();
                        }
                    }
                }
            }
            return string.Empty;
        }

    }
}
