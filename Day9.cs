using AoC2025;
using BenchmarkDotNet.Attributes;
using Microsoft.Diagnostics.Runtime;
using NKCSS.AoC;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text.RegularExpressions;
using System.Xml.Serialization;
namespace AoC2025
{
    public class Day9 : Solution
    {
        const char RedTileMarker = '#';
        const bool Test = false, SaveImage = false;
        const long AnswerP1Test = 50L, AnswerP2Test = 24L, AnswerP1 = 4749838800L, AnswerP2 = 1624057680L;
        List<GridLocation> redTiles;
        List<(GridLocation a, GridLocation b, long surface)> uniqueCombos;
        List<(GridLocation a, GridLocation b)> lineSegments;
        public Day9() : base(9) {
            if (Test)
            {
                // Paste test input here...
                Input = """
                    7,1
                    11,1
                    11,7
                    9,7
                    9,5
                    2,5
                    2,3
                    7,3
                    """;
            }
            redTiles = [.. Input.ToLines().Select(x => x.Split(',').AsInt32s().AsValueTuple()).Select(x => new GridLocation(x.Item1, x.Item2))];
            uniqueCombos = [..redTiles.GetPermutations(2, allowDupe: false).Select(x => x.AsValueTuple()).Select(x => (x.Item1, x.Item2, x.Item1.Surface(x.Item2))).OrderByDescending(x => x.Item3)];
            lineSegments = [.. redTiles.Zip(redTiles.Skip(1))];
            lineSegments.Add((redTiles.Last(), redTiles.First()));
            Part1();
            Part2();
        }
        void Part1()
        {
            long p1 = 0L;
            p1 = uniqueCombos.First().surface;
            Console.WriteLine($"Part 1: {p1}");
            Debug.Assert(p1 == (Test ? AnswerP1Test : AnswerP1), "You broke Part 1!");
        }
        Bitmap Visualize((GridLocation topLeft, GridLocation bottomRight) square, bool printDebug = false, int imageSize = 1000)
        {
            int row, col, from, to;
            if (printDebug) Console.WriteLine($"Area: {square.topLeft}->{square.bottomRight}");
            bool match = true;
            float scale = (float)imageSize / redTiles.Max(x => Math.Max(x.Row, x.Column));
            Bitmap bmp = new Bitmap(imageSize, imageSize);
            using (Graphics gfx = Graphics.FromImage(bmp))
            {
                gfx.FillRectangle(Brushes.White, new Rectangle(0, 0, imageSize, imageSize));
                gfx.FillRectangle(Brushes.Black, new RectangleF(square.topLeft.Column * scale, square.topLeft.Row * scale, (square.bottomRight.Column - square.topLeft.Column) * scale, (square.bottomRight.Row - square.topLeft.Row) * scale));
                foreach (var segment in lineSegments)
                {
                    row = segment.a.Row;
                    col = segment.a.Column;
                    if (row == segment.b.Row)
                    {
                        from = Math.Min(col, segment.b.Column);
                        to = Math.Max(col, segment.b.Column);
                        if (printDebug) Console.Write($"Line on row {row} from col {segment.a.Column} to {segment.b.Column}...");
                        // horizontal
                        if (row <= square.topLeft.Row || row >= square.bottomRight.Row)
                        {
                            // does not intersect
                            if (printDebug) Console.WriteLine($"does not intersect!");
                            gfx.DrawLine(Pens.Green, new PointF(segment.a.Column * scale, segment.a.Row * scale), new PointF(segment.b.Column * scale, segment.b.Row * scale));
                        }
                        else
                        {
                            // check col.
                            if (
                                square.topLeft.Column < to
                                &&
                                square.bottomRight.Column > from
                            )
                            {
                                if (printDebug) Console.WriteLine($"intersects!");
                                // intersects with our shape!
                                gfx.DrawLine(Pens.Red, new PointF(segment.a.Column * scale, segment.a.Row * scale), new PointF(segment.b.Column * scale, segment.b.Row * scale));
                                match = false;
                                break;
                            }
                            else
                            {
                                if (printDebug) Console.WriteLine($"On same row, not column!");
                                gfx.DrawLine(Pens.Blue, new PointF(segment.a.Column * scale, segment.a.Row * scale), new PointF(segment.b.Column * scale, segment.b.Row * scale));
                            }
                        }
                    }
                    // vertical (e.g. column == column)
                    else
                    {
                        from = Math.Min(row, segment.b.Row);
                        to = Math.Max(row, segment.b.Row);
                        if (Test) Console.Write($"Line on column {col} from row {segment.a.Row} to {segment.b.Row}...");
                        if (col <= square.topLeft.Column || col >= square.bottomRight.Column)
                        {
                            // does not intersect
                            if (Test) Console.WriteLine($"does not intersect!");
                            gfx.DrawLine(Pens.Green, new PointF(segment.a.Column * scale, segment.a.Row * scale), new PointF(segment.b.Column * scale, segment.b.Row * scale));
                        }
                        else
                        {
                            // check rows.
                            if (
                                square.topLeft.Column < to
                                &&
                                square.bottomRight.Column > from
                            )
                            {
                                // intersects with our shape!
                                if (Test) Console.WriteLine($"intersects!");
                                gfx.DrawLine(Pens.Red, new PointF(segment.a.Column * scale, segment.a.Row * scale), new PointF(segment.b.Column * scale, segment.b.Row * scale));
                                match = false;
                                break;
                            }
                            else
                            {
                                if (Test) Console.WriteLine($"On same col, not row!");
                                gfx.DrawLine(Pens.Blue, new PointF(segment.a.Column * scale, segment.a.Row * scale), new PointF(segment.b.Column * scale, segment.b.Row * scale));
                            }
                        }
                    }
                }
                gfx.Flush();
            }
            return bmp;
        }
        void Part2()
        {
            long p2 = 0L;
            foreach(var candidate in uniqueCombos)
            {
                // check if it intersects with any lines, if it does, scrap it.
                var square = candidate.a.Square(candidate.b);
                bool match = true;
                int row, col, from, to;
                foreach (var segment in lineSegments)
                {
                    row = segment.a.Row;
                    col = segment.a.Column;
                    if (row == segment.b.Row)
                    {
                        from = Math.Min(col, segment.b.Column);
                        to = Math.Max(col, segment.b.Column);
                        // horizontal
                        if (row <= square.topLeft.Row || row >= square.bottomRight.Row)
                        {
                            // does not intersect
                        }
                        else
                        {
                            // check col.
                            if (
                                square.topLeft.Column < to
                                &&
                                square.bottomRight.Column > from
                            )
                            {
                                // intersects with our shape!
                                match = false;
                                break;
                            }
                            else
                            {
                                // no intersection
                            }
                        }
                    }
                    // vertical (e.g. column == column)
                    else
                    {
                        from = Math.Min(row, segment.b.Row);
                        to = Math.Max(row, segment.b.Row);
                        if (col <= square.topLeft.Column || col >= square.bottomRight.Column)
                        {
                            // does not intersect
                        }
                        else
                        {
                            // check rows.
                            if (
                                square.topLeft.Column < to
                                &&
                                square.bottomRight.Column > from
                            )
                            {
                                // intersects with our shape!
                                match = false;
                                break;
                            }
                            else
                            {
                                // does not intersect
                            }
                        }
                    }
                }
                if (match)
                {
                    p2 = candidate.surface;
                    if (SaveImage) Visualize(square, printDebug: false, imageSize: 1000).Save("part2.png", System.Drawing.Imaging.ImageFormat.Png);
                    break;
                }
            }
            Console.WriteLine($"Part 2: {p2}");
            Debug.Assert(p2 == (Test ? AnswerP2Test : AnswerP2), "You broke Part 2!");
        }
        #region For Benchmark.NET
        [Benchmark]
        public void Day9_Part1() => Part1();
        [Benchmark]
        public void Day9_Part2() => Part2();
        #endregion
    }
}
