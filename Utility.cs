using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;

namespace AnimeDownloader
{
    static class Utility
    {

        public const string TorrentClientPathWindows = @"C:\Program Files\qBittorrent\qbittorrent.exe";
        public const string TorrentClientPathLinux = @"/usr/bin/qbittorrent";
        public static WebClient WClient = new();
        public static int AnimeQualityIndex = 0;

        public enum WindowHelp
        {
            MainWindowHelp,
            WatchlistWindowHelp
        }

        public static void DownloadAnime(string torrentLink)
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                Process.Start(TorrentClientPathWindows, torrentLink);
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                Process.Start(TorrentClientPathLinux, torrentLink);
            else
                DisplayError("ERROR: can't download the torrent file");

        }
        /// <summary>
        /// Slice out anime name from complete title
        /// </summary>
        /// <param name="title">Anime title to slice from</param>
        /// <returns>Cleared title</returns>
        public static string GetCleanName(ReadOnlySpan<char> title)
        {
            int sliceStart = 0;
            int sliceLength = title.Length;

            for (int i = title.Length - 1; i != 0; i--)
            {
                if (title[i] == '-') 
                {
                    sliceLength = i - sliceStart;
                    break; // This assumes that theres only 1 '-'
                }
            }

            return title.Slice(sliceStart, sliceLength).ToString();
        }
        /// <summary>
        /// Remove all bracket content in the title
        /// </summary>
        /// <param name="title">Anime title to remove bracket content from</param>
        /// <returns>Title with no bracket contents</returns>
        public static string RemoveBracketContentFromTitle(string title)
        {
            // Before: [MAGNET]ANIME NAME HERE - 69 [1080p][en]
            // After: ANIME NAME HERE - 69
            StringBuilder newTitle = new();

            bool isInBracket = false;
            foreach (var ch in title)
            {
                if (ch == '[')
                {
                    isInBracket = true;
                    continue;
                }
                else if (ch == ']')
                {
                    isInBracket = false;
                    continue;
                }

                if (!isInBracket)
                    newTitle.Append(ch);
            }

            return newTitle.ToString();
        }
        public static int GetAnimeEpisodeNumberFromTitle(string title)
        {
            for (int i = title.Length - 1; i != 0; i--)
            {
                if (title[i] == '-')
                {
                    var newSplit = title.Substring(i).Split(' ');// FORMAT: animeName - episodeNumber [othershit]...
                    if (int.TryParse(newSplit[1], out int number))
                        return number;
                    else
                        return -1;
                }
            }
            return -1;
        }

        public static void DisplayError(string infoText)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(infoText);
            Console.ResetColor();
            Console.WriteLine("Press any key to continue...");
            Console.ReadKey(true);
        }

        public static void SetSettingsFromFile()
        {
            if (!File.Exists("Settings.txt"))
                return;
            using var fs = new StreamReader("Settings.txt");
            var line = fs.ReadLine();
            var sizes = line.Split();
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows) 
                && sizes.Length == 3 
                && int.TryParse(sizes[0], out int Width) 
                && int.TryParse(sizes[1], out int Height) 
                && int.TryParse(sizes[2], out int LinkIndex))
            {
                Console.SetWindowSize(Width, Height);
                AnimeQualityIndex = LinkIndex;
                return;
            }
        }
        public static void SaveSettingsToFile()
        {
            using var fs = new StreamWriter("Settings.txt", false);
            fs.WriteLine($"{Console.WindowWidth} {Console.WindowHeight} {Utility.AnimeQualityIndex}");
        }

        private static string GetHelpString(WindowHelp specificWindow) => specificWindow switch
        {
            WindowHelp.MainWindowHelp => "[Any other key - quit]\n[0-... - anime to be downloaded]\n[w - add to watch list (eg. 0 11 43 ...)]\n[dw - display watchlist]\n[sq - switch quality]\n[r - refresh]",
            WindowHelp.WatchlistWindowHelp => "[q - go back to main window]\n[0-... - download anime]\n[x - mark as downloaded (eg. 1 5 10 30 ...)]\n[r - multiple removal (eg. 1 5 10 30 ...)]"
        };
        public static void DisplayHelp(WindowHelp specificWindow)
        {
            Console.WriteLine("\nAvailable options:");
            Console.WriteLine(GetHelpString(specificWindow));
            Console.WriteLine("Press any key to continue...");
            Console.ReadKey(true);
        }
    }
}
