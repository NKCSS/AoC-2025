using BenchmarkDotNet.Attributes;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using AoC2025;
namespace AoC2025
{
    public class Day0 : Solution
    {
        bool Test = true;
        const long AnswerP1Test = -1, AnswerP2Test = -1, AnswerP1 = -1L, AnswerP2 = -1L;
        public Day0() : base(0) {
            if (Test)
            {
                // Paste test input here...
                Input = """

                    """;
            }
            PrepareInput();
            Part1();
            Part2();
        }
        void PrepareInput()
        {
        }
        void Part1()
        {
            long p1 = 0L;
            // Add implementation here...
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
        public void Day0_Part1() => Part1();
        [Benchmark]
        public void Day0_Part2() => Part2();
        #endregion
    }
}
