using BenchmarkDotNet.Attributes;
using Microsoft.Diagnostics.Tracing.Parsers.ClrPrivate;
using Microsoft.Extensions.Logging;
using NKCSS.AoC;
using System.Diagnostics;
using System.Linq;
using System.Security.Cryptography.X509Certificates;

namespace AoC2025
{
    public class Day7 : Solution
    {
        const char StartMarker = 'S', SplitterMarker = '^';
        bool Test = true;
        const long AnswerP1Test = 21, AnswerP2Test = 40, AnswerP1 = 1656, AnswerP2 = -1L;
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
                        if(hits == 1) Console.ForegroundColor = ConsoleColor.Green;
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
            Console.WriteLine($"Part 1: {p1}");
            Debug.Assert(p1 == (Test ? AnswerP1Test : AnswerP1), "You broke Part 1!");
        }
        void Part2()
        {
            long p2 = 0L;
            Console.WriteLine($"Part 2: {p2}");
            // Find all the unique ways to get to the bottom.
            // Record connections between splitters (e.g. branches)
            HashSet<GridLocation> uniqueConstraint = [startLocation];
            GridLocation beam;
            newBeams = [];
            Queue<(GridLocation current, List<GridLocation> path)> q = [];
            List<GridLocation> hitlocations = [];
            (GridLocation pos, List<GridLocation> path) current;
            q.Enqueue((startLocation, [startLocation]));
            HashSet<string> uniquePaths = [];
            int c = 0;
            while (q.Count > 0)
            {
                current = q.Dequeue();
                beam = current.pos;
                do
                {
                    beam = beam.Down();
                    if (splitterLocations.Contains(beam))
                    {
                        string path = string.Join("|", [.. current.path, beam]);
                        if (!uniquePaths.Contains(path))
                        {
                            uniquePaths.Add(path);
                            GridLocation left = beam.Left(), right = beam.Right();
                            if (left.Column >= 0)
                            {
                                q.Enqueue((left, [.. current.path, beam]));
                                newBeams.Add(left);
                            }
                            if (right.Column < MaxCol)
                            {
                                q.Enqueue((right, [.. current.path, beam]));
                                newBeams.Add(right);
                            }
                        }
                        break;
                    }
                }
                while (beam.Row < MaxRows);
                if (beam.Row == MaxRows)
                {
                    // Reached the bottom
                    ++c;
                    //Console.WriteLine($"Path to the end: {string.Join("->", current.path)}");
                }
            }
            Console.WriteLine($"Printed {c} paths...");
            Debug.Assert(p2 == (Test ? AnswerP2Test : AnswerP2), "You broke Part 2!");
        }
        #region For Benchmark.NET
        [Benchmark]
        public void Day7_Part1() => Part1();
        [Benchmark]
        public void Day7_Part2() => Part2();
        #endregion
    }
}
