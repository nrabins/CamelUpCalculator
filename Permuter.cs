using CamelUpCalculator.Game;

namespace CamelUpCalculator
{
    public class Permuter
    {
        /**
         * Given a list of dice, return every possible sequence of moves (along with the count of each move)
         */
        public static IEnumerable<IEnumerable<MoveAndCount>> GetAllSequences(IEnumerable<Die> dice, int trailingDiceToIgnore = 0)
        {
            var sequences = new List<IEnumerable<MoveAndCount>>();

            // For each ordering, get every possible combination of moves (the Cartesian product)
            foreach (IEnumerable<Die> dieOrder in dice.Permute(trailingDiceToIgnore))
            {
                sequences.AddRange(dieOrder.Select(die => die.sides).CartesianProduct());
            }

            return sequences;
        }
    }

    public static class PermuteExtentions
    {
        public static IEnumerable<IEnumerable<T>> Permute<T>(this IEnumerable<T> sequence, int trailingToIgnore = 0)
        {
            if (sequence == null)
            {
                yield break;
            }

            var list = sequence.ToList();

            if (list.Count <= trailingToIgnore)
            {
                yield return Enumerable.Empty<T>();
            }
            else
            {
                var startingElementIndex = 0;

                foreach (var startingElement in list)
                {
                    var index = startingElementIndex;
                    var remainingItems = list.Where((e, i) => i != index);

                    foreach (var permutationOfRemainder in remainingItems.Permute(trailingToIgnore))
                    {
                        yield return permutationOfRemainder.Prepend(startingElement);
                    }

                    startingElementIndex++;
                }
            }
        }

        public static IEnumerable<IEnumerable<T>> CartesianProduct<T>(this IEnumerable<IEnumerable<T>> sequences)
        {
            IEnumerable<IEnumerable<T>> emptyProduct = new[] { Enumerable.Empty<T>() };
            return sequences.Aggregate(
              emptyProduct,
              (accumulator, sequence) =>
                from accseq in accumulator
                from item in sequence
                select accseq.Concat(new[] { item }));
        }
    }

}
