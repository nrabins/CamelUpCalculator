namespace CamelUpCalculator.Game
{
    public struct Move(Color camel, int numberOfSpaces)
    {
        public Color camel = camel;
        public int numberOfSpaces = numberOfSpaces;

        public override string ToString()
        {
            return $"{camel.ToDieString()} {numberOfSpaces}";
        }
    }
}
