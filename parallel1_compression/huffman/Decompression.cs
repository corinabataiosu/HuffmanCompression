using System.Text;
using System.Collections.Concurrent;

public static class Decompression
{
    public static void DecompressParallel(string inputBinaryFile, string outputTextFile, Dictionary<char, string> huffmanCodes)
    {
        var reversedHuffmanCodes = huffmanCodes.ToDictionary(kvp => kvp.Value, kvp => kvp.Key);
        var decompressedLines = new ConcurrentBag<(int, string)>(); 

        using (FileStream fs = new FileStream(inputBinaryFile, FileMode.Open))
        using (BinaryReader reader = new BinaryReader(fs))
        {
            List<(int, byte[])> compressedLines = new List<(int, byte[])>();

            while (reader.BaseStream.Position < reader.BaseStream.Length)
            {
                int index = compressedLines.Count; 
                int length = reader.ReadInt32();
                byte[] data = reader.ReadBytes(length);
                compressedLines.Add((index, data));
            }

            Parallel.ForEach(compressedLines, (entry) =>
            {
                int index = entry.Item1;
                byte[] data = entry.Item2;
                StringBuilder binaryString = new();

                foreach (byte b in data)
                {
                    binaryString.Append(Convert.ToString(b, 2).PadLeft(8, '0'));
                }

                StringBuilder decodedLine = new();
                string currentCode = "";

                foreach (char bit in binaryString.ToString())
                {
                    currentCode += bit;
                    if (reversedHuffmanCodes.TryGetValue(currentCode, out char decodedChar))
                    {
                        decodedLine.Append(decodedChar);
                        currentCode = "";
                    }
                }

                decompressedLines.Add((index, decodedLine.ToString()));
            });
        }

        using (StreamWriter writer = new StreamWriter(outputTextFile, false, Encoding.UTF8))
        {
            foreach (var line in decompressedLines.OrderBy(t => t.Item1))
            {
                writer.WriteLine(line.Item2);
            }
        }
    }
}