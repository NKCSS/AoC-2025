using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using NKCSS.AoC;
namespace AoC2025
{
    public class Day4 : Solution
    {
        bool Test = false;
        const int MaxSurroundingRolls = 4;
        const char PaperMarker = '@';
        const long AnswerP1Test = 13, AnswerP2Test = 43, AnswerP1 = 1569, AnswerP2 = -1L;
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
            Part1();
            Part2();
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
        void Part1()
        {
            long p1 = 0L;
            /*
            Dictionary<GridLocation, int> counts = [];
            foreach(var roll in paperRolls)
            {
                int count = GetSurroundingCount(paperRolls, roll.Row, roll.Column, MaxSurroundingRolls);
                counts.Add(roll, count);
            }
            for (int row = 0; row < rowCount; ++row)
            {
                for (int col = 0; col < colCount; ++col)
                {
                    Console.Write(counts.TryGetValue((row,col), out int c) ? (c < MaxSurroundingRolls ? 'X' : '@') : ".");
                }
                Console.WriteLine();
            }*/
            p1 = paperRolls.Count(roll => GetSurroundingCount(paperRolls, roll.Row, roll.Column, MaxSurroundingRolls) < MaxSurroundingRolls);
            /*
            for (int row = 0; row < rowCount; ++row)
            {
                for (int col = 0; col < colCount; ++col)
                {

                }
            }
            */
            Console.WriteLine($"Part 1: {p1}");
            Debug.Assert(p1 == (Test ? AnswerP1Test : AnswerP1), "You broke Part 1!");
        }
        void Part2()
        {
            long p2 = 0L;
            Dictionary<GridLocation, int> counts = [];
            foreach (var roll in paperRolls)
            {
                counts.Add(roll, GetSurroundingCount(paperRolls, roll.Row, roll.Column, 10));
            }
            int removed;
            do
            {
                removed = 0;
                HashSet<GridLocation> toRemove = [];
                /*
                Func<GridLocation, int> RemoveRoll = null;
                RemoveRoll = (location) => {
                    if (toRemove.Contains(location)) return 0;
                    int removeCount = 1;
                    toRemove.Add(location);
                    foreach (var neighbour in DirectionalMove)
                    {
                        GridLocation nLocation = location + neighbour.Value;
                        if (toRemove.Contains(nLocation)) continue;
                        if (counts.TryGetValue(nLocation, out int ncount))
                        {
                            if (ncount <= MaxSurroundingRolls)
                            {
                                toRemove.Add(nLocation);
                                removeCount += RemoveRoll(nLocation);
                            }
                            else counts[nLocation] = ncount - 1;
                        }
                    }
                    return removeCount;
                };*/
                foreach (var roll in counts)
                {
                    if (toRemove.Contains(roll.Key)) continue;
                    if (roll.Value < MaxSurroundingRolls)
                    {
                        //removed += RemoveRoll(roll.Key);
                        ++removed;
                        toRemove.Add(roll.Key);
                        foreach (var neighbour in DirectionalMove)
                        {
                            GridLocation nLocation = roll.Key + neighbour.Value;
                            if (counts.TryGetValue(nLocation, out int ncount))
                            {
                                counts[nLocation] = ncount - 1;
                            }
                        }
                    }
                }
                Console.WriteLine($"Removed {removed} rolls in this pass.");
                foreach (var roll in toRemove)
                    counts.Remove(roll);
                p2 += removed;

            } while (removed > 0);
            Console.WriteLine($"Part 2: {p2}");
            Debug.Assert(p2 == (Test ? AnswerP2Test : AnswerP2), "You broke Part 2!");
        }
    }
}
