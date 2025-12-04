using NKCSS.AoC;
using System.Diagnostics;

namespace AoC2025
{
    public class Day3 : Solution
    {
        // Single char value can't be higher than this, so if we encounter it, we can stop looking.
        const char MaxChar = '9';
        record Max(byte value, int index);
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
        int optimizeCount = 0;
        Max GetLargest(char[] value, int start, int end)
        {
            char biggest = '0';
            int biggestIndex = 0;
            for (int i = start, last = value.Length - end; i < last; ++i)
            {
                if (value[i] > biggest)
                {
                    biggestIndex = i;
                    biggest = value[i];
                    if (biggest == MaxChar)
                    {
                        ++optimizeCount;
                        break;
                    }
                }
            }
            return new((byte)(biggest - '0'), biggestIndex);
        }
        void Part1()
        {
            long p1 = 0L;
            Max first, second;
            foreach (string line in Input.ToLines())
            {
                var chars = line.ToCharArray();
                first = GetLargest(chars, 0, 1);
                second = GetLargest(chars, first.index + 1, 0);
                p1 += first.value * 10 + second.value;
            }
            Console.WriteLine($"Part 1: {p1}");
            Debug.Assert(p1 == (Test ? AnswerP1Test : AnswerP1), "You broke Part 1!");
        }
        void Part2()
        {
            long p2 = 0L;
            List<Max> values = [];
            char[] chars;
            int lastIndex;
            long localResult;
            foreach (string line in Input.ToLines())
            {
                localResult = 0;
                values.Clear();
                chars = line.ToCharArray();
                lastIndex = 0;
                for (int i = 11; i >= 0; i--)
                {
                    Max biggest = GetLargest(chars, lastIndex, i);
                    values.Add(biggest);
                    lastIndex = biggest.index + 1;
                    // by *= 10 the value, we shift the values 1 position to the left.
                    localResult = localResult * 10 + biggest.value;
                }
                p2 += localResult;
            }
            Console.WriteLine($"Part 2: {p2}");
            Console.WriteLine($"Breaked early: {optimizeCount}/{Input.ToLines().Length}");
            Debug.Assert(p2 == (Test ? AnswerP2Test : AnswerP2), "You broke Part 2!");
        }
    }
}