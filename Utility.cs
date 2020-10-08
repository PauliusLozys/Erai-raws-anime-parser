using System;
using System.Diagnostics;
using System.Net;

namespace AnimeDownloader
{
    static class Utility
    {
        public const string TempFile = "tmp.torrent";
        public const string TorrentClientPath = @"C:\Program Files\qBittorrent\qbittorrent.exe";
        public static WebClient WClient = new WebClient();
        public static void DownloadAnime(string torrentLink)
        {
            try
            {
                WClient.DownloadFile(torrentLink, TempFile);
            }
            catch (Exception)
            {
                throw new Exception($"ERROR: COULD NOT DOWNLOAD FILE FROM LINK {torrentLink}");
            }
            Process.Start(TorrentClientPath, TempFile);
        }
    }
}
