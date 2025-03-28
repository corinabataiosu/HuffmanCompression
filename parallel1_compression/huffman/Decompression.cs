using System.Text;
using System.Collections.Concurrent;

public static class Decompression
{
    public static void DecompressParallel(string inputBinaryFile, string outputTextFile, Dictionary<char, string> huffmanCodes)
    {
        // reverse the Huffman codes to decode the binary data back to characters
        var reversedHuffmanCodes = huffmanCodes.ToDictionary(kvp => kvp.Value, kvp => kvp.Key);
        //  we use a concurrent bag to store the decompressed lines
        var decompressedLines = new ConcurrentBag<(int, string)>(); 

        using (FileStream fs = new FileStream(inputBinaryFile, FileMode.Open))
        using (BinaryReader reader = new BinaryReader(fs))
        {
            List<(int, byte[])> compressedLines = new List<(int, byte[])>();

            // read the compressed data from the binary file
            while (reader.BaseStream.Position < reader.BaseStream.Length)
            {
                int index = compressedLines.Count; 
                int length = reader.ReadInt32(); // read the length of the compressed data
                byte[] data = reader.ReadBytes(length); // read the compressed data
                compressedLines.Add((index, data));
            }

            // parallel processing of each compressed line
            Parallel.ForEach(compressedLines, (entry) =>
            {
                int index = entry.Item1;
                byte[] data = entry.Item2;
                StringBuilder binaryString = new();

                // convert each byte array to a binary string
                foreach (byte b in data)
                {
                    binaryString.Append(Convert.ToString(b, 2).PadLeft(8, '0'));
                }

                // decode the binary string using the reversed Huffman codes
                StringBuilder decodedLine = new();
                string currentCode = "";

                foreach (char bit in binaryString.ToString())
                {
                    currentCode += bit;
                    if (reversedHuffmanCodes.TryGetValue(currentCode, out char decodedChar))
                    {
                        decodedLine.Append(decodedChar); // add the decoded character
                        currentCode = ""; // reset the current code
                    }
                }

                decompressedLines.Add((index, decodedLine.ToString())); // store the decompressed line with its index
            });
        }

        // write the decompressed lines to the output text file in the correct order
        using (StreamWriter writer = new StreamWriter(outputTextFile, false, Encoding.UTF8))
        {
            foreach (var line in decompressedLines.OrderBy(t => t.Item1))
            {
                writer.WriteLine(line.Item2);
            }
        }
    }
}