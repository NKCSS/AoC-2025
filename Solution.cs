using System.Net;

namespace AoC2025
{
    public class Solution
    {
        const string BaseUrl = "https://adventofcode.com/2025/day";
        const string InputSuffix = "input";
        static readonly string session = System.IO.File.ReadAllText($"{nameof(session)}.txt");
        public int Day { get; private set; }
        public string Input { get; set; }
        public Solution(int day)
        {
            Day = day;
            Input = GetInput(Day);
        }
        string GetInput(int day)
        {
            string cacheFile = $"cached_day_{day}.txt";
            if ((System.IO.File.Exists(cacheFile))) return System.IO.File.ReadAllText(cacheFile);
            else
            {
                var wc = new WebClient();
                wc.Headers.Add(HttpRequestHeader.UserAgent, "https://nick.yt/ | https://t.me/NicksTechdom | https://twitter.com/NKCSS");
                wc.Headers.Add(HttpRequestHeader.Cookie, $"{nameof(session)}={session}");
                string contents = wc.DownloadString($"{BaseUrl}/{day}/{InputSuffix}");
                System.IO.File.WriteAllText(cacheFile, contents);
                return contents;
            }
        }
    }
}
