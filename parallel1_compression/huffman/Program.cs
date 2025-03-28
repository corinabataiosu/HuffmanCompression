using System.Diagnostics;

class Program
{
    static void Main()
    {
        string inputFile = "data/2gbfile.txt";
        string outputBinaryFile = "data/compressed.bin";
        string outputTextFile = "data/decompressed.txt";

        // check if the input file exists
        if (!File.Exists(inputFile))
        {
            Console.WriteLine("Error: File not found!");
            return;
        }

        // read file and compute frequencies in parallel
        Stopwatch stopWatch = Stopwatch.StartNew();
        var frequencyDict = HuffmanCoding.CalculateFrequenciesParallel(inputFile);
        stopWatch.Stop();
        Console.WriteLine($"Read file and calculate frequencies completed in {stopWatch.ElapsedMilliseconds} ms");

        // generate Huffman codes (sequential)
        stopWatch.Restart();
        var huffmanCodes = HuffmanCoding.GetHuffmanCodes(frequencyDict);
        stopWatch.Stop();
        Console.WriteLine($"Generate Huffman codes completed in {stopWatch.ElapsedMilliseconds} ms");

        // compress in parallel
        stopWatch.Restart();
        Compression.CompressParallel(inputFile, outputBinaryFile, huffmanCodes);
        stopWatch.Stop();
        Console.WriteLine($"Compression complete in {stopWatch.ElapsedMilliseconds} ms!");

        // decompress in parallel
        stopWatch.Restart();
        Decompression.DecompressParallel(outputBinaryFile, outputTextFile, huffmanCodes);
        stopWatch.Stop();
        Console.WriteLine($"Decompression complete in {stopWatch.ElapsedMilliseconds} ms!");
    }
}