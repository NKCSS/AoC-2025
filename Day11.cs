using AoC2025;
using BenchmarkDotNet.Attributes;
using NKCSS.AoC;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.Serialization;
using IdType = short;
namespace AoC2025
{
    public class Day11 : Solution
    {
        const string Start = "you", End = "out", Server = "svr", DAC = "dac", FFT = "fft";
        bool Test = false;
        const long AnswerP1Test = 5, AnswerP2Test = -1, AnswerP1 = 658, AnswerP2 = -1L;
        Dictionary<string, HashSet<string>> parsed;
        IdType start, end, svr, dac, fft;
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
        Dictionary<IdType, string> lookup;
        Dictionary<string, IdType> inverseLookup;
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
            lookup = [];
            foreach (var x in parsed.Keys.WithIndexes())
            {
                lookup.Add((IdType)x.index, x.value);
            }
            inverseLookup = lookup.ToDictionary(x => x.Value, x => x.Key);
            inverseLookup.Add(End, (IdType)inverseLookup.Count);
            start = inverseLookup[Start];
            end = inverseLookup[End];
            svr = inverseLookup[Server];
            dac = inverseLookup[DAC];
            fft = inverseLookup[FFT];
            lookup.Add(end, End);
            parsed.Add(End, []);
        }
        Dictionary<string, long> cache = [];
        long ExploreUniquePaths(List<IdType> path, IdType next, IdType end)
        {
            string cacheKey = $"{string.Join(",", path)},{next}";
            if (cache.TryGetValue(cacheKey, out long val)) return val;
            if (next == end)
            {
                return 1L;
            }
            long result = parsed[lookup[next]].Sum(x => ExploreUniquePaths([.. path, next], inverseLookup[x], end));
            cache[cacheKey] = result;
            return result;
        }
        IEnumerable<List<IdType>> FindAllPaths(IdType from, IdType to)
        {
            Queue<(IdType current, List<IdType> path, HashSet<IdType> visited)> q = [];
            q.Enqueue((from, [], [from]));
            (IdType current, List<IdType> path, HashSet<IdType> visited) next;
            while (q.TryDequeue(out next))
            {
                //if (visited.Contains(next.current)) continue;
                //visited.Add(next.current);
                if (next.current == to)
                {
                    yield return [.. next.path, next.current];
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
        }
        void Part1()
        {
            long p1 = 0L;
            // find all unique ways to go from start to end.
            List<List<IdType>> ways = [.. FindAllPaths(start, end)];
            p1 = ways.Count;
            if(Test)
            {
                foreach (var way in ways)
                {
                    Console.WriteLine($"{string.Join("=>", way.Select(x => lookup[x]))}");
                }
            }
            Console.WriteLine($"Part 1: {p1}");
            var alt = ExploreUniquePaths([], start, end);
            Debug.Assert(p1 == (Test ? AnswerP1Test : AnswerP1), "You broke Part 1!");
        }
        void Part2()
        {
            //dac=>fft:0
            long p2 = 0L;
            List<(IdType from, IdType to)> queue = [
                (dac, fft),
                //(fft, dac),
                //(svr, dac),
                (svr, fft),
                (dac, end),
                (fft, end)
            ];
            //List<List<IdType>> ways;
            foreach (var q in queue)
            {
                //ways = [.. FindAllPaths(q.from, q.to)];
                long ways = ExploreUniquePaths([], q.from, q.to);
                Console.WriteLine($"{lookup[q.from]}=>{lookup[q.to]}:{ways}");
            }
            /*
            List<List<IdType>> dac2fft = [.. FindAllPaths(dac, fft)],
                fft2dac = [.. FindAllPaths(fft, dac)],
                srv2dac = [.. FindAllPaths(svr, dac)],
                srv2ftt = [.. FindAllPaths(svr, fft)],
                dac2end = [.. FindAllPaths(dac, end)],
                fft2end = [.. FindAllPaths(fft, end)];
            */
            // svr->dac,fft->out

            Console.WriteLine($"");
            
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
