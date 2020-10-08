using System;

namespace AnimeDownloader
{
    class WatchListItem : IEquatable<WatchListItem>, IComparable<WatchListItem>
    {
        public string Title { get; set; }
        public int LatestEpisode { get; set; }
        public bool IsDownloaded { get; set; }
        public bool FinishedAiring { get; set; }
        public DateTime ReleaseDay { get; set; }
        public string LatestEpisodeLink { get; set; }

        public int CompareTo(WatchListItem other)
        {
            // Handle animes with no episodes in the downloaded list
            if (other.FinishedAiring)
                return -1;
            else if (FinishedAiring)
                return 1;

            // Handle found animes
            if (ReleaseDay.ToShortDateString() == other.ReleaseDay.ToShortDateString())
                return ReleaseDay.TimeOfDay.CompareTo(other.ReleaseDay.TimeOfDay);
            return ReleaseDay.Date.CompareTo(other.ReleaseDay.Date);
        }
        public bool Equals(WatchListItem other) => other.Title == Title;
    }
}
