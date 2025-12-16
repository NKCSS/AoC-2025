using BenchmarkDotNet.Attributes;
using Microsoft.Diagnostics.Tracing.Parsers.ClrPrivate;
using Microsoft.Diagnostics.Tracing.Parsers.FrameworkEventSource;
using Microsoft.Extensions.Logging;
using NKCSS.AoC;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Security.Cryptography.X509Certificates;

namespace AoC2025
{
    public class Day7 : Solution
    {
        const char StartMarker = 'S', SplitterMarker = '^';
        bool Test = true;
        const long AnswerP1Test = 21, AnswerP2Test = 40, AnswerP1 = 1656, AnswerP2 = 76624086587804;
        GridLocation startLocation;
        HashSet<GridLocation> splitterLocations;
        int MaxRows, MaxCol;
        HashSet<GridLocation> newBeams;
        public Day7() : base(7) {
            if (Test)
            {
                // Paste test input here...
                Input = """
                    .......S.......
                    ...............
                    .......^.......
                    ...............
                    ......^.^......
                    ...............
                    .....^.^.^.....
                    ...............
                    ....^.^...^....
                    ...............
                    ...^.^...^.^...
                    ...............
                    ..^...^.....^..
                    ...............
                    .^.^.^.^.^...^.
                    ...............
                    """;
            }
            var map = Input.ToLines();
            MaxRows = map.Length;
            MaxCol = map[0].Length;
            var parsed = map.MapAsGridLocations(StartMarker, SplitterMarker);
            startLocation = parsed[0].First();
            splitterLocations = parsed[1];
            Part1();
            Part2();
        }
        void Part1()
        {
            long p1 = 0L;
            Queue<GridLocation> beams = [];
            beams.Enqueue(startLocation);
            HashSet<GridLocation> uniqueConstraint = [startLocation];
            GridLocation beam;
            newBeams = [];
            List<GridLocation> hitlocations = [];
            while (beams.Count > 0)
            {
                beam = beams.Dequeue();
                do
                {
                    beam = beam.Down().Down();
                    if (splitterLocations.Contains(beam))
                    {
                        if (!hitlocations.Contains(beam))
                        {
                            ++p1;
                            hitlocations.Add(beam);
                            GridLocation left = beam.Left(), right = beam.Right();
                            if (left.Column >= 0 && !uniqueConstraint.Contains(left))
                            {
                                beams.Enqueue(left);
                                uniqueConstraint.Add(left);
                                newBeams.Add(left);
                            }
                            if (right.Column < MaxCol && !uniqueConstraint.Contains(right))
                            {
                                beams.Enqueue(right);
                                uniqueConstraint.Add(right);
                                newBeams.Add(right);
                            }
                        }
                        break;
                    }
                }
                while (beam.Row < MaxRows);
            }
            if (Test)
            {
                var frequency = hitlocations.GroupBy(x => x).ToDictionary(x => x.Key, x => x.Count());
                GridLocation current;
                char m;
                for (int row = 0; row < MaxRows; ++row)
                {
                    for (int col = 0; col < MaxCol; ++col)
                    {
                        current = (row, col);
                        if (frequency.TryGetValue(current, out int hits))
                        {
                            if (hits == 1) Console.ForegroundColor = ConsoleColor.Green;
                            else Console.ForegroundColor = ConsoleColor.Red;
                        }
                        else
                        {
                            Console.ForegroundColor = ConsoleColor.White;
                        }
                        if (current == startLocation) m = StartMarker;
                        else if (splitterLocations.Contains(current)) m = SplitterMarker;
                        else if (newBeams.Contains(current)) m = '|';
                        else m = '.';
                        Console.Write(m);
                    }
                    Console.WriteLine();
                }
            }
            Console.WriteLine($"Part 1: {p1}");
            Debug.Assert(p1 == (Test ? AnswerP1Test : AnswerP1), "You broke Part 1!");
        }
        // C# version of https://github.com/cheeze2000/aoc/tree/main/2025/07
        Dictionary<GridLocation, long> cache = [];
        long GetValue(GridLocation pos)
        {
            long result;
            if (!cache.TryGetValue(pos, out result))
            {
                if (pos.Row > MaxRows) result = 1L;
                else if (splitterLocations.Contains(pos)) result = GetValue(pos.Left()) + GetValue(pos.Right());
                else result = GetValue(pos.Down().Down());
                cache.Add(pos, result);
            }
            return result;
        }
        (Dictionary<GridLocation, HashSet<GridLocation>> paths, HashSet<GridLocation> exits) BuildPaths()
        {
            Queue<(GridLocation pos, GridLocation prev)> beams = [];
            beams.Enqueue((startLocation, startLocation));
            HashSet<GridLocation> uniqueConstraint = [startLocation];
            (GridLocation pos, GridLocation prev) beam;
            newBeams = [];
            Dictionary<GridLocation, HashSet<GridLocation>> Paths = [];
            HashSet<GridLocation> exits = [];
            while (beams.Count > 0)
            {
                beam = beams.Dequeue();
                do
                {
                    beam.pos = beam.pos.Down().Down();
                    if (splitterLocations.Contains(beam.pos))
                    {
                        if (!Paths.ContainsKey(beam.pos))
                        {
                            Paths.Add(beam.pos, [beam.prev]);
                            GridLocation left = beam.pos.Left(), right = beam.pos.Right();
                            if (left.Column >= 0 && !uniqueConstraint.Contains(left))
                            {
                                beams.Enqueue((left, beam.pos));
                                uniqueConstraint.Add(left);
                                newBeams.Add(left);
                            }
                            if (right.Column < MaxCol && !uniqueConstraint.Contains(right))
                            {
                                beams.Enqueue((right, beam.pos));
                                uniqueConstraint.Add(right);
                                newBeams.Add(right);
                            }
                        }
                        else Paths[beam.pos].Add(beam.prev);
                        break;
                    }
                }
                while (beam.pos.Row < MaxRows);
                exits.Add(beam.prev);
            }
            return (Paths, exits);
        }
        // Not working; would need to do it row by row to get the right numbers.
        long CountUniquePaths(GridLocation pos)
        {
            Dictionary<GridLocation, long> p = [];
            Queue<GridLocation> q = [];
            q.Enqueue(pos);
            p[pos] = 1;
            HashSet<GridLocation> exits = [], seen = [];
            while (q.Count > 0)
            {
                pos = q.Dequeue();
                long v = p[pos];
                if (pos.Row > MaxRows) exits.Add(pos);
                else if (splitterLocations.Contains(pos))
                {
                    GridLocation left = pos.Left(), right = pos.Right();
                    if (!seen.Contains(pos))
                    {
                        seen.Add(pos);
                        q.Enqueue(left);
                        q.Enqueue(right);
                    }
                    p[left] = p.TryGetValue(left, out var currentL) ? currentL + v : v;
                    p[right] = p.TryGetValue(right, out var currentR) ? currentR + v : v;
                }
                else
                {
                    var next = pos.Down().Down();
                    p[next] = v;
                    q.Enqueue(next);
                }
            }
            return exits.Sum(x => p[x]);
        }
        void Part2()
        {
            long p2 = 0L;
            var start = Stopwatch.GetTimestamp();
            p2 = GetValue(startLocation);
            var p2t = Stopwatch.GetTimestamp();
            Console.WriteLine($"Part 2: {p2}");
            Debug.Assert(p2 == (Test ? AnswerP2Test : AnswerP2), "You broke Part 2!");
            var p2Alt = CountUniquePaths(startLocation);
            var p2AltT = Stopwatch.GetTimestamp();
            Console.WriteLine($"DFS: {Stopwatch.GetElapsedTime(start, p2t)}, Pascal: {Stopwatch.GetElapsedTime(p2t, p2AltT)}");
            var paths = BuildPaths();
            Console.WriteLine($"{paths.paths.Count}");
            Debug.Assert(p2Alt == p2, "Alternative implementation is wrong");
        }
        #region For Benchmark.NET
        [Benchmark]
        public void Day7_Part1() => Part1();
        [Benchmark]
        public void Day7_Part2() => Part2();
        #endregion
    }
}
