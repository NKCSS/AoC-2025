using BenchmarkDotNet.Attributes;
using Microsoft.Diagnostics.Tracing.Parsers.ClrPrivate;
using Microsoft.Extensions.Logging;
using NKCSS.AoC;
using System.Diagnostics;
using System.Linq;

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
                    beam = beam.Down();
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
            Queue<GridLocation> beams = [];
            beams.Enqueue(startLocation);
            Dictionary<GridLocation, HashSet<GridLocation>> uniqueConstraint = [];
            uniqueConstraint.Add(startLocation, [startLocation]);
            GridLocation beam;
            newBeams = [];
            List<GridLocation> hitlocations = [];
            while (beams.Count > 0)
            {
                beam = beams.Dequeue();
                do
                {
                    beam = beam.Down();
                    if (splitterLocations.Contains(beam))
                    {
                        if (!hitlocations.Contains(beam))
                        {
                            hitlocations.Add(beam);
                            GridLocation left = beam.Left(), right = beam.Right();
                            if (left.Column >= 0)
                            {
                                if (uniqueConstraint.TryGetValue(left, out var paths))
                                {
                                    paths.Add(beam);
                                }
                                else
                                {
                                    beams.Enqueue(left);
                                    uniqueConstraint.Add(left, [beam]);
                                    newBeams.Add(left);
                                }
                            }
                            if (right.Column < MaxCol)
                            {
                                if (uniqueConstraint.TryGetValue(right, out var paths))
                                {
                                    paths.Add(beam);
                                }
                                else
                                {
                                    beams.Enqueue(right);
                                    uniqueConstraint.Add(right, [beam]);
                                    newBeams.Add(right);
                                }
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
                        if (hits == 1) Console.ForegroundColor = ConsoleColor.Green;
                        else Console.ForegroundColor = ConsoleColor.Red;
                    }
                    else
                    {
                        Console.ForegroundColor = ConsoleColor.White;
                    }
                    if (current == startLocation) m = StartMarker;
                    else if (splitterLocations.Contains(current)) m = SplitterMarker;
                    //else if (newBeams.Contains(current)) m = '|';
                    else if (uniqueConstraint.TryGetValue(current, out var ways)) m = RecursiveCount(current).ToString()[0];
                    else m = '.';
                    Console.Write(m);
                }
                Console.WriteLine();
            }
            Console.WriteLine($"Part 2: {p2}");
            Debug.Assert(p2 == (Test ? AnswerP2Test : AnswerP2), "You broke Part 2!");

            int RecursiveCount(GridLocation loc)
            {
                int result = 0;
                if (uniqueConstraint.TryGetValue(loc, out var ways))
                {
                    result += ways.Count;
                    foreach(var way in ways)
                    {
                        result += RecursiveCount(way);
                    }
                }
                return result;
            }
        }
        #region For Benchmark.NET
        [Benchmark]
        public void Day7_Part1() => Part1();
        [Benchmark]
        public void Day7_Part2() => Part2();
        #endregion
    }
}
