using System.Text;

public class HuffmanCoding
{
    // method for computing the frequencies of the characters
    public static Dictionary<char, int> CalculateFrequencies(string filePath)
    {
        Dictionary<char, int> frequencyDict = new Dictionary<char, int>();

        using (StreamReader reader = new StreamReader(filePath, Encoding.UTF8))
        {
            int ch;
            while ((ch = reader.Read()) != -1)
            {
                char c = (char)ch;
                if (frequencyDict.ContainsKey(c))
                    frequencyDict[c]++;
                else
                    frequencyDict[c] = 1;
            }
        }

        return frequencyDict;
    }

    public static HuffmanNode BuildHuffmanTree(Dictionary<char, int> frequencyDict)
    {
        // create a minheap using the frequencies
        var priorityQueue = new PriorityQueue<HuffmanNode, int>();

        foreach (var entry in frequencyDict)
        {
            var node = new HuffmanNode(entry.Key, entry.Value);
            priorityQueue.Enqueue(node, entry.Value);
        }

        // we continue until there's only one node left
        while (priorityQueue.Count > 1)
        {
            // find 2 nodes with the least frequency
            var firstNode = priorityQueue.Dequeue();
            var secondNode = priorityQueue.Dequeue();

            // create a parent node with a combined frequency (the sum)
            var combinedNode = new HuffmanNode('\0', firstNode.Frequency + secondNode.Frequency);
            combinedNode.Left = firstNode;
            combinedNode.Right = secondNode;

            // add the combined node to the priority queue
            priorityQueue.Enqueue(combinedNode, combinedNode.Frequency);
        }

        // the last remaining node in the priority queue is the root of the huffman tree
        return priorityQueue.Dequeue();
    }

    public static void GenerateHuffmanCodes(HuffmanNode node, string code, Dictionary<char, string> huffmanCodes)
    {
        if (node == null)
            return;

        // if leaf node, add the code for the character
        if (node.Left == null && node.Right == null)
        {
            huffmanCodes[node.Character] = code;
        }

        // do it recursively for the left and right subtree
        GenerateHuffmanCodes(node.Left, code + "0", huffmanCodes);
        GenerateHuffmanCodes(node.Right, code + "1", huffmanCodes);
    }

    public static Dictionary<char, string> GetHuffmanCodes(Dictionary<char, int> frequencyDict)
    {
        // construct the huffman tree
        var root = BuildHuffmanTree(frequencyDict);

        // generate huffman codes
        var huffmanCodes = new Dictionary<char, string>();
        GenerateHuffmanCodes(root, "", huffmanCodes);

        return huffmanCodes;
    }
}