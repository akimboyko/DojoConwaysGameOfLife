#r "System.Runtime"
#r "ConwaysGameOfLife.dll"

using System;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using ConwaysGameOfLife;

const int width = 20;
const int height = 20;
const int aliveRatio = 2;

var randomGenerator = new Random(DateTime.Now.Millisecond);

ImmutableHashSet<Cell> currentGeneration = 
						Generation.Init(
							width: width, height: height,
							cells: Enumerable
										.Range(0, width * height)
										.Select(n => randomGenerator.Next() % aliveRatio == 0 ? 1 : 0)
										.ToArray());

ImmutableHashSet<Cell> previousGeneration = null;

bool continueProcessing = true;

do
{
	while (continueProcessing && !Console.KeyAvailable)
	{
		ImmutableList<string> resultArray;

		try
		{
			previousGeneration = currentGeneration;
			currentGeneration = Generation.Next(previousGeneration);

			string status;

			var endOfGame = 
				Generation.IsEndOfGame(currentGeneration, previousGeneration, out status);

			if (endOfGame)
			{
				continueProcessing = false;
                Console.WriteLine("End of game: {0}", status);
			}
			else
			{
				resultArray = Generation.ConvertToString(currentGeneration);
			}
		}
		catch (Exception ex)
		{
			Console.WriteLine("Exception: {0}\nStackTrace: {1}", ex.Message, ex.StackTrace);
			throw;
		}

		if (continueProcessing)
		{
			Console.WriteLine(new string('-', resultArray.First().Length));
			resultArray.ForEach(Console.WriteLine);

			Thread.Sleep(500);
		}
	}
}
while (continueProcessing && Console.ReadKey(true).Key != ConsoleKey.Escape);