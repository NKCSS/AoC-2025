using AoC2025;
using BenchmarkDotNet.Attributes;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using NKCSS.AoC;
using System.Diagnostics;
using System.Numerics;
using static AoC2025.Day12;

namespace AoC2025
{
    public class Day12 : Solution
    {
        const bool Visualize = false;
        const bool Test = false;
        const char ShapeMarker = '#', EmptyMaker = '.';
        const long AnswerP1Test = 2, AnswerP2Test = -1, AnswerP1 = -1L, AnswerP2 = -1L;
        List<((int width, int height) size, List<int> occurences)> areas;
        List<Shape> shapes;
        Dictionary<ushort, List<Shape>> shapeVariations;
        public record Shape
        {
            public byte[] Mask { get; private set; }
            public const int Width = 3;
            public const int Height = 3;
            public ushort Hash { get; private set; }
            public Shape(string[] raw)
                : this([.. AsBitMask(raw)])
            { }
            public Shape(byte[] mask)
            {
                Mask = mask;
                Hash = (ushort)(((ushort)Mask[0] << 4) + ((ushort)Mask[1] << 1) + ((ushort)Mask[2] >> 2));
            }
            public Shape(ushort hash)
                : this(MaskFromHash(hash))
            {
            }
            public Shape Copy() => new Shape([..Mask]);
            public Shape RotateRight()
            {
                byte[] newMask;
                //HACK: Our shapes are 3x3 shifted over two; we will hand-remap the values.
                byte a, b, c;
                a = (byte)
                    (
                        (((Mask[0] & 0b000_100_00) >> 2) & 0b000_001_00)
                        +
                        (((Mask[1] & 0b000_100_00) >> 1) & 0b000_010_00)
                        +
                        ((Mask[2] & 0b000_100_00) & 0b000_100_00)
                    );
                b = (byte)
                    (
                        (((Mask[2] & 0b000_010_00) << 1) & 0b000_100_00)
                        +
                        (Mask[1] & 0b000_010_00)
                        +
                        (((Mask[0] & 0b000_010_00) >> 1) & 0b000_001_00)
                    );
                c = (byte)
                    (
                        (((Mask[2] & 0b000_001_00) << 2) & 0b000_100_00)
                        +
                        (((Mask[1] & 0b000_001_00) << 1) & 0b000_010_00)
                        +
                        ((Mask[0] & 0b000_001_00) & 0b000_001_00)
                    );
                return new Shape([a, b, c]);
            }
            public Shape Flip(bool xAxis)
            {
                if (xAxis)
                {
                    // reverse line contents
                    // Our masks are << 2 and 3 wide, so when we flip the bits,
                    // pre-shift them one over, so they are still << 2 after flipping.
                    return new Shape([.. Mask.Select(x => ((byte)(x << 1)).Reverse())]);
                }
                else
                {
                    // reverse lines
                    return new Shape([..Mask.Reverse()]);
                }
            }
            public bool IntersectsAny(int sourceX, int sourceY, List<(Shape shape, int x, int y)> placed)
            {
                foreach (var p in placed)
                {
                    if (p.shape.Intersects(this, 0, 0, sourceX - p.x, sourceY - p.y))
                        return true;
                }
                return false;
            }
            public bool Intersects(Shape other, int sourceX, int sourceY, int otherX, int otherY)
            {
                // Shapes are 3x3, if they are further than 2 away, there won't be an intersection.
                if (Math.Abs(sourceX - otherX) >= 3 || Math.Abs(sourceY - otherY) >= 3) return false;
                const byte ZERO = 0;
                int minX = Math.Min(sourceX, otherX), minY = Math.Min(sourceY, otherY), maxX = Math.Max(sourceX + Width, otherX + Shape.Width), maxY = Math.Max(sourceY + Height, otherY + Shape.Height);
                int ourY, theirY;
                for (int y = minY; y < maxY; ++y)
                {
                    ourY = y - sourceY;
                    theirY = y - otherY;
                    byte ours = ourY < 0 || ourY >= Height ? ZERO : Mask[ourY];
                    byte theirs = theirY < 0 || theirY >= Shape.Height ? ZERO : other.Mask[theirY];
                    if (ours == 0 || theirs == 0) continue;
                    if (otherX > 0) theirs >>= otherX;
                    else if (otherX < 0) theirs <<= -otherX;
                    if (sourceX > 0) ours >>= sourceX;
                    else if (sourceX < 0) ours <<= -sourceX;
                    if ((theirs & ours) != 0) return true;
                }
                return false;
            }
            public IEnumerable<string> PrintMask()
            {
                foreach(byte m in Mask)
                {
                    yield return $"{(m >> 2):B3}".Replace('1', ShapeMarker).Replace('0', EmptyMaker);
                }
            }
            public List<Shape> UniqueRotationsAndFlips()
            {
                Dictionary<ulong, Shape> uniques = [];
                Shape next = this;
                for (int flip = 0; flip < 2; ++flip)
                {
                    for (int i = 0; i < 4; ++i)
                    {
                        if (!uniques.ContainsKey(next.Hash))
                        {
                            uniques.Add(next.Hash, next);
                        }
                        next = next.RotateRight();
                    }
                    next = next.Flip(true);
                }
                return [.. uniques.Values];
            }
            #region Static Helpers
            public static byte[] MaskFromHash(uint hash)
            => [
                (byte)((hash >> 4) & 0b000_111_00),
                (byte)((hash >> 1) & 0b000_111_00),
                (byte)((hash << 2) & 0b000_111_00),
                ];
            public static IEnumerable<byte> AsBitMask(string[] raw)
            {
                foreach (string line in raw)
                {
                    byte mask = 0;
                    for (int i = 0; i < Width; ++i)
                    {
                        if (line[i] == ShapeMarker) mask += (byte)(1 << i + 2);
                    }
                    yield return mask;
                }
            }
            public static (int width, int height) GetShapeDimensions(string[] raw)
            {
                int width, height, w = raw.Length, h = raw[0].Length;
                width = height = 0;
                string line;
                for (int y = 0; y < h; ++y)
                {
                    line = raw[y];
                    for (int x = 0; x < w; ++x)
                    {
                        if (line[x] == ShapeMarker)
                        {
                            if (x > width) width = x;
                            if (y > height) height = y;
                        }
                    }
                }
                return (width, height);
            }
            public static uint CombineHashes(ushort a, ushort b)
                => a > b ? ((uint)a << 16) + b : ((uint)b << 16) + a;
            public static IEnumerable<(Shape a, Shape b)> GetUniqueCombos(List<Shape> a, List<Shape> b)
            {
                HashSet<uint> seen = [];
                foreach (Shape x in a)
                    foreach (Shape y in b)
                    {
                        uint key = CombineHashes(x.Hash, y.Hash);
                        if (!seen.Contains(key))
                        {
                            seen.Add(key);
                            yield return (x, y);
                        }
                    }
            }
            public static IEnumerable<(int x, int y)> TryFit(List<Shape> a, List<Shape> b)
            {
                foreach (var pair in GetUniqueCombos(a, b))
                {
                    foreach (var way in TryFit(pair.a, pair.b))
                        yield return way;
                }
            }
            public static IEnumerable<(int x, int y)> TryFit(Shape a, Shape b)
            {
                for (int y = -2; y <= 2; ++y)
                {
                    for (int x = -2; x <= 2; ++x)
                    {
                        if (x == 0 && y == 0) continue;
                        if (!a.Intersects(b, 0, 0, x, y))
                        {
                            yield return (x, y);
                        }
                    }
                }
            }
            #endregion
        }
        public Day12() : base(12) {
            if (Test)
            {
                // Paste test input here...
                Input = """
                    0:
                    ###
                    ##.
                    ##.

                    1:
                    ###
                    ##.
                    .##

                    2:
                    .##
                    ###
                    ##.

                    3:
                    ##.
                    ###
                    ##.

                    4:
                    ###
                    #..
                    ###

                    5:
                    ###
                    .#.
                    ###

                    4x4: 0 0 0 0 2 0
                    12x5: 1 0 1 0 2 2
                    12x5: 1 0 1 0 3 2
                    """.ReplaceLineEndings("\n");
            }
            PrepareInput();
            Part1();
            Part2();
        }
        void PrepareInput()
        {
            string[] sections = [.. Input.SplitBy("\n\n")];
            shapes = [..sections[0..^1].Select(x => new Shape(x.ToLines()[1..]))];
            areas = [..sections[^1]
                .ToLines()
                .Select(
                    x => x.SplitBy(": ")
                    .As(
                        x => x
                            .Split('x')
                            .AsInt32s()
                            .AsValueTuple(),
                        x => x
                            .SplitWhiteSpaceAsInt32s()
                            .ToList()
                    )
                )];
            shapeVariations = shapes.ToDictionary(x => x.Hash, x => x.UniqueRotationsAndFlips());
        }
        static List<int> ExpandOccurrences(List<int> counts)
        {
            var result = new List<int>();
            for (int i = 0; i < counts.Count; i++)
                result.AddRange(Enumerable.Repeat(i, counts[i]));
            return result;
        }
        void Part1()
        {
            long p1 = 0L;
            /* Test Suite
            var shapeA = new Shape(new[] { "###", ".#.", "###" });
            bool testA = shapeA.Intersects(shapeA, 0, 0, 2, 1);
            bool testB = shapeA.Intersects(shapeA, 0, 0, 1, 1);
            bool testC = shapeA.Intersects(shapeA, 2, 1, 0, 0);
            bool testD = shapeA.Intersects(shapeA, 1, 1, 0, 0);
            Console.WriteLine($"{testA}, {testB}, {testC}, {testD}");
            var shapeB = new Shape(new[] { "###", ".#.", "##." });
            var referenceB = shapeB.Copy();
            for (int i = 0; i < 4; ++i)
            {
                foreach (string line in shapeB.PrintMask())
                {
                    Console.WriteLine(line);
                }
                Console.WriteLine();
                shapeB = shapeB.RotateRight();
            }
            Debug.Assert(shapeB.Mask.SequenceEqual(referenceB.Mask), "Rotation failed!");
            var flipBX = shapeB.Flip(true);
            Console.WriteLine($"Flipped (X):");
            foreach (string line in flipBX.PrintMask())
            {
                Console.WriteLine(line);
            }
            Console.WriteLine();
            var flipBY = shapeB.Flip(false);
            Console.WriteLine($"Flipped (Y):");
            foreach (string line in flipBY.PrintMask())
            {
                Console.WriteLine(line);
            }
            Console.WriteLine();
            Debug.Assert(flipBX.Flip(true).Mask.SequenceEqual(referenceB.Mask), "Flip X-Axis failed!");
            Debug.Assert(flipBY.Flip(false).Mask.SequenceEqual(referenceB.Mask), "Flip X-Axis failed!");

            ushort bHash = shapeB.Hash;

            Shape fromHash = new Shape(bHash, shapeB.Width, shapeB.Height);
            Debug.Assert(fromHash.Hash == bHash, "From Hash logic mismatch");
            Debug.Assert(fromHash.Mask.SequenceEqual(shapeB.Mask), "From Hash created different mask");

            List<Shape> bVariations = shapeB.UniqueRotationsAndFlips();

            List<Shape> aVariations = shapeA.UniqueRotationsAndFlips();
            Console.WriteLine($"b: {bVariations.Count}");
            Console.WriteLine($"a: {aVariations.Count}");
            */
            /*
            foreach(var shape in shapes)
            {
                foreach (string line in shape.PrintMask())
                {
                    Console.WriteLine(line);
                }
                var variations = shape.UniqueRotationsAndFlips();
                Console.WriteLine($"{variations.Count} variations");
                Console.WriteLine();
            }*/
            /*
            var shape = shapes[^2];
            foreach (string line in shape.PrintMask())
            {
                Console.WriteLine(line);
            }
            var ways = Shape.TryFit(shapeVariations[shape.Hash], shapeVariations[shape.Hash]);
            foreach(var way in ways)
            {
                Console.WriteLine($"x: {way.x}, y: {way.y}");
            }
            */
            const bool stupid = true;
            if (stupid)
            {
                foreach (var a in areas)
                {
                    // apparently, you only need to check the amount of 3x3 shapes that can fit inside the area 😅
                    int surface = (a.size.width / 3) * (a.size.height / 3);
                    int packages = a.occurences.Sum();
                    Console.WriteLine($"Need {packages}, have {surface}");
                    if (packages <= surface) ++p1;
                }
            }
            else
                foreach (var a in areas)
                {
                    List<int> asIdList = [
                        ..a.occurences
                    .WithIndexes()
                    .Where(x => x.value > 0)
                    .SelectMany(x => Enumerable.Repeat(x.index, x.value))
                    ];
                    if (Visualize)
                    {
                        Console.Clear();
                        Console.SetCursorPosition(0, 0);
                    }
                    bool fits = Solve(0, asIdList, [], a.size.width, a.size.height);
                    if (fits)
                    {
                        ++p1;
                        Console.WriteLine($"💪 we can fit [x{string.Join(", x", a.occurences)}] in a {a.size.width}x{a.size.height}");
                    }
                    else
                    {
                        Console.WriteLine($"😭 we can't fit [x{string.Join(", x", a.occurences)}] in a {a.size.width}x{a.size.height}");
                    }
                    if (Visualize)
                    {
                        Console.ReadLine();
                    }
                }
            // 1000 answer too high.
            Console.WriteLine($"Part 1: {p1}");
            Debug.Assert(p1 == (Test ? AnswerP1Test : AnswerP1), "You broke Part 1!");
        }
        static bool FitsInBounds(Shape s, int x, int y, int boardW, int boardH)
        {
            if (x < 0 || y < 0) return false;
            if (x + Shape.Width > boardW) return false;
            if (y + Shape.Height > boardH) return false;
            return true;
        }
        bool Solve(int index,List<int> shapeIds, List<(Shape shape, int x, int y)> placed, int areaW, int areaH)
        {
            if (index == shapeIds.Count)
            {
                //TODO: print full map with 'placed'.
                return true;
            }
            int id = shapeIds[index];
            var variants = shapeVariations[shapes[id].Hash];
            foreach (var shape in variants)
            {
                for (int y = 0, maxY = areaH - Shape.Height; y <= maxY; y++)
                {
                    for (int x = 0, maxX = areaW - Shape.Width; x <= maxX; x++)
                    {
                        if (
                            // in bounds of area
                            x < 0
                            ||
                            y < 0
                            ||
                            x + Shape.Width > areaW
                            ||
                            y + Shape.Height > areaH
                            || // check intersection
                            shape.IntersectsAny(x, y, placed)
                        ) continue;

                        placed.Add((shape, x, y));
                        if (Visualize)
                        {
                            Console.SetCursorPosition(x, y);
                            Console.ForegroundColor = ConsoleColor.Green;
                            int lineOffset = 0;
                            foreach (string line in shape.PrintMask())
                            {
                                for (int i = 0; i < 3; ++i)
                                {
                                    if (line[i] == ShapeMarker)
                                        Console.Write(ShapeMarker);
                                    else
                                        Console.SetCursorPosition(x + i + 1, y + lineOffset);
                                }
                                Console.SetCursorPosition(x, y + ++lineOffset);
                            }
                            Thread.Sleep(50);
                        }

                        if (Solve(index + 1, shapeIds, placed, areaW, areaH))
                            return true;

                        // backtrack
                        var removed = placed[placed.Count - 1];
                        placed.RemoveAt(placed.Count - 1);
                        if (Visualize)
                        {
                            Console.SetCursorPosition(removed.x, removed.y);
                            Console.ForegroundColor = ConsoleColor.Red;
                            int lineOffset = 0;                            
                            foreach (string line in shape.PrintMask())
                            {
                                for (int i = 0; i < 3; ++i)
                                {
                                    if (line[i] == ShapeMarker)
                                        Console.Write(ShapeMarker);
                                    else
                                        Console.SetCursorPosition(x + i + 1, y + lineOffset);
                                }
                                Console.SetCursorPosition(x, y + ++lineOffset);
                            }
                            Thread.Sleep(100);
                            Console.SetCursorPosition(removed.x, removed.y);
                            lineOffset = 0;
                            for (int i = 0; i < 3; ++i)
                            {
                                Console.Write("   ");
                                Console.SetCursorPosition(x, y + ++lineOffset);
                            }
                        }
                    }
                }
            }
            return false;
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
        public void Day12_PrepareInput() => PrepareInput();
        [Benchmark]
        public void Day12_Part1() => Part1();
        [Benchmark]
        public void Day12_Part2() => Part2();
        #endregion
    }
}
