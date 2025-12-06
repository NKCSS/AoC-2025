using CommandLine;
using NKCSS.AoC;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text.RegularExpressions;

namespace AoC2025
{
    public class Day5 : Solution
    {
        bool Test = false;
        const long AnswerP1Test = 3, AnswerP2Test = 14, AnswerP1 = 840, AnswerP2 = -1L;
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
        int Cleanup()
        {
            parsed.ranges = parsed.ranges.OrderBy(x => x.from).ThenBy(x => x.to).ToList();
            int changeCount = 0;
            Dictionary<int, HashSet<int>> overlap = [];
            HashSet<string> recorded = [];
            HashSet<int> linesWeCanDelete = [];
            for (int i = 0; i < parsed.ranges.Count; ++i)
            {
                HashSet<int> overlaps = [];
                for (int j = 0; j < parsed.ranges.Count; ++j)
                {
                    if (j == i) continue;
                    string key = i < j ? $"{i}_{j}" : $"{j}_{i}";
                    if (recorded.Contains(key)) continue;
                    recorded.Add(key);
                    var x = parsed.ranges[i];
                    var y = parsed.ranges[j];
                    if (x.to > y.from && x.from < y.to)
                    {
                        // overlap
                        overlaps.Add(j);
                    }
                }
                if (overlaps.Count > 0) overlap.Add(i, overlaps);
            }
            foreach (var overlapId in overlap.Keys)
            {
                var range = parsed.ranges[overlapId];
                Console.WriteLine($"{range.from}-{range.to} (line {overlapId}) ovelaps with: ");
                foreach (int otherIx in overlap[overlapId])
                {
                    var otherRange = parsed.ranges[otherIx];
                    if (otherRange.from >= range.from && otherRange.to <= range.to)
                    {
                        // range completely covers otherrange
                        Console.WriteLine($"{otherRange.from}-{otherRange.to} (line {otherIx}; is smaller than parent, so we can delete this)");
                        linesWeCanDelete.Add(otherIx);
                        ++changeCount;
                    }
                    else
                    {
                        if (otherRange.from >= range.from && otherRange.to >= range.to)
                        {
                            if (otherRange.from == range.from)
                            {
                                // both start at the same place, but other ends later, so it covers the range.
                                linesWeCanDelete.Add(overlapId);
                                Console.WriteLine($"{otherRange.from}-{otherRange.to} (line {otherIx}; covers line {overlapId} so delete line {overlapId}) ");
                                ++changeCount;
                            }
                            else if (otherRange.to == range.to)
                            {
                                // both end at the same place, but other starts later
                                Debugger.Break();
                            }
                            else
                            {
                                // other starts and ends later.
                                parsed.ranges[overlapId] = (range.from, otherRange.from - 1);
                                Console.WriteLine($"{otherRange.from}-{otherRange.to} (line {otherIx})");
                                ++changeCount;
                            }
                        }
                        else
                        {
                            Debugger.Break();
                        }
                    }
                }
                Console.WriteLine();
            }
            foreach (int lineToDelete in linesWeCanDelete.OrderByDescending(x => x))
            {
                parsed.ranges.RemoveAt(lineToDelete);
            }
            return changeCount;
        }
        void Part2()
        {
            ulong p2 = 0UL;
            /*
            Dictionary<int, HashSet<int>> overlap = [];
            HashSet<string> recorded = [];
            HashSet<int> linesWeCanDelete = [];
            for (int i = 0; i < parsed.ranges.Count - 1; ++i)
            {
                HashSet<int> overlaps = [];
                for (int j = 0; j < parsed.ranges.Count - 1; ++j)
                {
                    if (j == i) continue;
                    string key = i < j ? $"{i}_{j}" : $"{j}_{i}";
                    if (recorded.Contains(key)) continue;
                    recorded.Add(key);
                    var x = parsed.ranges[i];
                    var y = parsed.ranges[j];
                    if (x.to > y.from && x.from < y.to)
                    {
                        // overlap
                        overlaps.Add(j);
                    }
                }
                if (overlaps.Count > 0) overlap.Add(i, overlaps);
            }
            foreach (var overlapId in overlap.Keys)
            {
                var range = parsed.ranges[overlapId];
                Console.WriteLine($"{range.from}-{range.to} (line {overlapId}) ovelaps with: ");
                foreach(int otherIx in overlap[overlapId])
                {
                    var otherRange = parsed.ranges[otherIx];
                    if (otherRange.from >= range.from && otherRange.to <= range.to)
                    {
                        // range completely covers otherrange
                        Console.WriteLine($"{otherRange.from}-{otherRange.to} (line {otherIx}; is smaller than parent, so we can delete this)");
                        linesWeCanDelete.Add(otherIx);
                    }
                    else
                    {
                        if (otherRange.from >= range.from && otherRange.to >= range.to)
                        {
                            if (otherRange.from == range.from)
                            {
                                // both start at the same place, but other ends later, so it covers the range.
                                linesWeCanDelete.Add(overlapId);
                                Console.WriteLine($"{otherRange.from}-{otherRange.to} (line {otherIx}; covers line {overlapId} so delete line {overlapId}) ");
                            }
                            else if(otherRange.to == range.to)
                            {
                                // both end at the same place, but other starts later
                                Debugger.Break();
                            }
                            else
                            {
                                // other starts and ends later.
                                parsed.ranges[overlapId] = (range.from, otherRange.from - 1);
                                Console.WriteLine($"{otherRange.from}-{otherRange.to} (line {otherIx})");
                            }
                        }
                        else
                        {
                            Debugger.Break();
                        }
                    }
                }
                Console.WriteLine();
            }
            foreach(int lineToDelete in linesWeCanDelete.OrderByDescending(x => x))
            {
                parsed.ranges.RemoveAt(lineToDelete);
            }*/
            int loopCount = 0;
            while(Cleanup() > 0 && ++loopCount < 20)
            {
                Console.WriteLine($"Pass {loopCount}...");
            }
            (ulong from, ulong to) a, b = parsed.ranges[0];
            for (int i = 0; i < parsed.ranges.Count - 1; ++i)
            {
                a = b;
                b = parsed.ranges[i + 1];
                // When it's not ordered as expected, swap
                if (a.from > b.from)
                {
                    (a, b) = (b, a);
                }
                if (a.to > b.from)
                {
                    if (b.from == a.from)
                    {
                        if (b.to == a.to) continue;
                        else
                        {
                            if (b.to > a.to) b.from = a.to + 1;
                            else
                            {
                                a.from = b.to + 1;
                                (a, b) = (b, a);
                            }
                        }
                    }
                    else a.to = b.from - 1;
                }
                p2 += (a.to - a.from) + 1UL;
            }
            p2 += (b.to - b.from) + 1UL;
            Console.WriteLine($"Part 2: {p2}");
            Debug.Assert((long)p2 == (Test ? AnswerP2Test : AnswerP2), "You broke Part 2!");
            //wrong: 349330525876391 <= too low
            //349103042666062
            //359913027576337 <= too high.
            //359913027576337
        }
    }
}
