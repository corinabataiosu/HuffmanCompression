using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Text;
using System.Linq;

public static class Decompression
{
    public static void DecompressPLINQ(string inputBinaryFile, string outputTextFile, Dictionary<char, string> huffmanCodes)
    {
        var reversed = huffmanCodes.ToDictionary(kvp => kvp.Value, kvp => kvp.Key);
        var compressedLines = new List<(int, byte[])>();

        using (var fs = new FileStream(inputBinaryFile, FileMode.Open))
        using (var reader = new BinaryReader(fs))
        {
            while (reader.BaseStream.Position < reader.BaseStream.Length)
            {
                int index = compressedLines.Count; // the index of the position in the list (order)
                int len = reader.ReadInt32(); // read an integer that represents the length of the binary line
                byte[] data = reader.ReadBytes(len); // read the bytes of the binary line
                compressedLines.Add((index, data));
            }
        }

        var decompressed = compressedLines
            .AsParallel() // convert lines to a parallel query so it can be processed in parallel
            .AsOrdered() // keeps the order of the original lines
            .Select(entry =>
            {
                int idx = entry.Item1; // extract the index of the binary line
                byte[] data = entry.Item2; // extract the binary text (the line)
                StringBuilder bits = new();

                foreach (byte b in data)
                    bits.Append(Convert.ToString(b, 2).PadLeft(8, '0')); // converts every 8 bits to a byte

                StringBuilder decoded = new();
                string current = "";

                foreach (char bit in bits.ToString())
                {
                    current += bit;
                    if (reversed.TryGetValue(current, out char c))
                    {
                        decoded.Append(c);
                        current = "";
                    }
                }

                return (idx, decoded.ToString());
            })
            .ToList();

        using var writer = new StreamWriter(outputTextFile, false, Encoding.UTF8);
        foreach (var line in decompressed.OrderBy(x => x.idx))
            writer.WriteLine(line.Item2);
    }
}

