using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection.Emit;
using System.Text;

namespace CamelUpCalculator.Game
{
    public class State(List<State.Space> spaces) : ICloneable
    {
        private readonly List<Space> spaces = spaces;
        private static readonly int COLUMN_WIDTH = 17;


        /*
         * Given a list of tuples, make a state.
         * Tuple.Item1 = index of space
         * Tuple.Item2 = string representation of space occupants from top to bottom
         *               e.g. "RGU" => Red on top of Green on top of Blue
         *               e.g. ">" => Forward bump space
         *               
         * Legend
         * G: Green
         * P: Purple
         * R: Red
         * U: Blue
         * Y: Yellow
         * 
         * B: Black
         * W: White
         * 
         * >: Bump Forward
         * <: Bump Backward
         * 
         */
        public static State FromSimpleSpaces(List<Tuple<int, string>> rawSpaces)
        {
            var stateSpaces = new List<Space>();
            foreach(Tuple<int, string> rawSpace in rawSpaces)
            {
                var (spaceIndex, rawStr) = rawSpace;

                Space stateSpace = Space.ParseSpace(spaceIndex, rawStr.ToLower());
                stateSpaces.Add(stateSpace);
            }

            return new State(stateSpaces);
        }

        public void DoMove(Move move, bool printToConsole = false)
        {
            if (move.camel.IsCrazyCamel())
            {
                // Two rule exceptions for crazy camels:
                // 1. If only one crazy camel is carrying racing camels on its back, you must move that one
                // 2. If one crazy camel is sitting directly on top of another (no racing camels in-between), you must
                //    move the one on top.
                var whiteRiders = GetRiders(Color.WHITE);
                var blackRiders = GetRiders(Color.BLACK);
                var whiteHasRiders = whiteRiders.Any();
                var blackHasRiders = blackRiders.Any();

                // Move the camel that has riders
                if (whiteHasRiders && !blackHasRiders)
                {
                    move.camel = Color.WHITE;
                }
                if (!whiteHasRiders && blackHasRiders)
                {
                    move.camel = Color.BLACK;
                }

                // Move the camel on top of the white/black (or vice versa) pair
                if (whiteRiders.Any() && whiteRiders.First() == Color.BLACK)
                {
                    move.camel = Color.BLACK;
                }
                if (blackRiders.Any() && blackRiders.First() == Color.WHITE)
                {
                    move.camel = Color.WHITE;
                }
            }

            CamelSpace? fromSpace = GetSpace(move.camel);

            Debug.Assert(fromSpace != null, $"No camel found of color {move.camel}");

            var camelIndex = fromSpace.camels.FindIndex(camel => camel == move.camel);
            var camelsToMove = fromSpace.camels.TakeLast(fromSpace.camels.Count() - camelIndex).ToList();
            fromSpace.camels.RemoveRange(camelIndex, camelsToMove.Count());

            if (fromSpace.camels.Count == 0)
            {
                // All the camels have left this space and there's no need to maintain it
                spaces.Remove(fromSpace);
            }

            var destinationIndex = fromSpace.index + (move.camel.IsCrazyCamel() ? -1 : 1) * move.numberOfSpaces;

            var destinationSpace = spaces.Find(space => space.index == destinationIndex);

            if (destinationSpace is BumpSpace bumpSpace)
            {
                // Adjust the destination in the direction of the bump
                destinationIndex += (move.camel.IsCrazyCamel() ? -1 : 1) * bumpSpace.delta;
                destinationSpace = spaces.Find(space => space.index == destinationIndex);
                Debug.Assert(destinationSpace is not BumpSpace, "Two adjacent bump spaces are not allowed");
            }

            if (destinationSpace == null)
            {
                // No space exists at the destination, add it!
                spaces.Add(new CamelSpace(destinationIndex, camelsToMove.ToList()));
            } 
            else if (destinationSpace is CamelSpace camelSpace)
            {
                camelSpace.camels.AddRange(camelsToMove.ToList());
            }
            else
            {
                throw new Exception($"Unknown space type at index {destinationIndex}");
            }

            if (printToConsole)
            {
                PrintToConsole();
            }
        }


