using System;
using System.Diagnostics;
using System.Net;

namespace AnimeDownloader
{
    static class Utility
    {
        public const string TorrentClientPath = @"C:\Program Files\qBittorrent\qbittorrent.exe";
        public static WebClient WClient = new WebClient();
        public static void DownloadAnime(string torrentLink)
        {
            Process.Start(TorrentClientPath, torrentLink);
        }
        /// <summary>
        /// Gets rid of episode number and quality tag at the beggining
        /// </summary>
        /// <param name="title">Anime title to clear</param>
        /// <returns>Cleared title</returns>
        public static string GetCleanName(ReadOnlySpan<char> title)
        {
            int sliceStart = 0;
            int sliceLength = title.Length;
            bool startedSlice = false;
            for (int i = 0; i < title.Length; i++)
            {
                if(!startedSlice && title[i] == '[')
                {
                    startedSlice = true;
                }
                else if(startedSlice && title[i] == ']')
                {
                    sliceStart = i + 2; // exclude ']' and ' ' characters
                    break;
                }
            }
            for (int i = title.Length - 1; i != 0; i--)
            {
                if (title[i] == '-')
                {
                    sliceLength = i - sliceStart;
                }
            }

            return title.Slice(sliceStart, sliceLength).ToString();
        }
        /// <summary>
        /// Get the episode number from the anime title
        /// </summary>
        /// <param name="title">Anime title</param>
        /// <returns>If successful returns the episode number, otherwise returns -1</returns>
        public static int GetAnimeEpisodeNumber(string title)
        {
            for (int i = title.Length - 1; i != 0; i--)
            {
                if (title[i] == '-')
                {
                    var newSplit = title.Substring(i).Split(' ');// FORMAT: [-][episodeNumber][othershit]...
                    if (int.TryParse(newSplit[1], out int number))
                        return number;
                    else
                        return -1;
                }
            }
            return -1;
        }
    }
}
