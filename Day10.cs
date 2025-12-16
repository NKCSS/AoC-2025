using BenchmarkDotNet.Attributes;
using Microsoft.Diagnostics.Tracing.Analysis.GC;
using Microsoft.Extensions.Options;
using Microsoft.VisualBasic;
using NKCSS.AoC;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using FracType = short;
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
                joltageRequirements = [.. parts[parts.Length - 1][1..^1].Split(',').AsInt32s().Select(x => (short)x)];
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
                List<int> index = [.. Enumerable.Range(0, wireingSchematics.Count)];
                cap ??= wireingSchematics.Count;
                for (int i = 1, len = cap.Value; i <= len; ++i)
                {
                    uniqueCombos.AddRange([.. index.GetPermutations(i, allowDupe: false).Select(x => x.ToList())]);
                }
                return uniqueCombos;
            }
            public Dictionary<string, List<List<int>>> MapAllOutComes()
            {
                List<List<int>> uniqueCombos = [.. GetPossibleButtonCombos().OrderBy(x => x.Count)];
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
                return [.. result.Values];
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
                        string key = string.Join(",", localOptionsExplored.Keys.OrderBy(x => x).Select(x => $"{x}:{localOptionsExplored[x]}"));
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
        public Day10() : base(10)
        {
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
                if (!m.ToString().Equals(line))
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
            const bool PrintBest = false, TrySolve = false;
            if (PrintBest)
                foreach (var machine in machines)
                {
                    var options = machine.MapAllJoltageOutComes(1);
                    List<(List<int> best, short[] joltages, double efficiency)> bestOptions = [
                        ..options
    .Select(x => ( best: x.solutions.OrderByDescending(y => y.Count).First(), joltages: x.joltages))
    .Select(x => (x.best, x.joltages, efficiency: x.joltages.Sum(x => x) / (double)x.best.Count))
    .OrderByDescending(x => x.efficiency)
                        ];
                    foreach (var option in bestOptions)
                    {
                        Console.WriteLine($"We can achieve {string.Join(",", option.joltages)} in {option.best.Count} ways; best = {string.Join(",", option.best)}. Efficency: {option.efficiency:F2}");
                    }
                    Console.WriteLine();
                    Console.WriteLine();
                }
            if (TrySolve)
                foreach (var machine in machines)
                {
                    var answer = machine.SolveJoltage();
                    p2 += answer;
                    Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] {answer}");
                }

            foreach (var machine in machines)
            {
                List<short[]> singlePressOptions = [.. machine.MapAllJoltageOutComes(1).Select(o => o.joltages)];
                long minPresses = JoltageSolver.SolveMinimumPresses(singlePressOptions, machine.joltageRequirements, freeVarEnumerateLimit: 3); // Not sure why, but 3 is the lowest value I can pass here, though there is no performance benefit over passing 20.
                p2 += minPresses;
                //Console.WriteLine($"{machine.ToString()} => {minPresses}");
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
    #region ChatGPT
    public readonly struct Fraction<T> where T : INumber<T>
    {
        public readonly T Num;
        public readonly T Den; // positive

        public static readonly Fraction<T> Zero = new Fraction<T>(T.Zero, T.One);
        public static readonly Fraction<T> One = new Fraction<T>(T.One, T.One);

        public Fraction(T num, T den)
        {
            if (den == T.Zero) throw new DivideByZeroException();
            if (num == T.Zero) { Num = T.Zero; Den = T.One; return; }
            if (den < T.Zero) { num = -num; den = -den; }
            T g = Gcd(num < T.Zero ? -num : num, den);
            Num = num / g;
            Den = den / g;
        }
        public Fraction(T n) : this(n, T.One) { }

        static T Gcd(T a, T b)
        {
            if (a == T.Zero) return b;
            if (b == T.Zero) return a;
            while (b != T.Zero) { T t = a % b; a = b; b = t; }
            return a < T.Zero ? -a : a;
        }

        public static Fraction<T> operator +(Fraction<T> a, Fraction<T> b)
            => new Fraction<T>(a.Num * b.Den + b.Num * a.Den, a.Den * b.Den);
        public static Fraction<T> operator -(Fraction<T> a, Fraction<T> b)
            => new Fraction<T>(a.Num * b.Den - b.Num * a.Den, a.Den * b.Den);
        public static Fraction<T> operator *(Fraction<T> a, Fraction<T> b)
            => new Fraction<T>(a.Num * b.Num, a.Den * b.Den);
        public static Fraction<T> operator /(Fraction<T> a, Fraction<T> b)
        {
            if (b.Num == T.Zero) throw new DivideByZeroException();
            return new Fraction<T>(a.Num * b.Den, a.Den * b.Num);
        }
        public static Fraction<T> operator -(Fraction<T> a) => new Fraction<T>(-a.Num, a.Den);
        public override string ToString() => Den == T.One ? $"{Num}" : $"{Num}/{Den}";
        public bool IsZero => Num == T.Zero;
        public bool Equals(Fraction<T> other) => Num == other.Num && Den == other.Den;
    }

    /// <summary>
    /// Solve integer non-negative solution minimizing sum(x_i) for A x = b with non-negative integer A entries.
    /// options: List of column-vectors (each short[] length = n rows).
    /// target: short[] length n.
    /// freeVarEnumerateLimit: maximum allowed free variables to enumerate (safety).
    /// Returns minimal number of presses or -1 if impossible.
    /// </summary>
    public static class JoltageSolver
    {
        public static long SolveMinimumPresses(List<short[]> options, short[] target, int freeVarEnumerateLimit = 22)
        {
            if (options == null) throw new ArgumentNullException(nameof(options));
            if (target == null) throw new ArgumentNullException(nameof(target));
            int rows = target.Length;
            int cols = options.Count;
            if (cols == 0) return target.All(t => t == 0) ? 0 : -1;

            // Build augmented matrix M[row, 0..cols-1] = A, M[row, cols] = b
            var M = new Fraction<FracType>[rows, cols + 1];
            for (int i = 0; i < rows; ++i)
            {
                M[i, cols] = new Fraction<FracType>(target[i], 1);
            }
            for (int j = 0; j < cols; ++j)
            {
                var vec = options[j];
                if (vec.Length != rows) throw new ArgumentException("option vector length mismatch");
                for (int i = 0; i < rows; ++i)
                    M[i, j] = new Fraction<FracType>(vec[i], 1);
            }

            // Gauss-Jordan -> RREF
            int r = 0;
            int[] pivotRowOfCol = Enumerable.Repeat(-1, cols).ToArray();
            int[] pivotColOfRow = Enumerable.Repeat(-1, rows).ToArray();

            for (int c = 0; c < cols && r < rows; ++c)
            {
                int sel = -1;
                for (int i = r; i < rows; ++i) if (!M[i, c].IsZero) { sel = i; break; }
                if (sel == -1) continue;

                // swap rows sel <-> r
                if (sel != r)
                {
                    for (int j = c; j <= cols; ++j)
                    {
                        var tmp = M[sel, j]; M[sel, j] = M[r, j]; M[r, j] = tmp;
                    }
                }

                // normalize pivot row
                var pivot = M[r, c];
                for (int j = c; j <= cols; ++j) M[r, j] = M[r, j] / pivot;

                // eliminate other rows
                for (int i = 0; i < rows; ++i)
                {
                    if (i == r) continue;
                    var factor = M[i, c];
                    if (factor.IsZero) continue;
                    for (int j = c; j <= cols; ++j)
                        M[i, j] = M[i, j] - factor * M[r, j];
                }

                pivotRowOfCol[c] = r;
                pivotColOfRow[r] = c;
                r++;
            }

            // check inconsistency: all-zero coefficients but non-zero RHS
            for (int i = 0; i < rows; ++i)
            {
                bool allZero = true;
                for (int j = 0; j < cols; ++j) if (!M[i, j].IsZero) { allZero = false; break; }
                if (allZero && !M[i, cols].IsZero) return -1; // impossible
            }

            // Build free columns
            var freeCols = new List<int>();
            for (int c = 0; c < cols; ++c) if (pivotRowOfCol[c] == -1) freeCols.Add(c);

            // If no free cols, unique rational solution -> must check integrality and non-negativity
            if (freeCols.Count == 0)
            {
                var sol = new Fraction<FracType>[cols];
                for (int c = 0; c < cols; ++c)
                {
                    int prow = pivotRowOfCol[c];
                    if (prow == -1) sol[c] = Fraction<FracType>.Zero; else sol[c] = M[prow, cols];
                }
                // check integers >=0
                long sum = 0;
                for (int c = 0; c < cols; ++c)
                {
                    if (sol[c].Den != 1) return -1; // not integral
                    if (sol[c].Num < 0) return -1;
                    sum += sol[c].Num;
                }
                return sum;
            }

            // If too many free vars, bail or fallback to heuristic PQ search on integer space.
            if (freeCols.Count > freeVarEnumerateLimit)
            {
                // fallback: use a best-first search similar to your PQ but on columns (buttons)
                // We'll implement a simple bounded Dijkstra-like search over press combinations,
                // but this is likely slower than a tailored ILP. We return -1 here to force user to lower limit or use combos.
                // (Could implement a PQ-based search if you want — ask me.)
                throw new InvalidOperationException($"Too many free variables ({freeCols.Count}). Increase freeVarEnumerateLimit or reduce columns (combine columns).");
            }

            // Precompute integer upper bounds for each free var using original A (options)
            // For each free column f, for each row i with A[i,f] > 0:
            //   x_f <= target[i] / A[i,f]
            // If no positive entries (column all zeros) -> then set free var to 0 (it doesn't affect eqns but increases cost)
            var freeBounds = new Dictionary<int, int>();
            int globalUpper = 0;
            // compute a conservative global upper bound as sum of target (if all options had coefficient 1)
            globalUpper = target.Sum(t => (int)t);
            for (int idx = 0; idx < freeCols.Count; ++idx)
            {
                int f = freeCols[idx];
                int ub = int.MaxValue;
                bool anyPos = false;
                for (int i = 0; i < rows; ++i)
                {
                    long a = options[f][i]; // non-negative
                    if (a > 0)
                    {
                        anyPos = true;
                        int candidate = (int)(target[i] / a);
                        if (candidate < ub) ub = candidate;
                    }
                }
                if (!anyPos) ub = 0; // this column has zero effect -> pressing it only increases cost so best is 0
                if (ub == int.MaxValue) ub = globalUpper; // fallback
                                                          // clamp ub to a reasonable ceiling (avoid huge enumeration)
                ub = Math.Min(ub, globalUpper);
                freeBounds[f] = ub;
            }

            // Order free variables to help pruning: sort by smaller bound first
            var orderedFree = freeCols.OrderBy(f => freeBounds[f]).ToArray();
            var boundArray = orderedFree.Select(f => freeBounds[f]).ToArray();

            long bestCost = long.MaxValue;
            // We'll do a DFS enumerating all integer assignments to free vars within [0, ub].
            // For partial assignments, we check pivot variables that become *fully determined*:
            // A pivot var x[p] depends on all free columns for which M[row,free] != 0;
            // only when all those free columns have been assigned we can compute x[p].
            // For simplicity we only check when all free vars are assigned (full check).
            // (You could add incremental pruning by computing partial lower bounds.)

            int freeCount = orderedFree.Length;
            var freeValues = new FracType[freeCount];

            void CheckFullAssignment()
            {
                // build full x as Fraction[]: free entries are integers; pivot entries determined by RREF rows
                var x = new Fraction<FracType>[cols];
                // set free vars
                for (int k = 0; k < freeCount; ++k) x[orderedFree[k]] = new Fraction<FracType>(freeValues[k], 1);

                // compute pivot column values: for pivot column p with row prow:
                // x[p] = M[prow, cols] - sum_{free f} M[prow, f] * x[f]
                for (int p = 0; p < cols; ++p)
                {
                    int prow = pivotRowOfCol[p];
                    if (prow == -1) continue;
                    Fraction<FracType> val = M[prow, cols];
                    for (int k = 0; k < freeCount; ++k)
                    {
                        int fcol = orderedFree[k];
                        var coef = M[prow, fcol];
                        if (!coef.IsZero && freeValues[k] != 0)
                        {
                            val = val - coef * new Fraction<FracType>(freeValues[k], 1);
                        }
                    }
                    x[p] = val;
                }

                // Validate integrality and non-negativity
                long cost = 0;
                for (int c = 0; c < cols; ++c)
                {
                    var v = x[c];
                    if (v.Den != 1) return; // not integer -> invalid
                    if (v.Num < 0) return; // negative -> invalid
                    cost += v.Num;
                    if (cost >= bestCost) return; // prune
                }

                // Additionally verify A*x == b (safety check for numeric correctness)
                // (Should hold because we used RREF relations, but check to be safe)
                for (int i = 0; i < rows; ++i)
                {
                    long sum = 0;
                    for (int j = 0; j < cols; ++j) sum += options[j][i] * (int)(x[j].Num); // x[j].Den==1
                    if (sum != target[i]) return;
                }

                if (cost < bestCost) bestCost = cost;
            }

            // Recursive enumeration with simple pruning: stop if partial assigned cost >= bestCost
            void Dfs(int idx, long partialCost)
            {
                if (partialCost >= bestCost) return;
                if (idx == freeCount)
                {
                    CheckFullAssignment();
                    return;
                }
                int ub = boundArray[idx];
                // Try small-to-large values; trying low values first helps find low-cost solution early
                for (FracType v = 0; v <= ub; ++v)
                {
                    freeValues[idx] = v;
                    Dfs(idx + 1, partialCost + v);
                    // small optimization: if v==0 and bound is 0, it's trivial; but loop handles it.
                    // Another optimization would compute lower bounds for pivot negativity, omitted for clarity.
                }
            }

            Dfs(0, 0);

            if (bestCost == long.MaxValue) return -1;
            return bestCost;
        }
    }
    #endregion
}


/*
 
 Alternative solution: https://www.reddit.com/r/adventofcode/comments/1pk87hl/2025_day_10_part_2_bifurcate_your_way_to_victory/

from functools import cache
from itertools import combinations, product
import aocd

def patterns(coeffs: list[tuple[int, ...]]) -> dict[tuple[int, ...], dict[tuple[int, ...], int]]:
	num_buttons = len(coeffs)
	num_variables = len(coeffs[0])
	out = {parity_pattern: {} for parity_pattern in product(range(2), repeat=num_variables)}
	for num_pressed_buttons in range(num_buttons+1):
		for buttons in combinations(range(num_buttons), num_pressed_buttons):
			pattern = tuple(map(sum, zip((0,) * num_variables, *(coeffs[i] for i in buttons))))
			parity_pattern = tuple(i%2 for i in pattern)
			if pattern not in out[parity_pattern]:
				out[parity_pattern][pattern] = num_pressed_buttons
	return out

def solve_single(coeffs: list[tuple[int, ...]], goal: tuple[int, ...]) -> int:
	pattern_costs = patterns(coeffs)
	@cache
	def solve_single_aux(goal: tuple[int, ...]) -> int:
		if all(i == 0 for i in goal): return 0
		answer = 1000000
		for pattern, pattern_cost in pattern_costs[tuple(i%2 for i in goal)].items():
			if all(i <= j for i, j in zip(pattern, goal)):
				new_goal = tuple((j - i)//2 for i, j in zip(pattern, goal))
				answer = min(answer, pattern_cost + 2 * solve_single_aux(new_goal))
		return answer
	return solve_single_aux(goal)

def solve(raw: str):
	score = 0
	lines = raw.splitlines()
	for I, L in enumerate(lines, 1):
		_, *coeffs, goal = L.split()
		goal = tuple(int(i) for i in goal[1:-1].split(","))
		coeffs = [[int(i) for i in r[1:-1].split(",")] for r in coeffs]
		coeffs = [tuple(int(i in r) for i in range(len(goal))) for r in coeffs]

		subscore = solve_single(coeffs, goal)
		print(f'Line {I}/{len(lines)}: answer {subscore}')
		score += subscore
	print(score)

# solve(open('input/10.test').read())
solve(aocd.get_data(year=2025, day=10))

 */