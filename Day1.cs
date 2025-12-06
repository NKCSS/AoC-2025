using BenchmarkDotNet.Attributes;
using NKCSS.AoC;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;

namespace AoC2025
{
    public class Day1 : Solution
    {
        bool Test = false;
        const long AnswerP1Test = 3, AnswerP2Test = 6, AnswerP1 = 1081, AnswerP2 = 6689;
        const char RotateLeft = 'L', RotateRight = 'R';
        public Day1() : base(1) {
            if (Test)
            {
                // Paste test input here...
                Input = @"L68
L30
R48
L5
R60
L55
L1
L99
R14
L82";
            }
            Part1();
            Part2();
        }
        void Part1()
        {
            long p1 = 0;
            long currentValue = 50L;
            int value = 0;
            // Add implementation here...
            foreach(string instruction in Input.ToLines())
            {
                if (string.IsNullOrEmpty(instruction))
                {
                    Console.WriteLine($"Input contains empty lines!");
                    continue;
                }
                switch(instruction[0])
                {
                    case RotateLeft:
                        value = int.Parse(instruction.Substring(1)) * -1;
                        break;
                    case RotateRight:
                        value = int.Parse(instruction.Substring(1));
                        break;
                    default:
                        Console.WriteLine($"Unexpected instruction: {instruction}");
                        break;
                }
                currentValue = (currentValue + value) % 100;
                if (currentValue == 0) ++p1;
            }
            Console.WriteLine($"Part 1: {p1}");
            Debug.Assert(p1 == (Test ? AnswerP1Test : AnswerP1), "You broke Part 1!");
        }
        void Part2()
        {
            const int DialSize = 100;
            long p2 = 0L;
            long currentValue = 50L;
            int value = 0, remain = 0;
            bool fromZero = false;
            // Add implementation here...
            foreach (string instruction in Input.ToLines())
            {
                if (string.IsNullOrEmpty(instruction))
                {
                    Console.WriteLine($"Input contains empty lines!");
                    continue;
                }
                value = int.Parse(instruction[1..]);
                // extra full rotations
                p2 += value / DialSize;
                // remainder to process.
                remain = value % DialSize;
                fromZero = currentValue == 0;
                switch (instruction[0])
                {
                    case RotateLeft:
                        currentValue -= remain;
                        if (currentValue <= 0)
                        {
                            if (!fromZero) ++p2;
                            if (currentValue < 0) currentValue += DialSize;
                        }
                        break;
                    case RotateRight:
                        currentValue += remain;
                        if (currentValue >= DialSize)
                        {
                            if (!fromZero) ++p2;
                            currentValue -= DialSize;
                        }
                        break;
                    default:
                        Console.WriteLine($"Unexpected instruction: {instruction}");
                        break;
                }
            }
            Console.WriteLine($"Part 2: {p2}");
            Debug.Assert(p2 == (Test ? AnswerP2Test : AnswerP2), "You broke Part 2!");
        }
        #region For Benchmark.NET
        [Benchmark]
        public void Day1_Part1() => Part1();
        [Benchmark]
        public void Day1_Part2() => Part2();
        #endregion
    }
}
