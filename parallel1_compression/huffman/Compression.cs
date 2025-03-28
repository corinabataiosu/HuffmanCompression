using System.Collections.Concurrent;
using System.Text;

public static class Compression
{
    public static void CompressParallel(string inputFile, string outputBinaryFile, Dictionary<char, string> huffmanCodes)
    {
        var compressedLines = new ConcurrentBag<(int, byte[])>();
        
        // read all lines from the input file
        string[] lines = File.ReadAllLines(inputFile, Encoding.UTF8);
        
        // parallel compression of each line
        Parallel.ForEach(lines, (line, state, index) =>
        {
            List<byte> compressedData = new List<byte>();
            int bitCount = 0;
            byte currentByte = 0;

            // convert each character to its corresponding Huffman code
            foreach (char c in line)
            {
                if (!huffmanCodes.TryGetValue(c, out string huffCode))
                    continue; 

                // compress each bit of the huffman code
                foreach (char bit in huffCode)
                {
                    if (bit == '1')
                        currentByte |= (byte)(1 << (7 - bitCount)); // set the bit to 1 if bit='1'

                    bitCount++;
                    if (bitCount == 8)
                    {
                        compressedData.Add(currentByte);
                        currentByte = 0;
                        bitCount = 0;
                    }
                }
            }

            // if there are remaining bits, add the last byte
            if (bitCount > 0)
            {
                compressedData.Add(currentByte);
                currentByte = 0;
                bitCount = 0;
            }

            compressedLines.Add(((int)index, compressedData.ToArray()));
        });

        // write the compressed data to the output file
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