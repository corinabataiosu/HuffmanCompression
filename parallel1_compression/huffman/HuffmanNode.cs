public class HuffmanNode
{
    public char Character { get; set; }
    public int Frequency { get; set; }
    public HuffmanNode Left { get; set; }
    public HuffmanNode Right { get; set; }
    
    public HuffmanNode(char character, int frequency)
    {
        Character = character;
        Frequency = frequency;
        Left = null;
        Right = null;
    }
}