using System.Text;

public static class Compression
{
    // method for text compression
    public static void Compress(string inputFile, string outputBinaryFile, Dictionary<char, string> huffmanCodes)
    {
        using (StreamReader reader = new StreamReader(inputFile, Encoding.UTF8))
        using (FileStream fs = new FileStream(outputBinaryFile, FileMode.Create))
        using (BinaryWriter writer = new BinaryWriter(fs))
        {
            int bitCount = 0;
            byte currentByte = 0;

            int ch;
            while ((ch = reader.Read()) != -1)
            {
                char c = (char)ch;
                string huffCode = huffmanCodes[c];

                foreach (char bit in huffCode)
                {
                    if (bit == '1')
                        currentByte |= (byte)(1 << (7 - bitCount));

                    bitCount++;

                    if (bitCount == 8)
                    {
                        writer.Write(currentByte);
                        currentByte = 0;
                        bitCount = 0;
                    }
                }
            }

            // write the leftover bits (the ones that weren't written)
            if (bitCount > 0)
                writer.Write(currentByte);
        }
    }
}