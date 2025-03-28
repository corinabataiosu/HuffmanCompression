using System.Collections.Concurrent;
using System.Text;

public static class HuffmanCoding
{
    public static Dictionary<char, int> CalculateFrequenciesParallel(string filePath)
    {
        var frequencyDict = new ConcurrentDictionary<char, int>();

        Parallel.ForEach(File.ReadLines(filePath, Encoding.UTF8), () => new Dictionary<char, int>(), 
        (line, state, localDict) =>
        {
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
            foreach (var kvp in localDict)
            {
                frequencyDict.AddOrUpdate(kvp.Key, kvp.Value, (_, count) => count + kvp.Value);
            }
        });

        return frequencyDict.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
    }


    public static Dictionary<char, string> GetHuffmanCodes(Dictionary<char, int> frequencyDict)
    {
        var root = BuildHuffmanTree(frequencyDict);
        var huffmanCodes = new Dictionary<char, string>();
        GenerateHuffmanCodes(root, "", huffmanCodes);
        return huffmanCodes;
    }

    private static HuffmanNode BuildHuffmanTree(Dictionary<char, int> frequencyDict)
    {
        var priorityQueue = new PriorityQueue<HuffmanNode, int>();

        foreach (var entry in frequencyDict)
        {
            var node = new HuffmanNode(entry.Key, entry.Value);
            priorityQueue.Enqueue(node, entry.Value);
        }

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

        return priorityQueue.Dequeue();
    }

    private static void GenerateHuffmanCodes(HuffmanNode node, string code, Dictionary<char, string> huffmanCodes)
    {
        if (node == null) return;

        if (node.Left == null && node.Right == null)
        {
            huffmanCodes[node.Character] = code;
        }

        GenerateHuffmanCodes(node.Left, code + "0", huffmanCodes);
        GenerateHuffmanCodes(node.Right, code + "1", huffmanCodes);
    }
}