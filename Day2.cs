using BenchmarkDotNet.Attributes;
using NKCSS.AoC;
using System.Diagnostics;
using System.Text;

namespace AoC2025
{
    public class Day2 : Solution
    {
        bool Test = false;
        const long AnswerP1Test = 1227775554, AnswerP2Test = 4174379265, AnswerP1 = 26255179562, AnswerP2 = 31680313976;
        Dictionary<int, long> Pow = [];
        Dictionary<(int startLen, int endLen), HashSet<int>> divisorCache = [];
        Dictionary<int, HashSet<int>> repeatCountInfo = [];
        HashSet<int> GetDivisors((int startLen, int endLen) range)
        {
            if (!divisorCache.TryGetValue(range, out var result))
            {
                result = [];
                for (int i = range.startLen, endLength = range.endLen; i <= endLength; ++i)
                {
                    if (repeatCountInfo.ContainsKey(i)) result.UnionWith(repeatCountInfo[i]);
                }
                divisorCache.Add(range, result);
            }
            return result;
        }
        public Day2() : base(2)
        {
            int maxLongLength = long.MaxValue.ToString().Length;
            checked
            {
                for (int i = 0; i < maxLongLength; ++i)
                {
                    Pow.Add(i, (long)Math.Pow(10, i));
                }
            }
            for (int j = 2; j < maxLongLength; ++j)
            {
                HashSet<int> d = [], r = [];
                for (int i = 1; i < maxLongLength; ++i)
                {
                    if (j % i == 0 && j / i > 1)
                    {
                        // repeat counts
                        // I mistakingly took i here, but what we need to figure out,
                        // is how many times the pattern can repeat by dividing it by this number,
                        // because that's what our other function needs.
                        r.Add(j / i);
                    }
                }
                repeatCountInfo.Add(j, r);
            }
            if (Test)
            {
                // Paste test input here...
                Input = @"11-22,95-115,998-1012,1188511880-1188511890,222220-222224,1698522-1698528,446443-446449,38593856-38593862,565653-565659,824824821-824824827,2121212118-2121212124";
            }
            Part1();
            Part2();
        }
        void Part1()
        {
            long p1 = 0L;
            List<long> invalidIds = [];
            long start, end, startMaybe;
            int startLength;
            long half;
            foreach (List<long> range in Input.Split(',').Select(x => x.Split('-').AsInt64s()))
            {
                start = range[0];
                end = range[1];
                invalidIds.AddRange(FindPossiblePatterns(start, end, 2));
            }
            p1 = invalidIds.Sum();
            Console.WriteLine($"Part 1: {p1}");
            Debug.Assert(p1 == (Test ? AnswerP1Test : AnswerP1), "You broke Part 1!");
        }
        string Repeat(string value, int count)
        {
            StringBuilder sb = new();
            for(int i = 0; i < count; ++i)
                sb.Append(value);
            return sb.ToString();
        }
        long Repeat(long value, int count)
        {
            long result = value;
            int valLen = value.ToString().Length;
            for (int i = 1; i < count; ++i)
            {
                result += value * Pow[valLen * i];
            }
            return result;
        }
        IEnumerable<long> FindPossiblePatterns(long start, long end, int repeats)
        {
            int len = start.ToString().Length, repeatLen = len / repeats;
            long half, startMaybe;
            if (len % repeats != 0)
            {
                // Round up to the next even numberd value.
                half = Pow[repeatLen];
                startMaybe = Repeat(half, repeats);
            }
            else
            {
                half = long.Parse(start.ToString()[0..repeatLen]);
                startMaybe = Repeat(half, repeats);
            }
            // we can only be out of range once, so account for it.
            if (startMaybe < start)
            {
                ++half;
                startMaybe = start = Repeat(half, repeats);
            }
            start = startMaybe;
            while (start <= end)// && half < maxHalf)
            {
                yield return start;
                ++half;
                start = Repeat(half, repeats);
            }
        }
        void Part2()
        {
            long p2 = 0L;
            List<long> invalidIds = [];
            long start, end;
            foreach (List<long> range in Input.Split(',').Select(x => x.Split('-').AsInt64s()))
            {
                start = range[0];
                end = range[1];
                HashSet<long> ids = [];
                var divisors = GetDivisors((start.ToString().Length, end.ToString().Length));
                foreach (int div in divisors)
                {
                    ids.UnionWith(FindPossiblePatterns(start, end, div));
                }
                invalidIds.AddRange(ids);
            }
            p2 = invalidIds.Sum();
            Console.WriteLine($"Part 2: {p2}");
            Debug.Assert(p2 == (Test ? AnswerP2Test : AnswerP2), "You broke Part 2!");
        }
        #region For Benchmark.NET
        [Benchmark]
        public void Day2_Part1() => Part1();
        [Benchmark]
        public void Day2_Part2() => Part2();
        #endregion
    }
}
