using AoC2025;
using System.Diagnostics;
using System.Text;
Console.OutputEncoding = Encoding.UTF8;



//byte test = 0b1100_0010;
//byte reversed = 0;
//int delta;
//const int BitLength = 8;
//for (int i = 0; i < BitLength; ++i)
//{
//    delta = (BitLength - 1) - i * 2;
//    if (delta > 0)
//    {
//        reversed += (byte)((test & (1 << i)) << delta);
//    }
//    else
//    {
//        reversed += (byte)((test & (1 << i)) >> -delta);
//    }
//}
//Console.WriteLine($"{test:B8} -> {reversed:B8}");
//return;
new Day12();