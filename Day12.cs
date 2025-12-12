using AoC2025;
using BenchmarkDotNet.Attributes;
using NKCSS.AoC;
using System.Diagnostics;
using System.Numerics;
using static AoC2025.Day12;

namespace AoC2025
{
    public class Day12 : Solution
    {
        bool Test = true;
        const char ShapeMarker = '#', EmptyMaker = '.';
        const long AnswerP1Test = 2, AnswerP2Test = -1, AnswerP1 = -1L, AnswerP2 = -1L;
        List<((int width, int height) size, List<int> occurences)> areas;
        List<Shape> shapes;
        Dictionary<ushort, List<Shape>> shapeVariations;
        public record Shape
        {
            string[] Raw;
            public byte[] Mask { get; private set; }
            public int Width { get; private set; }
            public int Height { get; private set; }
            public ushort Hash { get; private set; }
            public Shape(string[] raw)
            {
                this.Raw = raw;
                Height = raw.Length;
                Width = raw[0].Length;
                Mask = [..AsBitMask()];
                Hash = (ushort)(((ushort)Mask[0] << 4) + ((ushort)Mask[1] << 1) + ((ushort)Mask[2] >> 2));
            }
            public static byte[] MaskFromHash(uint hash)
            => [
                (byte)((hash >> 4) & 0b000_111_00),
                (byte)((hash >> 1) & 0b000_111_00),
                (byte)((hash << 2) & 0b000_111_00),
                ];
            public Shape(byte[] mask, int width, int height)
            {
                Mask = mask;
                Width = width;
                Height = height;
                Hash = (ushort)(((ushort)Mask[0] << 4) + ((ushort)Mask[1] << 1) + ((ushort)Mask[2] >> 2));
            }
            public Shape(ushort hash, int width, int height)
            {
                Mask = MaskFromHash(hash);
                Width = width;
                Height = height;
                Hash = (ushort)(((ushort)Mask[0] << 4) + ((ushort)Mask[1] << 1) + ((ushort)Mask[2] >> 2));
            }
            public Shape Copy() => new Shape([..Mask], Width, Height);
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
                newMask = [a, b, c];
                return new Shape(newMask, width: Height, height: Width);
            }
            public Shape Flip(bool xAxis)
            {
                if (xAxis)
                {
                    // reverse line contents
                    // Our masks are << 2 and 3 wide, so when we flip the bits,
                    // pre-shift them one over, so they are still << 2 after flipping.
                    return new Shape([.. Mask.Select(x => ((byte)(x << 1)).Reverse())], Width, Height);                    
                }
                else
                {
                    // reverse lines
                    return new Shape([..Mask.Reverse()], Width, Height);
                }
            }
            public IEnumerable<byte> AsBitMask() {
                foreach(string line in Raw)
                {
                    byte mask = 0;
                    for (int i = 0; i < Width; ++i)
                    {
                        if (line[i] == ShapeMarker) mask += (byte)(1 << i + 2);
                    }
                    yield return mask;
                }
            }
            public bool Intersects(Shape other, int sourceX, int sourceY, int otherX, int otherY)
            {
                byte ZERO = 0;
                int minX = Math.Min(sourceX, otherX), minY = Math.Min(sourceY, otherY), maxX = Math.Max(sourceX + Width, otherX + other.Width), maxY = Math.Max(sourceY + Height, otherY + other.Height);
                int ourY, theirY;
                for (int y = minY; y < maxY; ++y)
                {
                    ourY = y - sourceY;
                    theirY = y - otherY;
                    byte ours = ourY < 0 || ourY >= Height ? ZERO : Mask[ourY];
                    byte theirs = theirY < 0 || theirY >= other.Height ? ZERO : other.Mask[theirY];
                    if (ours == 0 || theirs == 0) continue;
                    if (otherX > 0) theirs >>= otherX;
                    else if (otherX < 0) theirs <<= -otherX;
                    if (sourceX > 0) ours >>= sourceX;
                    else if (sourceX < 0) ours <<= -sourceX;
                    if ((theirs & ours) != 0) return true;
                }
                return false;
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
            public static uint CombineHashes(ushort a, ushort b)
                => a > b ? ((uint)a << 16) + b : ((uint)b << 16) + a;
            public static IEnumerable<(Shape a, Shape b)> GetUniqueCombos(List<Shape> a, List<Shape> b)
            {
                HashSet<uint> seen = [];
                foreach(Shape x in a)
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
                foreach(var pair in GetUniqueCombos(a, b))
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
                        if(!a.Intersects(b, 0, 0, x, y))
                        {
                            yield return (x, y);
                        }
                    }
                }
            }
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
        public void Day12_PrepareInput() => PrepareInput();
        [Benchmark]
        public void Day12_Part1() => Part1();
        [Benchmark]
        public void Day12_Part2() => Part2();
        #endregion
    }
}
