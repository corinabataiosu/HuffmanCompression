using System.Collections.Concurrent;
using System.Text;

public static class HuffmanCoding
{
    public static Dictionary<char, int> CalculateFrequenciesParallel(string filePath)
    {
        // concurrent dictionary to store character frequencies
        var frequencyDict = new ConcurrentDictionary<char, int>();

        // read lines from the file in parallel and update frequencies
        Parallel.ForEach(File.ReadLines(filePath, Encoding.UTF8), () => new Dictionary<char, int>(), 
        (line, state, localDict) =>
        {
            // count the frequency of each character in the line
            foreach (char c in line)
            {
                if (localDict.ContainsKey(c))
                    localDict[c]++;
                else
                    localDict[c] = 1;
            }
            return localDict;
        }, 
        (localDict) =>
        {
            // merge the local dictionary into the global frequency dictionary

            foreach (var kvp in localDict)
            {
                frequencyDict.AddOrUpdate(kvp.Key, kvp.Value, (_, count) => count + kvp.Value);
            }
        });

        // return the final frequency dictionary
        return frequencyDict.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
    }


    public static Dictionary<char, string> GetHuffmanCodes(Dictionary<char, int> frequencyDict)
    {
        // build the huffman tree and generate codes
        var root = BuildHuffmanTree(frequencyDict);
        var huffmanCodes = new Dictionary<char, string>();
        GenerateHuffmanCodes(root, "", huffmanCodes);
        return huffmanCodes;
    }

    private static HuffmanNode BuildHuffmanTree(Dictionary<char, int> frequencyDict)
    {
        // priority queue to store the nodes of the huffman tree
        var priorityQueue = new PriorityQueue<HuffmanNode, int>();

        // add all characters with their frequencies to the priority queue
        foreach (var entry in frequencyDict)
        {
            var node = new HuffmanNode(entry.Key, entry.Value);
            priorityQueue.Enqueue(node, entry.Value);
        }

        // build the tree by combining the two least frequent nodes until only one node remains
        while (priorityQueue.Count > 1)
        {
            var firstNode = priorityQueue.Dequeue();
            var secondNode = priorityQueue.Dequeue();

            var combinedNode = new HuffmanNode('\0', firstNode.Frequency + secondNode.Frequency)
            {
                Left = firstNode,
                Right = secondNode
            };

            priorityQueue.Enqueue(combinedNode, combinedNode.Frequency);
        }

        // return the root of the huffman tree
        return priorityQueue.Dequeue();
    }

    private static void GenerateHuffmanCodes(HuffmanNode node, string code, Dictionary<char, string> huffmanCodes)
    {
        if (node == null) return;

        // if the node is a leaf node, store the code for the character
        if (node.Left == null && node.Right == null)
        {
            huffmanCodes[node.Character] = code;
        }

        // generate codes for the left and right subtrees
        // append '0' for left and '1' for right
        GenerateHuffmanCodes(node.Left, code + "0", huffmanCodes);
        GenerateHuffmanCodes(node.Right, code + "1", huffmanCodes);
    }
}