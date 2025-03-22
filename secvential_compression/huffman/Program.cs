using System.Diagnostics;

class Program
{
    static void Main()
    {
        string inputFile = "data/10mbfile.txt";
        string outputBinaryFile = "data/compressed.bin";
        string outputTextFile = "data/decompressed.txt";

        if (!File.Exists(inputFile))
        {
            Console.WriteLine("Error: File not found!");
            return;
        }

        // read the file and compute the frequency of the characters
        Stopwatch stopWatch = Stopwatch.StartNew();
        Dictionary<char, int> frequencyDict = HuffmanCoding.CalculateFrequencies(inputFile);
        stopWatch.Stop();
        Console.WriteLine($"Read file and calculate frequencies completed in {stopWatch.ElapsedMilliseconds} ms");

        // generate the huffman codes
        stopWatch.Restart();
        Dictionary<char, string> huffmanCodes = HuffmanCoding.GetHuffmanCodes(frequencyDict);
        stopWatch.Stop();
        Console.WriteLine($"Generate Huffman codes completed in {stopWatch.ElapsedMilliseconds} ms");

        Console.WriteLine("Huffman Codes:");
        foreach (var pair in huffmanCodes)
        {
            Console.WriteLine($"'{pair.Key}': {pair.Value}");
        }

        // compress the text
        stopWatch.Restart();
        Compression.Compress(inputFile, outputBinaryFile, huffmanCodes);
        stopWatch.Stop();
        Console.WriteLine($"Compression complete in {stopWatch.ElapsedMilliseconds} ms!");

        // decompress the binary file
        Decompression.Decompress(outputBinaryFile, outputTextFile, huffmanCodes);
    }
}
