using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public static class Compression
{
    public static void CompressPLINQ(string inputFile, string outputBinaryFile, Dictionary<char, string> huffmanCodes)
    {
        var lines = File.ReadAllLines(inputFile, Encoding.UTF8);

        var compressedLines = lines
            .AsParallel() // convert lines to a parallel query so it can be processed in parallel
            .AsOrdered() // keeps the order of the original lines
            .Select((line, index) => // for every line, the index is extracted and it executes the following actions
            {
                List<byte> bytes = new();
                int bitCount = 0; // counter for currentByte
                byte currentByte = 0;

                foreach (char c in line)
                {
                    if (!huffmanCodes.TryGetValue(c, out string code)) continue; // check if the charcater is in the dictionary

                    foreach (char bit in code)
                    {
                        if (bit == '1')
                        {
                            currentByte |= (byte)(1 << (7 - bitCount));
                        }
                            
                        bitCount++;
                        if (bitCount == 8) // add the 8 bits to the bytes variable
                        {
                            bytes.Add(currentByte);
                            currentByte = 0;
                            bitCount = 0;
                        }
                    }
                }

                if (bitCount > 0) // check if there are any remaining bits
                    bytes.Add(currentByte);

                return (index, bytes.ToArray());
            })
            .ToList(); // return the result as a list of tuples containing the index and the compressed byte array

        using var fs = new FileStream(outputBinaryFile, FileMode.Create);
        using var writer = new BinaryWriter(fs);

        foreach (var (index, data) in compressedLines.OrderBy(x => x.index))
        {
            writer.Write(data.Length);
            writer.Write(data);
        }
    }
}
