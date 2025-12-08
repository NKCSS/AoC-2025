using BenchmarkDotNet.Attributes;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using NKCSS.AoC;
using System.Collections.Immutable;
namespace AoC2025
{
    public class Day8 : Solution
    {
        const bool Test = false;
        const int NumToProcessTest = 10, NumToProcessReal = 1000, NumToProcess = Test ? NumToProcessTest : NumToProcessReal;
        const int BiggestGroupsToCount = 3;
        const long AnswerP1Test = 40, AnswerP2Test = 25272, AnswerP1 = 171503, AnswerP2 = 9069509600;
        List<Location3D> locations;
        List<(Location3D a, Location3D b, double distance)> uniquePairs = [];
        public Day8() : base(8) {
            if (Test)
            {
                // Paste test input here...
                Input = """
                    162,817,812
                    57,618,57
                    906,360,560
                    592,479,940
                    352,342,300
                    466,668,158
                    542,29,236
                    431,825,988
                    739,650,466
                    52,470,668
                    216,146,977
                    819,987,18
                    117,168,530
                    805,96,715
                    346,949,466
                    970,615,88
                    941,993,340
                    862,61,35
                    984,92,344
                    425,690,689
                    """;
            }
            locations = [.. Input.ToLines().Select(Location3D.Parse)];
            foreach(var pair in locations.GetPermutations(2, allowDupe: false))
            {
                (Location3D a, Location3D b) p = pair.AsValueTuple();
                uniquePairs.Add((p.a, p.b, p.a.EuclideanDistance(p.b)));
            }
            uniquePairs.Sort((a, b) => a.distance > b.distance ? 1 : -1);
            Part1();
            Part2();
        }
        void Part1()
        {
            long p1 = 0L;
            List<(Location3D a, Location3D b, double distance)> toProcess = [..uniquePairs.Take(NumToProcess)];
            Dictionary<Location3D, int> groupIdLookup = [];
            Dictionary<int, HashSet<Location3D>> groups = [];
            int groupId, otherGroupId;
            //foreach (var entry in toProcess)

            void mergeGroups(int groupId, int otherGroupId)
            {
                // merge groups when it's not the same group, otherwise, NOOP.
                if(groupId == otherGroupId) return;
                var groupA = groups[groupId];
                var groupB = groups[otherGroupId];
                bool aBigger = groupA.Count > groupB.Count;
                int biggestGroupId = aBigger ? groupId : otherGroupId;
                int smallestGroupId = aBigger ? otherGroupId : groupId;
                var from = aBigger ? groupB : groupA;
                var to = aBigger ? groupA : groupB;
                Console.Write($"Merging {from.Count} entries from group {smallestGroupId} into group {biggestGroupId} which has {to.Count} entries");
                foreach (var p in from)
                {
                    groupIdLookup[p] = biggestGroupId;
                }
                to.UnionWith(from);
                from.Clear();
                Console.WriteLine($" ({to.Count} afer the merge).");
            }

            int pairsProcessed = 0;
            foreach(var entry in toProcess)//uniquePairs)
            {
                ++pairsProcessed;
                //Console.WriteLine($"{entry.a} -> {entry.b} = {entry.distance:F2}");
                if (groupIdLookup.TryGetValue(entry.a, out groupId))
                {
                    // make sure to only link when not already part of a group
                    if (groupIdLookup.TryGetValue(entry.b, out otherGroupId))
                    {
                        mergeGroups(groupId, otherGroupId);
                    }
                    else 
                    {
                        // add b to a's group if it's not already part
                        groupIdLookup.Add(entry.b, groupId);
                        groups[groupId].Add(entry.b);
                    }
                }
                else if (groupIdLookup.TryGetValue(entry.b, out groupId))
                {
                    // make sure to only link when not already part of a group
                    if (groupIdLookup.TryGetValue(entry.a, out otherGroupId))
                    {
                        mergeGroups(groupId, otherGroupId);
                    }
                    else
                    {
                        // add a to b's group
                        groupIdLookup.Add(entry.a, groupId);
                        groups[groupId].Add(entry.a);
                    }
                } 
                else
                {
                    groupId = groups.Count;
                    groups.Add(groupId, [entry.a, entry.b]);
                    groupIdLookup.Add(entry.a, groupId);
                    groupIdLookup.Add(entry.b, groupId);
                }
                long sum = groups.OrderByDescending(x => x.Value.Count).Take(BiggestGroupsToCount).Aggregate(1L, (toAdd, current) => Math.Max(1L, (long)current.Value.Count) * toAdd);
                Console.WriteLine($"After processing {pairsProcessed} pairs ({groupIdLookup.Count} unique locations, {groups.Count} groups formed so far): {sum}");
                if (sum == 0) Debugger.Break();
            }
            p1 = groups.OrderByDescending(x => x.Value.Count).Take(BiggestGroupsToCount)
                .Aggregate(1L, (toAdd, current) => (long)current.Value.Count * toAdd);
            Console.WriteLine($"Part 1: {p1}");
            Debug.Assert(p1 == (Test ? AnswerP1Test : AnswerP1), "You broke Part 1!");
        }
        void Part2()
        {
            long p2 = 0L;
            List<(Location3D a, Location3D b, double distance)> toProcess = [.. uniquePairs.Take(NumToProcess)];
            Dictionary<Location3D, int> groupIdLookup = [];
            Dictionary<int, HashSet<Location3D>> groups = [];
            int groupId, otherGroupId, nonEmptyGroupCount;
            //foreach (var entry in toProcess)

            void mergeGroups(int groupId, int otherGroupId)
            {
                // merge groups when it's not the same group, otherwise, NOOP.
                if (groupId == otherGroupId) return;
                var groupA = groups[groupId];
                var groupB = groups[otherGroupId];
                bool aBigger = groupA.Count > groupB.Count;
                int biggestGroupId = aBigger ? groupId : otherGroupId;
                int smallestGroupId = aBigger ? otherGroupId : groupId;
                var from = aBigger ? groupB : groupA;
                var to = aBigger ? groupA : groupB;
                Console.Write($"Merging {from.Count} entries from group {smallestGroupId} into group {biggestGroupId} which has {to.Count} entries");
                foreach (var p in from)
                {
                    groupIdLookup[p] = biggestGroupId;
                }
                to.UnionWith(from);
                from.Clear();
                Console.WriteLine($" ({to.Count} afer the merge).");
            }

            int pairsProcessed = 0;
            foreach (var entry in uniquePairs)
            {
                ++pairsProcessed;
                //Console.WriteLine($"{entry.a} -> {entry.b} = {entry.distance:F2}");
                if (groupIdLookup.TryGetValue(entry.a, out groupId))
                {
                    // make sure to only link when not already part of a group
                    if (groupIdLookup.TryGetValue(entry.b, out otherGroupId))
                    {
                        mergeGroups(groupId, otherGroupId);
                    }
                    else
                    {
                        // add b to a's group if it's not already part
                        groupIdLookup.Add(entry.b, groupId);
                        groups[groupId].Add(entry.b);
                    }
                }
                else if (groupIdLookup.TryGetValue(entry.b, out groupId))
                {
                    // make sure to only link when not already part of a group
                    if (groupIdLookup.TryGetValue(entry.a, out otherGroupId))
                    {
                        mergeGroups(groupId, otherGroupId);
                    }
                    else
                    {
                        // add a to b's group
                        groupIdLookup.Add(entry.a, groupId);
                        groups[groupId].Add(entry.a);
                    }
                }
                else
                {
                    groupId = groups.Count;
                    groups.Add(groupId, [entry.a, entry.b]);
                    groupIdLookup.Add(entry.a, groupId);
                    groupIdLookup.Add(entry.b, groupId);
                }
                long sum = groups.OrderByDescending(x => x.Value.Count).Take(BiggestGroupsToCount).Aggregate(1L, (toAdd, current) => Math.Max(1L, (long)current.Value.Count) * toAdd);
                //Console.WriteLine($"After processing {pairsProcessed} pairs ({groupIdLookup.Count} unique locations, {groups.Count} groups formed so far): {sum}");
                if (sum == 0) Debugger.Break();
                if (groupIdLookup.Count == locations.Count)
                {
                    Console.WriteLine($"last node connected. {entry.a} <-> {entry.b}");
                    p2 = (long)entry.a.x * (long)entry.b.x;
                    break;
                }
            }
            Console.WriteLine($"Part 2: {p2}");
            Debug.Assert(p2 == (Test ? AnswerP2Test : AnswerP2), "You broke Part 2!");
        }
        #region For Benchmark.NET
        [Benchmark]
        public void Day8_Part1() => Part1();
        [Benchmark]
        public void Day8_Part2() => Part2();
        #endregion
    }
}
