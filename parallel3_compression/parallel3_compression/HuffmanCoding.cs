using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public static class HuffmanCoding
{
    public static Dictionary<char, int> CalculateFrequenciesPLINQ(string filePath)
    {
        var frequencyDict = File.ReadLines(filePath, Encoding.UTF8)
            .AsParallel()
            .SelectMany(line => line)
            .GroupBy(c => c)
            .Select(group => new { Char = group.Key, Count = group.Count() })
            .ToDictionary(g => g.Char, g => g.Count);

        return frequencyDict;
    }

    //public static Dictionary<char, int> CalculateFrequenciesParallel(string filePath)
    //{
    //    var frequencyDict = new ConcurrentDictionary<char, int>();

    //    Parallel.ForEach(File.ReadLines(filePath, Encoding.UTF8),
    //        () => new Dictionary<char, int>(),
    //        (line, state, localDict) =>
    //        {
    //            foreach (char c in line)
    //            {
    //                if (localDict.ContainsKey(c)) localDict[c]++;
    //                else localDict[c] = 1;
    //            }
    //            return localDict;
    //        },
    //        localDict =>
    //        {
    //            foreach (var kvp in localDict)
    //                frequencyDict.AddOrUpdate(kvp.Key, kvp.Value, (_, old) => old + kvp.Value);
    //        });

    //    return frequencyDict.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
    //}

    public static Dictionary<char, string> GetHuffmanCodes(Dictionary<char, int> frequencyDict)
    {
        var root = BuildHuffmanTree(frequencyDict);
        var codes = new Dictionary<char, string>();
        GenerateHuffmanCodes(root, "", codes);
        return codes;
    }

    private static HuffmanNode BuildHuffmanTree(Dictionary<char, int> freqDict)
    {
        var queue = new PriorityQueue<HuffmanNode, int>();

        foreach (var kvp in freqDict)
            queue.Enqueue(new HuffmanNode(kvp.Key, kvp.Value), kvp.Value);

        while (queue.Count > 1)
        {
            var left = queue.Dequeue();
            var right = queue.Dequeue();
            var parent = new HuffmanNode('\0', left.Frequency + right.Frequency) { Left = left, Right = right };
            queue.Enqueue(parent, parent.Frequency);
        }

        return queue.Dequeue();
    }

    private static void GenerateHuffmanCodes(HuffmanNode node, string code, Dictionary<char, string> codes)
    {
        if (node == null) return;

        if (node.Left == null && node.Right == null)
            codes[node.Character] = code;

        GenerateHuffmanCodes(node.Left, code + "0", codes);
        GenerateHuffmanCodes(node.Right, code + "1", codes);
    }
}