        public string CalculateOutcomes(List<Die> dice)
        {
            // This is our main backing data dictionary
            // { (Position, Camel) => Count} }
            Dictionary<Tuple<int, Color>, int> positionCounts = new Dictionary<Tuple<int, Color>, int>();

            // Initialize all possible combinations of position and camel
            var camels = GetCamelOrder();

            for (int position = 0; position < camels.Count(); position++)
            {
                foreach (Color camel in camels)
                {
                    positionCounts[Tuple.Create(position, camel)] = 0;
                }
            }
            var sequences = Permuter.GetAllSequences(dice, trailingDiceToIgnore: 1);
            Console.WriteLine($"Aggregating result for {sequences.Count()} sequences...");

            foreach (IEnumerable<MoveAndCount> sequence in sequences)
            {
                State activeState = new State(spaces.Select(space => (Space)space.Clone()).ToList());
                foreach (MoveAndCount moveAndCount in sequence)
                {
                    activeState.DoMove(moveAndCount.move);
                }
                List<Color> camelOrder = activeState.GetCamelOrder();

                int combinationCount = sequence.Aggregate(1, (product, moveAndCount) => product * moveAndCount.count);

                for (var position = 0; position < camelOrder.Count; position++)
                {
                    var camel = camelOrder[position];
                    var key = Tuple.Create(position, camel);
                    positionCounts[key] += combinationCount; 
                }
            }

            return StringifyOutcomes(positionCounts, camels);
        }


        public List<Color> GetCamelOrder()
        {
            var camels = new List<Color>();
            var sortedSpaces = spaces
                .Where(space => space is CamelSpace)
                .Select(space => (CamelSpace)space)
                .OrderByDescending(space => space.index);
            foreach (var space in sortedSpaces)
            {
                for (int i = space.camels.Count - 1; i >= 0; i--)
                {
                    var camel = space.camels[i];
                    if (!camel.IsCrazyCamel())
                    {
                        camels.Add(camel);
                    }
                }
            }

            return camels;
        }
        private static string StringifyOutcomes(Dictionary<Tuple<int, Color>, int> positionCounts, IEnumerable<Color> camels)
        {

            var positionToCamelToCount = new Dictionary<int, Dictionary<Color, int>>();


            // For output, it will be useful to have each position have a sorted list of color outcomes with counts
            Dictionary<int, List<Tuple<Color, int>>> countsByPosition = [];

            foreach (KeyValuePair<Tuple<int, Color>, int> kvp in positionCounts)
            {
                var position = kvp.Key.Item1;
                var camel = kvp.Key.Item2;
                var count = kvp.Value;

                if (!countsByPosition.ContainsKey(position))
                {
                    countsByPosition[position] = [];
                }
                countsByPosition[position].Add(Tuple.Create(camel, count));
            }

            // Sort position lists
            foreach (KeyValuePair<int, List<Tuple<Color, int>>> kvp in countsByPosition)
            {
                kvp.Value.Sort((a, b) => b.Item2.CompareTo(a.Item2));
            }

            StringBuilder sb = new();
            
            var numberOfPlaces = countsByPosition.Count;
            var placeEnumerable = Enumerable.Range(0, numberOfPlaces);
            var numberOfTrials = countsByPosition.First().Value.Sum(tuple => tuple.Item2);

            sb.AppendLine("Results");
            sb.AppendLine(string.Join("", placeEnumerable.Select(num => (num + 1).ToString().PadRight(COLUMN_WIDTH))));

            // Row by row, this should be square
            foreach (int rowIndex in placeEnumerable)
            {
                foreach (int cellIndex in placeEnumerable)
                {
                    var (camel, count) = countsByPosition[cellIndex][rowIndex];
                    
                    var percentStr = ((double)count/numberOfTrials).ToString("0.0%");
                    sb.Append($"{camel.ToFriendlyString()} ({percentStr})".PadRight(COLUMN_WIDTH));
                }
                sb.AppendLine();
            }

            return sb.ToString();
        }

        public object Clone()
        {
            return new State(new List<Space>(spaces));
        }

        private IEnumerable<Color> GetRiders(Color camel)
        {
            var space = GetSpace(camel);
            Debug.Assert(space != null, $"No space found for {camel}");

            var camelIndex = space.camels.FindIndex(spaceCamel => spaceCamel == camel);
            return space.camels.TakeLast(space.camels.Count - camelIndex - 1);
        }

        private CamelSpace? GetSpace(Color camel)
        {
            return (CamelSpace?)spaces.Find(space =>
            {
                if (space is CamelSpace camelSpace)
                {
                    return camelSpace.camels.Any(spaceCamel => spaceCamel == camel);
                }

                return false;
            });
        }


