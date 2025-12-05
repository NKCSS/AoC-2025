using BenchmarkDotNet.Attributes;
using NKCSS.AoC;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
namespace AoC2025
{
    public class Day4 : Solution
    {
        bool Test = false;
        const int MaxSurroundingRolls = 4;
        const char PaperMarker = '@';
        const long AnswerP1Test = 13, AnswerP2Test = 43, AnswerP1 = 1569, AnswerP2 = 9280;
        HashSet<GridLocation> paperRolls;
        int rowCount, colCount;
        public Day4() : base(4) {
            if (Test)
            {
                // Paste test input here...
                Input = """
            ..@@.@@@@.
            @@@.@.@.@@
            @@@@@.@.@@
            @.@@@@..@.
            @@.@@@@.@@
            .@@@@@@@.@
            .@.@.@.@@@
            @.@@@.@@@@
            .@@@@@@@@.
            @.@.@@@.@.
            """;
            }
            var rows = Input.ToLines();
            rowCount = rows.Length;
            colCount = rows[0].Length;
            paperRolls = rows.MapAsGridLocations(PaperMarker)[0];
            var start = Stopwatch.GetTimestamp();
            Part1();
            var p1Time = Stopwatch.GetTimestamp();            
            Part2();
            var p2Time = Stopwatch.GetTimestamp();
            Console.WriteLine($"P1: {Stopwatch.GetElapsedTime(start, p1Time)}");
            Console.WriteLine($"P2: {Stopwatch.GetElapsedTime(p1Time, p2Time)}");
        }
        Dictionary<Direction, (int row, int col)> DirectionalMove = new() {
            { Direction.Up, (-1, 0) },
            { Direction.Right, (0, 1) },
            { Direction.Down, (1, 0) },
            { Direction.Left, (0, -1) },
            { Direction.UpLeft, (-1, -1) },
            { Direction.UpRight, (-1, 1) },
            { Direction.DownLeft, (1, -1) },
            { Direction.DownRight, (1, 1) },
        };
        /// <summary>
        /// Version with early-exit via <paramref name="exitAfter"/> />
        /// </summary>
        int GetSurroundingCount(HashSet<GridLocation> paperRolls, int row, int col, int exitAfter)
        {
            int count = 0;
            foreach (var dir in DirectionalMove)
            {
                if (paperRolls.Contains((row + dir.Value.row, col + dir.Value.col)))
                {
                    if(++count >= exitAfter) return count;
                }
            }
            return count;
        }
        int GetSurroundingCount(HashSet<GridLocation> paperRolls, int row, int col)
        {
            int count = 0;
            foreach (var dir in DirectionalMove)
            {
                if (paperRolls.Contains((row + dir.Value.row, col + dir.Value.col)))
                {
                    ++count;
                }
            }
            return count;
        }
        void Part1()
        {
            long p1 = paperRolls.Count(roll => GetSurroundingCount(paperRolls, roll.Row, roll.Column, MaxSurroundingRolls) < MaxSurroundingRolls);
            Console.WriteLine($"Part 1: {p1}");
            Debug.Assert(p1 == (Test ? AnswerP1Test : AnswerP1), "You broke Part 1!");
        }
        void Part2()
        {
            long p2 = 0L;
            Dictionary<GridLocation, int> counts = [];
            foreach (var roll in paperRolls)
            {
                counts.Add(roll, GetSurroundingCount(paperRolls, roll.Row, roll.Column));
            }
            HashSet<GridLocation> removed = [];
            foreach (var roll in counts)
            {
                if (removed.Contains(roll.Key)) continue;
                if (roll.Value < MaxSurroundingRolls)
                {
                    p2 += RemoveRoll(roll.Key);
                }
            }
            Console.WriteLine($"Part 2: {p2}");
            Debug.Assert(p2 == (Test ? AnswerP2Test : AnswerP2), "You broke Part 2!");


            int RemoveRoll(GridLocation location)
            {
                if (removed.Contains(location)) return 0;
                int removeCount = 1;
                removed.Add(location);
                foreach (var neighbour in DirectionalMove)
                {
                    GridLocation nLocation = location + neighbour.Value;
                    if (removed.Contains(nLocation)) continue;
                    if (counts.TryGetValue(nLocation, out int ncount))
                    {
                        if (ncount <= MaxSurroundingRolls)
                        {
                            removeCount += RemoveRoll(nLocation);
                        }
                        else counts[nLocation] = ncount - 1;
                    }
                }
                return removeCount;
            };
        }
    }
}