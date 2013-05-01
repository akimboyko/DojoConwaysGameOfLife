#r "System.Runtime"
#r "ConwaysGameOfLife.dll"

using System;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using ConwaysGameOfLife;

const int width = 150;
const int height = 20;
const int aliveRatio = 4;

var randomGenerator = new Random(DateTime.Now.Millisecond);

ImmutableHashSet<Cell> currentGeneration = 
						Generation.Init(
							width: width, height: height,
							cells: Enumerable
										.Range(0, width * height)
										.Select(n => randomGenerator.Next() % aliveRatio == 0 ? 1 : 0)
										.ToArray());

ImmutableHashSet<Cell> previousGeneration = null;

var generationQueue = ImmutableQueue.Create<ImmutableHashSet<Cell>>();

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

			var endOfGame = 
				Generation.IsEndOfGame(currentGeneration, generationQueue);

			continueProcessing = endOfGame == ConwaysGameOfLife.GameStatus.Continue;

			if (!continueProcessing)
			{
                Console.WriteLine("End of game: {0}", endOfGame);
				break;
			}
		
			generationQueue = Generation.AddRecentGenerationToHistoryQueue(currentGeneration, generationQueue);
			resultArray = Generation.ConvertToString(currentGeneration);
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