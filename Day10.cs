using BenchmarkDotNet.Attributes;
using Microsoft.Diagnostics.Tracing.Analysis.GC;
using Microsoft.Extensions.Options;
using Microsoft.VisualBasic;
using NKCSS.AoC;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
namespace AoC2025
{
    public class Day10 : Solution
    {
        bool Test = false;
        const long AnswerP1Test = 7, AnswerP2Test = 33, AnswerP1 = 475, AnswerP2 = -1L;
        const char LightOnTarget = '#', LightOff = '.';
        //2 + 3 + 2 = 7.
        //The manual describes one machine per line. Each line contains a single indicator 
        //light diagram in [square brackets], one or more button wiring schematics in (parentheses),
        // and joltage requirements in {curly braces}.
        //[.##.] (3) (1,3) (2) (2,3) (0,2) (0,1) {3,5,4,7}
        public record Machine
        {
            public required bool[] lightDiagram { get; set; }
            public required bool[] state { get; set; }
            public required short[] joltage { get; set; }
            public required List<List<int>> wireingSchematics { get; set; }
            public required short[] joltageRequirements { get; set; }
            [SetsRequiredMembers]
            public Machine(string raw)
            {
                string[] parts = [.. raw.SplitByASCIIWhiteSpace()];
                string diagram = parts[0];
                this.lightDiagram = new bool[diagram.Length - 2];
                for (int i = 1, len = diagram.Length; i < len - 1; ++i)
                {
                    lightDiagram[i - 1] = diagram[i] == LightOnTarget;
                }
                joltageRequirements = [..parts[parts.Length -1][1..^1].Split(',').AsInt32s().Select(x => (short)x)];
                wireingSchematics = new();
                for (int i = 1, len = parts.Length; i < len - 1; ++i)
                {
                    wireingSchematics.Add(parts[i][1..^1].Split(',').AsInt32s());
                }
                // default state == off
                ResetState();
            }
            public string AsStateRaw(bool[] values) => $"{string.Join(string.Empty, values.Select(x => x ? LightOnTarget : LightOff))}";
            public void ResetState() => state = new bool[lightDiagram.Length];
            public void ResetJoltageState() => joltage = new short[lightDiagram.Length];
            public void Push(int index)
            {
                foreach (int indexToToggle in wireingSchematics[index])
                {
                    this.state[indexToToggle] = !this.state[indexToToggle];
                }
            }
            public void PushJoltage(int index)
            {
                foreach (int indexToToggle in wireingSchematics[index])
                {
                    ++this.joltage[indexToToggle];
                }
            }
            public void Push(IEnumerable<int> indexes)
            {
                foreach (int index in indexes)
                    Push(index);
            }
            public string GetState() => AsStateRaw(state);
            public List<List<int>> GetPossibleButtonCombos(int? cap = null)
            {
                List<List<int>> uniqueCombos = [];
                List<int> index = [..Enumerable.Range(0, wireingSchematics.Count)];
                cap ??= wireingSchematics.Count;
                for (int i = 1, len = cap.Value; i <= len; ++i)
                {
                    uniqueCombos.AddRange([.. index.GetPermutations(i, allowDupe: false).Select(x => x.ToList())]);
                }
                return uniqueCombos;
            }
            public Dictionary<string, List<List<int>>> MapAllOutComes()
            {
                List<List<int>> uniqueCombos = [..GetPossibleButtonCombos().OrderBy(x => x.Count)];
                Dictionary<string, List<List<int>>> result = [];
                foreach (var combo in uniqueCombos)
                {
                    ResetState();
                    foreach (var buttonIndex in combo)
                        Push(buttonIndex);
                    string state = GetState();
                    if (!result.TryGetValue(state, out var solutions))
                    {
                        solutions = [];
                        result.Add(state, solutions);
                    }
                    solutions.Add(combo);
                }
                return result;
            }
            public List<(short[] joltages, List<List<int>> solutions)> MapAllJoltageOutComes(int? cap = null)
            {
                List<List<int>> uniqueCombos = [.. GetPossibleButtonCombos(cap).OrderBy(x => x.Count)];
                Dictionary<string, (short[] joltages, List<List<int>> solutions)> result = [];
                foreach (var combo in uniqueCombos)
                {
                    ResetJoltageState();
                    foreach (var buttonIndex in combo)
                        PushJoltage(buttonIndex);
                    short[] joltage = [.. this.joltage];
                    string state = string.Join(",", joltage);
                    if (!result.TryGetValue(state, out var solutions))
                    {
                        solutions = (joltage, []);
                        result.Add(state, solutions);
                    }
                    solutions.solutions.Add(combo);
                }
                return [..result.Values];
            }
            public int Solve()
            {
                var options = MapAllOutComes();
                string desiredState = AsStateRaw(lightDiagram);
                if (options.TryGetValue(desiredState, out var solutions))
                {
                    return solutions.OrderBy(x => x.Count).First().Count;
                }
                else
                {
                    return -1;
                }
            }
            public long SolveJoltage()
            {
                var options = MapAllJoltageOutComes(1);
                List<(short[] joltages, int count)> bestOptions = [];
                foreach (var option in options)
                {
                    var best = option.solutions.OrderBy(x => x.Count).First();
                    bestOptions.Add((option.joltages, best.Count));
                }
                int numberOfOptions = bestOptions.Count;
                //Dictionary<string, (Dictionary<int, int> optionsExplored, byte[] joltages, long count)> cache = [];
                HashSet<string> cache = [];
                PriorityQueue<(short[] joltages, Dictionary<int, int> optionsExplored, long count), long> q = new();
                q.Enqueue((new short[lightDiagram.Length], [], 0L), 0L);
                int joltageCount = this.joltage.Length;
                while (q.Count > 0)
                {
                    var candidate = q.Dequeue();
                    if (candidate.joltages.SequenceEqual(this.joltageRequirements))
                    {
                        // match found
                        return candidate.count;
                    }
                    for (int i = 0; i < numberOfOptions; ++i)
                    {
                        var option = bestOptions[i];
                        Dictionary<int, int> localOptionsExplored = candidate.optionsExplored.ToDictionary();
                        if (!localOptionsExplored.TryGetValue(i, out int optionCount))
                        {
                            localOptionsExplored.Add(i, 1);
                        }
                        else localOptionsExplored[i] = optionCount + 1;
                        string key = string.Join(",",localOptionsExplored.Keys.OrderBy(x => x).Select(x => $"{x}:{localOptionsExplored[x]}"));
                        if (!cache.Contains(key))
                        {
                            short[] newJoltage = [.. candidate.joltages.WithIndexes().Select((x) => (short)(x.value + option.joltages[x.index]))];
                            bool enqueue = true;
                            for (int j = 0; j < joltageCount; ++j)
                            {
                                if (newJoltage[j] > joltageRequirements[j])
                                {
                                    enqueue = false;
                                    break;
                                }
                            }
                            if (enqueue) q.Enqueue((newJoltage, localOptionsExplored, candidate.count + option.count), candidate.count + option.count);
                            //cache.Add(key, (localOptionsExplored, candidate.joltages, candidate.count));
                            cache.Add(key);
                        }
                    }
                }
                return -1L;
            }
            public override string ToString()
            => $"[{AsStateRaw(lightDiagram)}] ({string.Join(") (", wireingSchematics.Select(x => string.Join(",", x.ToArray())))}) {{{string.Join(",", joltageRequirements)}}}";
        }
        List<Machine> machines = [];
        public Day10() : base(10) {
            if (Test)
            {
                // Paste test input here...
                Input = """
                    [.##.] (3) (1,3) (2) (2,3) (0,2) (0,1) {3,5,4,7}
                    [...#.] (0,2,3,4) (2,3) (0,4) (0,1,2) (1,2,3,4) {7,5,12,7,2}
                    [.###.#] (0,1,2,3,4) (0,3,4) (0,1,2,4,5) (1,2) {10,11,11,5,10,5}
                    """;
            }
            foreach (string line in Input.ToLines())
            {
                Machine m = new(line);
                machines.Add(m);
                if(!m.ToString().Equals(line))
                {
                    Console.WriteLine($"Parse mismatch! '{line}' -> '{m.ToString()}'");
                }
                //var combos = m.GetPossibleButtonCombos();
                //Console.WriteLine($"We have {combos.Count} unique ways of pushing these {m.wireingSchematics.Count} buttons.");
            }
            //if (Test)
            //{
            //    var machine = machines[0];
            //    var options = machine.MapAllOutComes();
            //    string desiredState = machine.AsStateRaw(machine.lightDiagram);
            //    if (options.TryGetValue(desiredState, out var solutions))
            //    {
            //        Console.WriteLine($"We can get to [{desiredState}] {solutions.Count} way(s)");
            //        foreach (var solution in solutions.OrderBy(x => x.Count))
            //        {
            //            machine.ResetState();
            //            machine.Push(solution);
            //            Console.WriteLine($"{solution.Count} press(es): {string.Join(",", solution)} [{machine.GetState()}]");
            //        }
            //    } 
            //    else
            //    {
            //        Console.WriteLine($"Could not solve!");
            //    }
            //    /*
            //    Console.WriteLine($"Initial state: {machine.GetState()}");
            //    List<int> buttonsToPush = [1,3,5,5];
            //    foreach(int buttonToPush in buttonsToPush)
            //    {
            //        machine.Push(buttonToPush);
            //        Console.WriteLine($"state after pushing ({string.Join(",", machine.wireingSchematics[buttonToPush])}): {machine.GetState()}");
            //    }*/
            //}
            Part1();
            Part2();
        }
        void Part1()
        {
            long p1 = 0L;
            p1 = machines.Sum(m => m.Solve());
            Console.WriteLine($"Part 1: {p1}");
            Debug.Assert(p1 == (Test ? AnswerP1Test : AnswerP1), "You broke Part 1!");
        }
        void Part2()
        {
            long p2 = 0L;
            foreach (var machine in machines)
            {
                var options = machine.MapAllJoltageOutComes(1);
                List<(List<int> best, short[] joltages, double efficiency)> bestOptions = [
                    ..options
                        .Select(x => ( best: x.solutions.OrderByDescending(y => y.Count).First(), joltages: x.joltages))
                        .Select(x => (x.best, x.joltages, efficiency: x.joltages.Sum(x => x) / (double)x.best.Count))
                        .OrderByDescending(x => x.efficiency)
                    ];
                foreach(var option in bestOptions)
                {
                    Console.WriteLine($"We can achieve {string.Join(",", option.joltages)} in {option.best.Count} ways; best = {string.Join(",", option.best)}. Efficency: {option.efficiency:F2}");
                }
                Console.WriteLine();
                Console.WriteLine();
                /*
                var answer = machine.SolveJoltage();
                p2 += answer;
                Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] {answer}");
                */
            }
            /*
            var machine = machines[0];
            var options = machine.MapAllJoltageOutComes();
            foreach(var option in options)
            {
                var best = option.solutions.OrderBy(x => x.Count).First();
                //Console.WriteLine($"We can achieve {string.Join(",", option.joltages)} in {option.solutions.Count} ways ({string.Join(" | ", option.solutions.Select(x => string.Join(",", x)))})");
                Console.WriteLine($"We can achieve {string.Join(",", option.joltages)} in {option.solutions.Count} ways; best = {string.Join(",", best)}. Efficency: {option.joltages.Sum(x => x) / (double)best.Count:F2}");
            }
            */
            /*
            Console.WriteLine($"Initial state: {machine.GetState()}");
            List<int> buttonsToPush = [1,3,5,5];
            foreach(int buttonToPush in buttonsToPush)
            {
                machine.Push(buttonToPush);
                Console.WriteLine($"state after pushing ({string.Join(",", machine.wireingSchematics[buttonToPush])}): {machine.GetState()}");
            }*/
            Console.WriteLine($"Part 2: {p2}");
            Debug.Assert(p2 == (Test ? AnswerP2Test : AnswerP2), "You broke Part 2!");
        }
        #region For Benchmark.NET
        [Benchmark]
        public void Day9_PrepareInput() => Part1();
        [Benchmark]
        public void Day10_Part1() => Part1();
        [Benchmark]
        public void Day10_Part2() => Part2();
        #endregion
    }
}
