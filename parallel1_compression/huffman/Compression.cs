using System.Collections.Concurrent;
using System.Text;

public static class Compression
{
    public static void CompressParallel(string inputFile, string outputBinaryFile, Dictionary<char, string> huffmanCodes)
    {
        var compressedLines = new ConcurrentBag<(int, byte[])>();
        
        string[] lines = File.ReadAllLines(inputFile, Encoding.UTF8);
        
        Parallel.ForEach(lines, (line, state, index) =>
        {
            List<byte> compressedData = new List<byte>();
            int bitCount = 0;
            byte currentByte = 0;

            foreach (char c in line)
            {
                if (!huffmanCodes.TryGetValue(c, out string huffCode))
                    continue; 

                foreach (char bit in huffCode)
                {
                    if (bit == '1')
                        currentByte |= (byte)(1 << (7 - bitCount));

                    bitCount++;
                    if (bitCount == 8)
                    {
                        compressedData.Add(currentByte);
                        currentByte = 0;
                        bitCount = 0;
                    }
                }
            }

            if (bitCount > 0)
                compressedData.Add(currentByte);

            compressedLines.Add(((int)index, compressedData.ToArray()));
        });

        using (FileStream fs = new FileStream(outputBinaryFile, FileMode.Create))
        using (BinaryWriter writer = new BinaryWriter(fs))
        {
            foreach (var line in compressedLines.OrderBy(t => t.Item1))
            {
                writer.Write(line.Item2.Length);
                writer.Write(line.Item2);
            }
        }
    }
}