#r "System.Runtime"
#r "ConwaysGameOfLife.dll"

using System;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using ConwaysGameOfLife;

const int width = 10;
const int height = 10;
const int aliveRatio = 3;

var randomGenerator = new Random(DateTime.Now.Millisecond);

var generation = Generation.Init(
							width: width, height: height,
							cells: Enumerable
										.Range(0, width * height)
										.Select(n => randomGenerator.Next() % aliveRatio != 0 ? 0 : 1)
										.ToArray());

bool continueProcessing = true;

do
{
	while (continueProcessing && !Console.KeyAvailable)
	{
		ImmutableList<string> resultArray;

		try
		{
			generation = Generation.Next(generation);

			if (generation.IsEmpty)
			{
                Console.WriteLine("End of game: generation is empty");
			}
			else
			{
				resultArray = Generation.ConvertToString(generation);
			}

			continueProcessing = !generation.IsEmpty;
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