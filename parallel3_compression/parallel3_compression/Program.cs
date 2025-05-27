// Program.cs
using System;
using System.Diagnostics;

class Program
{
    static void Main()
    {
        string inputFile = "data/250mbfile.txt";
        string outputBinaryFile = "data/compressed.bin";
        string outputTextFile = "data/decompressed.txt";

        if (!File.Exists(inputFile))
        {
            Console.WriteLine("Error: File not found!");
            return;
        }

        Stopwatch stopWatch = Stopwatch.StartNew();
        var frequencyDict = HuffmanCoding.CalculateFrequenciesPLINQ(inputFile);
        stopWatch.Stop();
        Console.WriteLine($"Frequency calculation: {stopWatch.ElapsedMilliseconds} ms");

        stopWatch.Restart();
        var huffmanCodes = HuffmanCoding.GetHuffmanCodes(frequencyDict);
        stopWatch.Stop();
        Console.WriteLine($"Huffman code generation: {stopWatch.ElapsedMilliseconds} ms");

        stopWatch.Restart();
        Compression.CompressPLINQ(inputFile, outputBinaryFile, huffmanCodes);
        stopWatch.Stop();
        Console.WriteLine($"Compression (PLINQ): {stopWatch.ElapsedMilliseconds} ms");

        stopWatch.Restart();
        Decompression.DecompressPLINQ(outputBinaryFile, outputTextFile, huffmanCodes);
        stopWatch.Stop();
        Console.WriteLine($"Decompression (PLINQ): {stopWatch.ElapsedMilliseconds} ms");
    }
}