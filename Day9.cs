using BenchmarkDotNet.Attributes;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using AoC2025;
using NKCSS.AoC;
using System.Drawing;
using Microsoft.Diagnostics.Runtime;
using System.Net.NetworkInformation;
using System.Xml.Serialization;
namespace AoC2025
{
    public class Day9 : Solution
    {
        const char RedTileMarker = '#';
        bool Test = false;
        const long AnswerP1Test = 50, AnswerP2Test = 24, AnswerP1 = 4749838800, AnswerP2 = -1L;
        List<GridLocation> redTiles;
        List<(GridLocation a, GridLocation b, ulong surface)> uniqueCombos;
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
            ulong p1 = 0L;
            p1 = uniqueCombos.First().surface;
            Console.WriteLine($"Part 1: {p1}");
            Debug.Assert(p1 == (ulong)(Test ? AnswerP1Test : AnswerP1), "You broke Part 1!");
        }
        void Part2()
        {
            ulong p2 = 0L;
            // Add implementation here...
            const bool visualize = false;
            if (visualize)
            {
                const int ImgSize = 1000;
                float scale = (float)ImgSize / redTiles.Max(x => Math.Max(x.Row, x.Column));
                using (Bitmap bmp = new Bitmap(ImgSize, ImgSize))
                {
                    using (Graphics gfx = Graphics.FromImage(bmp))
                    {
                        gfx.FillRectangle(Brushes.White, new Rectangle(0, 0, ImgSize, ImgSize));
                        foreach (var segment in lineSegments)
                        {
                            gfx.DrawLine(Pens.Black, new PointF(segment.a.Column * scale, segment.a.Row * scale), new PointF(segment.b.Column * scale, segment.b.Row * scale));
                        }
                        gfx.Flush();
                        gfx.Save();
                    }
                    bmp.Save("area.png", System.Drawing.Imaging.ImageFormat.Png);
                }
            }
            int areaId = 0;
            foreach(var candidate in uniqueCombos)
            {
                // check if it intersects with any lines, if it does, scrap it.
                var square = candidate.a.Square(candidate.b);
                bool match = true;
                int row, col, from, to;
                if (Test) Console.WriteLine($"Area: {square.topLeft}->{square.bottomRight}");
                
                const int ImgSize = 1000;
                float scale = (float)ImgSize / redTiles.Max(x => Math.Max(x.Row, x.Column));
                using (Bitmap bmp = new Bitmap(ImgSize, ImgSize))
                {
                    using (Graphics gfx = Graphics.FromImage(bmp))
                    {
                        gfx.FillRectangle(Brushes.White, new Rectangle(0, 0, ImgSize, ImgSize));
                        gfx.FillRectangle(Brushes.Black, new RectangleF(square.topLeft.Column * scale, square.topLeft.Row * scale, (square.bottomRight.Column - square.topLeft.Column) * scale, (square.bottomRight.Row - square.topLeft.Row) * scale));
                        foreach (var segment in lineSegments)
                        {
                            row = segment.a.Row;
                            col = segment.a.Column;
                            if (row == segment.b.Row)
                            {
                                from = Math.Min(col, segment.b.Column);
                                to = Math.Max(col, segment.b.Column);
                                Console.Write($"Line on row {row} from col {segment.a.Column} to {segment.b.Column}...");
                                // horizontal
                                if (row <= square.topLeft.Row || row >= square.bottomRight.Row)
                                {
                                    // does not intersect
                                    Console.WriteLine($"does not intersect!");
                                    gfx.DrawLine(Pens.Green, new PointF(segment.a.Column * scale, segment.a.Row * scale), new PointF(segment.b.Column * scale, segment.b.Row * scale));
                                }
                                else
                                {
                                    // check col.
                                    if (
                                        square.topLeft.Column < to
                                        && 
                                        square.bottomRight.Column > from
                                        /*
                                        (col > square.topLeft.Column && col < square.bottomRight.Column)
                                        ||
                                        (col >= square.topLeft.Column && col < square.bottomRight.Column)
                                        ||
                                        (col > square.topLeft.Column && col <= square.bottomRight.Column)
                                        ||
                                        (segment.b.Column > square.topLeft.Column && segment.b.Column < square.bottomRight.Column)
                                        ||
                                        (segment.b.Column >= square.topLeft.Column && segment.b.Column < square.bottomRight.Column)
                                        ||
                                        (segment.b.Column > square.topLeft.Column && segment.b.Column <= square.bottomRight.Column)
                                        ||
                                        (segment.b.Column > square.topLeft.Column && segment.b.Column <= square.bottomRight.Column)
                                        */
                                    )
                                    {
                                        Console.WriteLine($"intersects!");
                                        // intersects with our shape!
                                        gfx.DrawLine(Pens.Red, new PointF(segment.a.Column * scale, segment.a.Row * scale), new PointF(segment.b.Column * scale, segment.b.Row * scale));
                                        match = false;
                                        break;
                                    }
                                    else
                                    {
                                        Console.WriteLine($"On same row, not column!");
                                        gfx.DrawLine(Pens.Blue, new PointF(segment.a.Column * scale, segment.a.Row * scale), new PointF(segment.b.Column * scale, segment.b.Row * scale));
                                    }
                                }
                            }
                            // vertical (e.g. column == column)
                            else
                            {
                                from = Math.Min(row, segment.b.Row);
                                to = Math.Max(row, segment.b.Row);
                                Console.Write($"Line on column {col} from row {segment.a.Row} to {segment.b.Row}...");
                                if (col <= square.topLeft.Column || col >= square.bottomRight.Column)
                                {
                                    // does not intersect
                                    Console.WriteLine($"does not intersect!");
                                    gfx.DrawLine(Pens.Green, new PointF(segment.a.Column * scale, segment.a.Row * scale), new PointF(segment.b.Column * scale, segment.b.Row * scale));
                                }
                                else
                                {
                                    // check rows.
                                    if (
                                        /*
                                        (row > square.topLeft.Row && row < square.bottomRight.Row)
                                        ||
                                        (row >= square.topLeft.Row && row < square.bottomRight.Row)
                                        ||
                                        (row > square.topLeft.Row && row <= square.bottomRight.Row)
                                        ||
                                        (segment.b.Row > square.topLeft.Row && segment.b.Row < square.bottomRight.Row)
                                        */
                                        square.topLeft.Column < to
                                        &&
                                        square.bottomRight.Column > from
                                    )
                                    {
                                        // intersects with our shape!
                                        Console.WriteLine($"intersects!");
                                        gfx.DrawLine(Pens.Red, new PointF(segment.a.Column * scale, segment.a.Row * scale), new PointF(segment.b.Column * scale, segment.b.Row * scale));
                                        match = false;
                                        break;
                                    }
                                    else
                                    {
                                        Console.WriteLine($"On same col, not row!");
                                        gfx.DrawLine(Pens.Blue, new PointF(segment.a.Column * scale, segment.a.Row * scale), new PointF(segment.b.Column * scale, segment.b.Row * scale));
                                    }
                                }
                            }
                        }
                        gfx.Flush();
                    }
                    ++areaId;
                    if (match)
                    {
                        if (p2 == 0) p2 = candidate.surface;
                        if (!Test) bmp.Save($"real_{(areaId).ToString().PadLeft(5, '0')}.png", System.Drawing.Imaging.ImageFormat.Png);
                        break;
                    }
                    if (Test) bmp.Save($"testv2_{(areaId).ToString().PadLeft(5, '0')}.png", System.Drawing.Imaging.ImageFormat.Png);
                }
                /*
                foreach (var segment in lineSegments)
                {
                    row = segment.a.Row;
                    col = segment.a.Column;
                    if (row == segment.b.Row)
                    {
                        if (Test) Console.Write($"Line on row {row} from col {segment.a.Column} to {segment.b.Column}...");
                        // horizontal
                        if (row <= square.topLeft.Row || row >= square.bottomRight.Row)
                        {
                            // does not intersect
                            if (Test) Console.WriteLine($"does not intersect!");
                        }
                        else
                        {
                            // check col.
                            if (
                                (col > square.topLeft.Column && col < square.bottomRight.Column)
                                ||
                                (col >= square.topLeft.Column && col < square.bottomRight.Column)
                                ||
                                (col > square.topLeft.Column && col <= square.bottomRight.Column)
                                ||
                                (segment.b.Column > square.topLeft.Column && segment.b.Column < square.bottomRight.Column)
                            )
                            {
                                if (Test) Console.WriteLine($"intersects!");
                                // intersects with our shape!
                                match = false;
                                break;
                            }
                            else
                            {
                                if (Test) Console.WriteLine($"On same row, not column!");
                            }
                        }
                    }
                    // vertical (e.g. column == column)
                    else
                    {
                        if (Test) Console.Write($"Line on column {col} from row {segment.a.Row} to {segment.b.Row}...");
                        if (col <= square.topLeft.Column || col >= square.bottomRight.Column)
                        {
                            // does not intersect
                            if (Test) Console.WriteLine($"does not intersect!");
                        }
                        else
                        {
                            // check rows.
                            if (
                                (row > square.topLeft.Row && row < square.bottomRight.Row)
                                ||
                                (row >= square.topLeft.Row && row < square.bottomRight.Row)
                                ||
                                (row > square.topLeft.Row && row <= square.bottomRight.Row)
                                ||
                                (segment.b.Row > square.topLeft.Row && segment.b.Row < square.bottomRight.Row)
                            )
                            {
                                // intersects with our shape!
                                if (Test) Console.WriteLine($"intersects!");
                                match = false;
                                break;
                            }
                            else
                            {
                                if (Test) Console.WriteLine($"On same col, not now!");
                            }
                        }
                    }
                }
                if (match)
                {
                    p2 = candidate.surface;
                    break;
                }
                */
            }
            Console.WriteLine($"Part 2: {p2}");
            Debug.Assert(p2 == (ulong)(Test ? AnswerP2Test : AnswerP2), "You broke Part 2!");
        }
        #region For Benchmark.NET
        [Benchmark]
        public void Day9_Part1() => Part1();
        [Benchmark]
        public void Day9_Part2() => Part2();
        #endregion
    }
}
