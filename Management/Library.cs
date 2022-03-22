using UnityEngine;

namespace Library
{
    public enum Player { None, Green, Red }
    public enum Turn { Start, Attack }
    public enum Type { None = ' ', Rock = '#', Paper = '>', Scissors = '<' }

    [System.Serializable]
    public struct Icon
    {
        public Type type;
        public Sprite sprite;
    }

    public interface IDotContainer
    {
        public Dot[] Dots { get; }
    }
}
