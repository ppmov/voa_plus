namespace Library
{
    [System.Serializable]
    public enum Player { None, Green, Red }
    public enum Turn { Start, Attack }
    public enum Type { None = ' ', Rock = '#', Paper = '>', Scissors = '<' }
    public enum Mode { Single, Multi }

    [System.Serializable]
    public struct Icon
    {
        public Type type;
        public UnityEngine.Sprite sprite;
    }

    public interface IDotContainer
    {
        public Dot[] Dots { get; }
    }
}