        public override string? ToString()
        {
            /* 
             * Example:
             * 
             *       R  Y     P
             * G     B  U  <  W
             * 3  4  5  6  7  8 
             */

            var sb = new StringBuilder();

            IterateForPrint(
                handleEmpty: () => sb.Append("   "),
                handleCamel: (camel) => sb.Append(camel.ToChar().PadRight(3)),
                handleBump: (delta) => sb.Append((delta < 0 ? "<" : ">").PadRight(3)),
                handleNewLine: () => sb.Append('\n'),
                handleSpaceNumber: (spaceNum) => sb.Append(spaceNum.ToString().PadRight(3))
            );

            return sb.ToString();

        }

        public void PrintToConsole()
        {
            Console.WriteLine("");

            var defaultForegroundColor = ConsoleColor.White;
            var defaultBackgroundColor = ConsoleColor.Black;
            IterateForPrint(
                handleEmpty: () => Console.Write("   "),
                handleCamel: (camel) =>
                {
                    Console.ForegroundColor = camel.GetForegroundConsoleColor();
                    Console.BackgroundColor = camel.GetBackgroundConsoleColor();
                    Console.Write(camel.ToChar());
                    Console.ForegroundColor = defaultForegroundColor;
                    Console.BackgroundColor = defaultBackgroundColor;
                    Console.Write("  ");
                },
                handleBump: (delta) => Console.Write((delta < 0 ? "<" : ">").PadRight(3)),
                handleNewLine: () => Console.Write('\n'),
                handleSpaceNumber: (spaceNum) => Console.Write(spaceNum.ToString().PadRight(3))
                );
            Console.WriteLine("");
        }

        private void IterateForPrint(
            Action handleEmpty,
            Action<Color> handleCamel,
            Action<int> handleBump,
            Action handleNewLine,
            Action<int> handleSpaceNumber
        )
        {

            var minIndex = spaces.Min(space => space.index);
            var maxIndex = spaces.Max(space => space.index);

            var tallestStack = spaces.Max(space => space.GetHeight());


            for (var iHeight = tallestStack - 1; iHeight >= 0; iHeight--)
            {
                for (var iSpace = minIndex; iSpace <= maxIndex; iSpace++)
                {
                    var space = spaces.Find(space => space.index == iSpace);
                    if (space == null)
                    {
                        handleEmpty();
                        continue;
                    }

                    if (iHeight > space.GetHeight() - 1)
                    {
                        handleEmpty();
                        continue;
                    }

                    if (space is CamelSpace camelSpace)
                    {
                        handleCamel(camelSpace.camels[iHeight]);
                        continue;
                    }

                    if (space is BumpSpace bumpSpace)
                    {
                        handleBump(bumpSpace.delta);
                        continue;
                    }
                }
                handleNewLine();
            }

            for (var iSpace = minIndex; iSpace <= maxIndex; iSpace++)
            {
                handleSpaceNumber(iSpace);
            }
        }


        public abstract class Space : ICloneable
        {
            public int index;

            protected Space(int index)
            {
                this.index = index;
            }

            public abstract object Clone();
            public abstract int GetHeight();

            public static Space ParseSpace(int index, string spaceStr)
            {
                if (spaceStr.StartsWith('<'))
                {
                    return new BumpSpace(index, -1);
                }
                
                if (spaceStr.StartsWith('>'))
                {
                    return new BumpSpace(index, 1);
                }

                // It's a camel space! We reverse to match the more intuitive convention of the first listed camel being on top.
                var camels = spaceStr.Reverse().Select(c =>
                {
                    return ColorExtensions.Parse(c);
                });
                return new CamelSpace(index, camels.ToList());
            }
        }

        public class CamelSpace(int index, List<Color> camels) : Space(index)
        {
            public List<Color> camels = camels;

            public override object Clone()
            {
                return new CamelSpace(index, new List<Color>(camels));
            }

            public override int GetHeight()
            {
                return camels.Count;
            }
        }

        public class BumpSpace(int index, int delta) : Space(index)
        {
            public int delta = delta;

            public override object Clone()
            {
                return new BumpSpace(index, delta);
            }

            public override int GetHeight()
            {
                return 1;
            }
        }

    }
}