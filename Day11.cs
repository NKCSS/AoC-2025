using BenchmarkDotNet.Attributes;
using System.Diagnostics;
using AoC2025;
using NKCSS.AoC;
using System.Runtime.Serialization;
using IdType = short;
namespace AoC2025
{
    public class Day11 : Solution
    {
        const string Start = "you", End = "out";
        bool Test = false;
        const long AnswerP1Test = 5, AnswerP2Test = -1, AnswerP1 = 658, AnswerP2 = -1L;
        Dictionary<string, HashSet<string>> parsed;
        public Day11() : base(11) {
            if (Test)
            {
                // Paste test input here...
                Input = """
                    aaa: you hhh
                    you: bbb ccc
                    bbb: ddd eee
                    ccc: ddd eee fff
                    ddd: ggg
                    eee: out
                    fff: out
                    ggg: out
                    hhh: ccc fff iii
                    iii: out
                    """;
            }
            PrepareInput();
            Part1();
            Part2();
        }
        void PrepareInput()
        {
            parsed = Input
                .ToLines()
                .Select(x => 
                    x.SplitBy(": ")
                        .As(
                            x => x, 
                            x => new HashSet<string>(x.SplitByASCIIWhiteSpace()))
                        )
                        .ToDictionary(x => x.Item1, x => x.Item2);
        }
        void Part1()
        {
            long p1 = 0L;
            // find all unique ways to go from start to end.
            Dictionary<IdType, string> lookup = [];
            Dictionary<string, IdType> inverseLookup;
            //HashSet<byte> visited = [];
            foreach (var x in parsed.Keys.WithIndexes())
            {
                lookup.Add((IdType)x.index, x.value);
            }
            inverseLookup = lookup.ToDictionary(x => x.Value, x => x.Key);
            inverseLookup.Add(End, (IdType)inverseLookup.Count);
            Queue<(IdType current, List<IdType> path, HashSet<IdType> visited)> q = [];
            IdType start = inverseLookup[Start], end = inverseLookup[End];
            lookup.Add(end, End);
            q.Enqueue((start, [], [start]));
            (IdType current, List<IdType> path, HashSet<IdType> visited) next;
            List<List<IdType>> ways = [];
            while (q.TryDequeue(out next))
            {
                //if (visited.Contains(next.current)) continue;
                //visited.Add(next.current);
                if (next.current == end)
                {
                    ways.Add([..next.path, next.current]);
                    continue;
                }
                foreach (string output in parsed[lookup[next.current]])
                {
                    IdType n = inverseLookup[output];
                    if (!next.visited.Contains(n))
                    {
                        q.Enqueue((n, [.. next.path, next.current], [.. next.visited, next.current]));
                    }
                }
            }
            p1 = ways.Count;
            if(Test)
            {
                foreach (var way in ways)
                {
                    Console.WriteLine($"{string.Join("=>", way.Select(x => lookup[x]))}");
                }
            }
            Console.WriteLine($"Part 1: {p1}");
            Debug.Assert(p1 == (Test ? AnswerP1Test : AnswerP1), "You broke Part 1!");
        }
        void Part2()
        {
            long p2 = 0L;
            // Add implementation here...
            Console.WriteLine($"Part 2: {p2}");
            Debug.Assert(p2 == (Test ? AnswerP2Test : AnswerP2), "You broke Part 2!");
        }
        #region For Benchmark.NET
        [Benchmark]
        public void Day11_PrepareInput() => PrepareInput();
        [Benchmark]
        public void Day11_Part1() => Part1();
        [Benchmark]
        public void Day11_Part2() => Part2();
        #endregion
    }
}
