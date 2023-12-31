

using CamelUpCalculator.Game;

var state = State.FromSimpleSpaces([
    Tuple.Create(1, "Y"),
    Tuple.Create(2, "P"),
    Tuple.Create(3, "G"),
    Tuple.Create(4, "U"),
    Tuple.Create(5, "R"),
    Tuple.Create(6, "W"),
    Tuple.Create(7, "B"),
    Tuple.Create(8, "<"),
    Tuple.Create(9, ">"),
]);

var dice = Die.GetBaseDiceWithout("gp");

Console.WriteLine("Starting scenario:");
state.PrintToConsole();
Console.WriteLine($"Available dice: {string.Join(", ", dice.Select(die => die.id))}");

var results = state.CalculateOutcomes(dice);
Console.WriteLine(results);