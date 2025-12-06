using NKCSS.AoC;
using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Diagnostics;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.RegularExpressions;

namespace AoC2025
{
    public class Day6 : Solution
    {
        bool Test = false;
        const long AnswerP1Test = 4277556, AnswerP2Test = 3263827, AnswerP1 = 6725216329103, AnswerP2 = 10600728112865;
        List<int> columnSeparators;
        List<List<ulong>> problemset = [];
        List<char> problemsetOperators = [];
        int problemCount, lineCount;
        string[] lines;
        public Day6() : base(6) {
            if (Test)
            {
                // Paste test input here...
                Input = """
                    123 328  51 64 
                     45 64  387 23 
                      6 98  215 314
                    *   +   *   +  
                    """;
            }
            lines = Input.ToLines();
            lineCount = lines.Length;
            columnSeparators = [.. FindColumns(lines)];
            Part1();
            Part2();
        }
        string GetColumnString(string[] lines, int columnIndex)
        {
            StringBuilder sb = new(lineCount);
            // do -1 to skip the operator!
            for(int i = 0; i < lineCount -1; ++i)
            {
                sb.Append(lines[i][columnIndex]);
            }
            return sb.ToString();
        }
        IEnumerable<int> FindColumns(string[] lines, char toFind = ' ')
        {
            char[] firstLine = lines[0].ToCharArray();
            int lineCount = lines.Length, charCount = firstLine.Length;
            bool good;
            for (int i = 0; i < charCount; ++i)
            {
                good = false;
                if (firstLine[i] == toFind)
                {
                    good = true;
                    // validate all the way down.
                    for (int j = 1; j < lineCount; ++j)
                    {
                        if (lines[j][i] != toFind)
                        {
                            good = false;
                            break;
                        }
                    }
                    if (good) yield return i;
                }
                if (good)
                {
                    while (++i < charCount && firstLine[i] == toFind)
                    {
                        // skip till we get to a non-whitespace column
                    }
                }
            }
        }
        const char AddOperator = '+', MultiplyOperator = '*';
        void Part1()
        {
            problemset = [];
            problemsetOperators = [];
            int startIndex = 0;
            foreach (int nextIndex in columnSeparators)
            {
                List<ulong> problem = [];
                for (int i = 0; i < lineCount - 1; ++i)
                {
                    problem.Add(ulong.Parse(lines[i][startIndex..nextIndex].Trim()));
                }
                problemsetOperators.Add(lines[lineCount - 1][startIndex..nextIndex].Trim()[0]);
                problemset.Add(problem);
                startIndex = nextIndex;
            }
            {
                List<ulong> problem = [];
                for (int i = 0; i < lineCount - 1; ++i)
                {
                    problem.Add(ulong.Parse(lines[i][startIndex..].Trim()));
                }
                problemsetOperators.Add(lines[lineCount - 1][startIndex..].Trim()[0]);
                problemset.Add(problem);
            }
            problemCount = problemsetOperators.Count;
            Console.WriteLine($"We parsed {problemCount} math problems to solve...");
            ulong p1 = 0L;
            for (int i = 0; i < problemCount; ++i)
            {
                char op = problemsetOperators[i];
                var values = problemset[i];
                ulong result = values[0];
                switch(op)
                {
                    case AddOperator:
                        for(int j = 1, valueCount = values.Count; j < valueCount; ++j)
                        {
                            result += values[j];
                        }
                        break;
                    case MultiplyOperator:
                        for (int j = 1, valueCount = values.Count; j < valueCount; ++j)
                        {
                            result *= values[j];
                        }
                        break;
                    default:
                        throw new ArgumentException($"We don't support the '{op}' operator (yet).");
                }
                p1 += result;
            }
            Console.WriteLine($"Part 1: {p1}");
            Debug.Assert(p1 == (ulong)(Test ? AnswerP1Test : AnswerP1), "You broke Part 1!");
        }
        void Part2()
        {
            ulong p2 = 0L;
            problemset = [];
            int startIndex = 0;
            foreach (int nextIndex in columnSeparators)
            {
                List<ulong> problem = [];

                for (int i = startIndex; i < nextIndex; ++i)
                {
                    string colString = GetColumnString(lines, i).Trim();
                    if(!string.IsNullOrWhiteSpace(colString)) problem.Add(ulong.Parse(colString));
                 }
                problemset.Add(problem);
                startIndex = nextIndex;
            }
            {
                List<ulong> problem = [];
                for (int i = startIndex, c = lines[0].Length; i < c; ++i)
                {
                    string colString = GetColumnString(lines, i).Trim();
                    if (!string.IsNullOrWhiteSpace(colString)) problem.Add(ulong.Parse(colString));
                }
                problemset.Add(problem);
            }
            problemCount = problemsetOperators.Count;

            for (int i = 0; i < problemCount; ++i)
            {
                char op = problemsetOperators[i];
                var values = problemset[i];
                ulong result = values[0];
                switch (op)
                {
                    case AddOperator:
                        for (int j = 1, valueCount = values.Count; j < valueCount; ++j)
                        {
                            result += values[j];
                        }
                        break;
                    case MultiplyOperator:
                        for (int j = 1, valueCount = values.Count; j < valueCount; ++j)
                        {
                            result *= values[j];
                        }
                        break;
                    default:
                        throw new ArgumentException($"We don't support the '{op}' operator (yet).");
                }
                p2 += result;
            }
            Console.WriteLine($"Part 2: {p2}");
            Debug.Assert(p2 == (ulong)(Test ? AnswerP2Test : AnswerP2), "You broke Part 2!");
        }
    }
}
