using System.Text;
using System.Diagnostics;

public class Decompression
{
    public static void Decompress(string inputBinaryFile, string outputTextFile, Dictionary<char, string> huffmanCodes)
    {
        // invert the huffman codes so they can be used for decoding
        Dictionary<string, char> reversedHuffmanCodes = new Dictionary<string, char>();
        foreach (var pair in huffmanCodes)
        {
            reversedHuffmanCodes[pair.Value] = pair.Key;
        }

        // call the actual decompressing method
        PerformDecompression(reversedHuffmanCodes, inputBinaryFile, outputTextFile);
    }

    private static void PerformDecompression(Dictionary<string, char> huffmanCodes, string inputBinaryFile, string outputTextFile)
    {
        if (!File.Exists(inputBinaryFile))
        {
            Console.WriteLine("Error: Compressed file not found!");
            return;
        }

        Stopwatch decompressTime = Stopwatch.StartNew();
        using (FileStream fs = new FileStream(inputBinaryFile, FileMode.Open))
        using (BinaryReader reader = new BinaryReader(fs))
        using (StreamWriter writer = new StreamWriter(outputTextFile, false, Encoding.UTF8))
        {
            string currentCode = "";
            long fileSize = fs.Length;

            while (reader.BaseStream.Position < fileSize)
            {
                byte b = reader.ReadByte();

                // convert the byte to 8 bits
                string bits = Convert.ToString(b, 2).PadLeft(8, '0');

                foreach (char bit in bits)
                {
                    currentCode += bit;
                    if (huffmanCodes.ContainsKey(currentCode))
                    {
                        writer.Write(huffmanCodes[currentCode]); // write directly into the file
                        currentCode = ""; // reset the code
                    }
                }
            }
        }

        decompressTime.Stop();
        Console.WriteLine($"Decompression complete in {decompressTime.ElapsedMilliseconds} ms!");
    }
}