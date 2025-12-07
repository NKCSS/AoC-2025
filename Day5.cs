using BenchmarkDotNet.Attributes;
using NKCSS.AoC;
using System.Diagnostics;

namespace AoC2025
{
    public class Day5 : Solution
    {
        bool Test = false;
        const long AnswerP1Test = 3, AnswerP2Test = 14, AnswerP1 = 840, AnswerP2 = 359913027576322L;
        (List<(ulong from, ulong to)> ranges, List<ulong> values) parsed;
        public Day5() : base(5) {
            if (Test)
            {
                // Paste test input here...
                Input = """
                    3-5
                    10-14
                    16-20
                    12-18

                    1
                    5
                    8
                    11
                    17
                    32
                    """;
                parsed = Input
                    .SplitBy("\r\n\r\n")
                    .As(
                        x => x.ToLines()
                            .Distinct()
                            .Select(x => 
                                x.Split('-')
                                    .AsIntu64s()
                                    .AsValueTuple()
                                )
                                .OrderBy(x => x.Item1)
                                .ThenBy(x => x.Item2)
                                .ToList(), 
                        x => x.ToLines().AsIntu64s()
                    );
            }
            else
            {
                parsed = Input
                    .SplitBy("\n\n")
                    .As(
                        x => x.ToLines()
                            .Distinct()
                            .Select(x =>
                                x.Split('-')
                                    .AsIntu64s()
                                    .AsValueTuple()
                                )
                                .OrderBy(x => x.Item1)
                                .ThenBy(x => x.Item2)
                                .ToList(),
                        x => x.ToLines().AsIntu64s()
                    );
            }
            Part1();
            Part2();
        }
        void Part1()
        {
            long p1 = 0L;
            foreach (var ingredientId in parsed.values)
            {
                foreach (var range in parsed.ranges)
                {
                    if (range.from <= ingredientId && range.to >= ingredientId)
                    {
                        ++p1;
                        break;
                    }
                    else if (ingredientId < range.from) break; // won't be any matching ranges after.
                }
            }
            Console.WriteLine($"Part 1: {p1}");
            Debug.Assert(p1 == (Test ? AnswerP1Test : AnswerP1), "You broke Part 1!");
        }
        void Part2()
        {
            ulong p2 = 0UL;
            // make sure the ranges are sorted by start
            // Keep the range open untill you find a new one that doesn't overlap
            // if they do overlap, max the end of the range (update the open range)
            (ulong from, ulong to) openRange = parsed.ranges[0], next;
            for (int i = 1, cnt = parsed.ranges.Count; i < cnt; ++i)
            {
                next = parsed.ranges[i];
                if (next.from <= openRange.to)
                {
                    openRange.to = Math.Max(next.to, openRange.to);
                }
                else
                {
                    // current open range is finalized; add it up and move on.
                    p2 += openRange.to - openRange.from + 1UL;
                    openRange = next;
                }
            }
            p2 += openRange.to - openRange.from + 1UL;
            Console.WriteLine($"Part 2: {p2}");
            Debug.Assert((long)p2 == (Test ? AnswerP2Test : AnswerP2), "You broke Part 2!");
        }
        #region For Benchmark.NET
        [Benchmark]
        public void Day5_Part1() => Part1();
        [Benchmark]
        public void Day5_Part2() => Part2();
        #endregion
    }
}
