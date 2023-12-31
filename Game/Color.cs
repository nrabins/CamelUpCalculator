
namespace CamelUpCalculator.Game
{
    public enum Color
    {
        RED,
        YELLOW,
        BLUE,
        GREEN,
        PURPLE,
        WHITE,
        BLACK
    }

    public static class ColorExtensions
    {

        public static Color Parse(char c)
        {
            return c switch
            {
                'r' => Color.RED,
                'y' => Color.YELLOW,
                'u' => Color.BLUE,
                'g' => Color.GREEN,
                'p' => Color.PURPLE,
                'w' => Color.WHITE,
                'b' => Color.BLACK,
                _ => throw new Exception($"Failed to parse Color. Unrecognized character: {c}")
            };
        }

        public static string ToChar(this Color me)
        {
            return me switch
            {
                Color.RED => "R",
                Color.YELLOW => "Y",
                Color.BLUE => "U",
                Color.GREEN => "G",
                Color.PURPLE => "P",
                Color.WHITE => "W",
                Color.BLACK => "B",
                _ => "?",
            };
        }

        public static string ToDieString(this Color me)
        {
            return me switch
            {
                Color.RED => "Red",
                Color.YELLOW => "Yellow",
                Color.BLUE => "Blue",
                Color.GREEN => "Green",
                Color.PURPLE => "Purple",
                Color.WHITE => "Crazy",
                Color.BLACK => "Crazy",
                _ => throw new NotImplementedException(),
            };
        }

        public static string ToFriendlyString(this Color me)
        {
            return me switch
            {
                Color.RED => "Red",
                Color.YELLOW => "Yellow",
                Color.BLUE => "Blue",
                Color.GREEN => "Green",
                Color.PURPLE => "Purple",
                Color.WHITE => "White",
                Color.BLACK => "Black",
                _ => throw new NotImplementedException(),
            };
        }

        public static ConsoleColor GetForegroundConsoleColor(this Color me)
        {
            return me switch
            {

                Color.RED => ConsoleColor.Black,
                Color.YELLOW => ConsoleColor.Black,
                Color.BLUE => ConsoleColor.Black,
                Color.GREEN => ConsoleColor.Black,
                Color.PURPLE => ConsoleColor.Black,
                Color.WHITE => ConsoleColor.Black,
                Color.BLACK => ConsoleColor.White,
                _ => throw new NotImplementedException(),
            };
        }
        public static ConsoleColor GetBackgroundConsoleColor(this Color me)
        {
            return me switch
            {
                Color.RED => ConsoleColor.Red,
                Color.YELLOW => ConsoleColor.Yellow,
                Color.BLUE => ConsoleColor.Blue,
                Color.GREEN => ConsoleColor.Green,
                Color.PURPLE => ConsoleColor.Magenta,
                Color.WHITE => ConsoleColor.White,
                Color.BLACK => ConsoleColor.DarkGray,
                _ => throw new NotImplementedException(),
            };
        }

        public static bool IsCrazyCamel(this Color me)
        {
            return me == Color.BLACK || me == Color.WHITE;
        }
    }

}
