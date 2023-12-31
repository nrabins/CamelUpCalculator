
namespace CamelUpCalculator.Game
{
    public struct Die(string id, List<MoveAndCount> sides)
    {
        public List<MoveAndCount> sides = sides;
        public string id = id;

        public override readonly string ToString()
        {
            return sides[0].move.ToString();
        }

        public static List<Die> GetBaseDice()
        {
            return [
                new Die(
                    "r",
                [
                    new(new Move(Color.RED, 1), 2),
                    new(new Move(Color.RED, 2), 2),
                    new(new Move(Color.RED, 3), 2),
                ]),
                new Die(
                    "g",
                [
                    new(new Move(Color.GREEN, 1), 2),
                    new(new Move(Color.GREEN, 2), 2),
                    new(new Move(Color.GREEN, 3), 2),
                ]),
                new Die(
                    "u",
                [
                    new(new Move(Color.BLUE, 1), 2),
                    new(new Move(Color.BLUE, 2), 2),
                    new(new Move(Color.BLUE, 3), 2),
                ]),
                new Die(
                    "y",
                [
                    new(new Move(Color.YELLOW, 1), 2),
                    new(new Move(Color.YELLOW, 2), 2),
                    new(new Move(Color.YELLOW, 3), 2),
                ]),
                new Die(
                    "p",
                [
                    new(new Move(Color.PURPLE, 1), 2),
                    new(new Move(Color.PURPLE, 2), 2),
                    new(new Move(Color.PURPLE, 3), 2),
                ]),
                new Die(
                    "c",
                [
                    new(new Move(Color.BLACK, 1), 1),
                    new(new Move(Color.BLACK, 2), 1),
                    new(new Move(Color.BLACK, 3), 1),
                    new(new Move(Color.WHITE, 1), 1),
                    new(new Move(Color.WHITE, 2), 1),
                    new(new Move(Color.WHITE, 3), 1),
                ]),
            ];
        }

        public static List<Die> GetBaseDiceWithout(string usedDiceIds)
        {
            return GetBaseDice().Where(die => !usedDiceIds.Contains(die.id, StringComparison.CurrentCultureIgnoreCase)).ToList();
        }
        public static List<Die> GetBaseDiceWithOnly(string availableDiceIds)
        {
            return GetBaseDice().Where(die => availableDiceIds.Contains(die.id, StringComparison.CurrentCultureIgnoreCase)).ToList();
        }
    }

    public struct MoveAndCount(Move move, int count)
    {
        public Move move = move;
        public int count = count;
    }
}
