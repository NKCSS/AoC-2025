using NKCSS.AoC;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace AoC2025
{
    public class Day3 : Solution
    {
        bool Test = false;
        const long AnswerP1Test = 357, AnswerP2Test = 3121910778619, AnswerP1 = 17359, AnswerP2 = 172787336861064;
        public Day3() : base(3) {
            if (Test)
            {
                Input = """
                    987654321111111
                    811111111111119
                    234234234234278
                    818181911112111
                    """;
            }
            Part1();
            Part2();
        }
        (char largest, int index) GetLargest(char[] value, int start, int end)
        {
            const char max = '9';
            char biggest = '0';
            int biggestIndex = 0;
            for (int i = start, last = value.Length - end; i < last; ++i)
            {
                if (value[i] > biggest)
                {
                    biggestIndex = i;
                    biggest = value[i];
                    if (biggest == max) break;
                }
            }
            return (biggest, biggestIndex);
        }
        void Part1()
        {
            long p1 = 0L;
            foreach (string line in Input.ToLines())
            {
                var chars = line.ToCharArray();
                var first = GetLargest(chars, 0, 1);
                var second = GetLargest(chars, first.index + 1, 0);
                Console.WriteLine($"{line} -> {first.largest}{second.largest}");
                p1 += long.Parse($"{first.largest}{second.largest}");
            }
            //Console.WriteLine($"Part 1: {p1}");
            Debug.Assert(p1 == (Test ? AnswerP1Test : AnswerP1), "You broke Part 1!");
        }
        void Part2()
        {
            long p2 = 0L;
            List<(char largest, int index)> values = [];
            char[] chars;
            int lastIndex;
            long localResult;
            foreach (string line in Input.ToLines())
            {
                values.Clear();
                chars = line.ToCharArray();
                lastIndex = 0;
                for (int i = 11; i >= 0; i--)
                {
                    var biggest = GetLargest(chars, lastIndex, i);
                    values.Add(biggest);
                    lastIndex = biggest.index + 1;
                }
                localResult = long.Parse(string.Join(string.Empty, values.Select(x => x.largest)));
                //Console.WriteLine($"{line} -> {localResult}");
                p2 += localResult;
            }
            Console.WriteLine($"Part 2: {p2}");
            Debug.Assert(p2 == (Test ? AnswerP2Test : AnswerP2), "You broke Part 2!");
        }
    }
}