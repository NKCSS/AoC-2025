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
        bool Test = true;
        const long AnswerP1Test = 5, AnswerP2Test = 2, AnswerP1 = 658, AnswerP2 = -1L;
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
            if (Test)
            {
                Input = """
                    svr: aaa bbb
                    aaa: fft
                    fft: ccc
                    bbb: tty
                    tty: ccc
                    ccc: ddd eee
                    ddd: hub
                    hub: fff
                    eee: dac
                    dac: fff
                    fff: ggg hhh
                    ggg: out
                    hhh: out
                    """;
            }
            PrepareInput();
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
            end = inverseLookup[End];
            if (inverseLookup.TryGetValue(Server, out svr))
            {
                dac = inverseLookup[DAC];
                fft = inverseLookup[FFT];
            }
            else
            {
                start = inverseLookup[Start];
            }
            lookup.Add(end, End);
            parsed.Add(End, []);
        }
        Dictionary<uint, long> cache = [];
        long ExploreUniquePaths(IdType start, IdType next, IdType end)
        {
            uint cacheKey = MakeKey(start, next);
            if (cache.TryGetValue(cacheKey, out long val)) return val;
            if (next == end)
            {
                return 1L;
            }
            var paths = parsed[lookup[next]];
            if (paths.Count == 0) return 0;
            long result = paths.Sum(x => ExploreUniquePaths(start, inverseLookup[x], end));
            cache.Add(cacheKey, result);
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
            if (Test)
            {
                foreach (var way in ways)
                {
                    Console.WriteLine($"{string.Join("=>", way.Select(x => lookup[x]))}");
                }
            }
            Console.WriteLine($"Part 1: {p1}");
            var alt = ExploreUniquePaths(start, start, end);
            Debug.Assert(p1 == (Test ? AnswerP1Test : AnswerP1), "You broke Part 1!");
        }
        uint MakeKey(IdType a, IdType b) => ((uint)a << 16) + (uint)b;
        void Part2()
        {
            //dac=>fft:0
            long p2 = 0L;
            List<(IdType from, IdType to)> queue = [
                (dac, fft),
                (fft, dac),
                (svr, dac),
                (svr, fft),
                (dac, end),
                (fft, end)
            ];
            List<List<IdType>> ways;
            Dictionary<uint, int> results = [];
            foreach (var q in queue)
            {
                cache.Clear();
                uint key = MakeKey(q.from, q.to);
                ways = [.. FindAllPaths(q.from, q.to)];
                long altWays = ExploreUniquePaths(q.from, q.from, q.to);
                results.Add(key, ways.Count);
                Console.WriteLine($"{lookup[q.from]}=>{lookup[q.to]}:{ways.Count} (alt: {altWays})");
            }
            p2 = (long)results[MakeKey(svr, fft)] * (long)results[MakeKey(fft, dac)] * (long)results[MakeKey(dac, end)];
            /*
            List<List<IdType>> dac2fft = [.. FindAllPaths(dac, fft)],
                fft2dac = [.. FindAllPaths(fft, dac)],
                srv2dac = [.. FindAllPaths(svr, dac)],
                srv2ftt = [.. FindAllPaths(svr, fft)],
                dac2end = [.. FindAllPaths(dac, end)],
                fft2end = [.. FindAllPaths(fft, end)];
            */
            // svr->dac,fft->out

            // valid paths: svr->dac->fft->out or svr->fft->dac->out
            // dac->fft never happens, so only path is: svr->fft->dac->out
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
