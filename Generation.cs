using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;

namespace ConwaysGameOfLife
{
    public static class Generation
    {
        public static ImmutableHashSet<Cell> Init(int width, int height, IEnumerable<int> cells)
        {
            return ImmutableHashSet.Create(
                Enumerable.Range(0, width * height)
                          .Select(cells.ElementAtOrDefault)
                          .Select((value, index) => new { index, value })
                          .Where(cell => cell.value > default(int))
                          .Select(cell => new Cell(x: cell.index % width, y: cell.index / width)));
        }

        public static ImmutableHashSet<Cell> Next(ImmutableHashSet<Cell> generation)
        {
            return ImmutableHashSet.Create(
                generation
                    .SelectMany(cell =>
                        new[]
                            {
                                new { alive = false, x = cell.X - 1, y = cell.Y - 1, density = 1 },
                                new { alive = false, x = cell.X + 0, y = cell.Y - 1, density = 1 },
                                new { alive = false, x = cell.X + 1, y = cell.Y - 1, density = 1 },

                                new { alive = false, x = cell.X - 1, y = cell.Y + 0, density = 1 },
                                new { alive = true,  x = cell.X + 0, y = cell.Y + 0, density = 0 },
                                new { alive = false, x = cell.X + 1, y = cell.Y + 0, density = 1 },

                                new { alive = false, x = cell.X - 1, y = cell.Y + 1, density = 1 },
                                new { alive = false, x = cell.X + 0, y = cell.Y + 1, density = 1 },
                                new { alive = false, x = cell.X + 1, y = cell.Y + 1, density = 1 }
                            })
                    .GroupBy(position => new { position.x, position.y })
                    .Select(probability =>
                        new
                            {
                                alive = probability.Any(each => each.alive),
                                x = probability.First().x,
                                y = probability.First().y,
                                density = probability.Sum(each => each.density)
                            })
                    .Where(probability =>
                                probability.density == 3
                                || (probability.alive && probability.density == 2))
                    .Select(probability => new Cell(probability.x, probability.y)));
        }

        public static bool IsEndOfGame(ImmutableHashSet<Cell> currentGeneration,
                                        ImmutableQueue<ImmutableHashSet<Cell>> previousGenerations, 
                                        out GameStatus status)
        {
            bool result;

            if (currentGeneration.IsEmpty)
            {
                result = true;
                status = GameStatus.GenerationIsEmpty;
            }
            else if (previousGenerations.Any()
                        && previousGenerations.Last().SetEquals(currentGeneration))
            {
                result = true;
                status = GameStatus.StillLife;
            }
            else if (previousGenerations.Any() 
                        && previousGenerations.Any(currentGeneration.SetEquals))
            {
                result = true;
                status = GameStatus.OscillatorDetected;
            }
            else
            {
                result = false;
                status = GameStatus.Continue;
            }

            return result;
        }

        // INFO: RoslynCTP has limitation related to enum, that is why this method exists
        public static bool IsEndOfGame(ImmutableHashSet<Cell> currentGeneration,
                                        ImmutableQueue<ImmutableHashSet<Cell>> previousGeneration, 
                                        out string description)
        {
            GameStatus status;
            var result = IsEndOfGame(currentGeneration, previousGeneration, out status);

            description = status.ToString();

            return result;
        }

        public static ImmutableQueue<ImmutableHashSet<Cell>> AddRecentGenerationToHistoryQueue(ImmutableHashSet<Cell> currentGeneration, ImmutableQueue<ImmutableHashSet<Cell>> generationQueue)
        {
            while (generationQueue.Count() > 4)
            {
                generationQueue = generationQueue.Dequeue();
            }

            return generationQueue.Enqueue(currentGeneration);
        }

        public static ImmutableList<int> Convert(ImmutableHashSet<Cell> generation, 
                                                    out int width, out int height)
        {
            var dimentions = new
                {
                    startX = generation.Min(cell => cell.X) - 1,
                    startY = generation.Min(cell => cell.Y) - 1,
                    width = generation.Max(cell => cell.X) - generation.Min(cell => cell.X) + 3,
                    height = generation.Max(cell => cell.Y) - generation.Min(cell => cell.Y) + 3
                };

            var result = new int[dimentions.width * dimentions.height];

            generation
                .Select(cell => new
                                {
                                    relativeX = cell.X - dimentions.startX,
                                    relativeY = cell.Y - dimentions.startY 
                                })
                .Select(position => position.relativeX + (position.relativeY * dimentions.width))
                .ToList()
                .ForEach(offset => result[offset] = 1);

            width = dimentions.width;
            height = dimentions.height;

            return ImmutableList.Create(result);
        }

        public static ImmutableList<string> ConvertToString(ImmutableHashSet<Cell> generation)
        {
            int width;
            int height;
            var output = Convert(generation, out width, out height);

            var stringBuilder = new StringBuilder();

            output.ForEach(cell => stringBuilder.Append(cell == 0 ? ' ' : '*'));

            var sequenceOfEmptinessAndStarts = stringBuilder.ToString();

            return ImmutableList.Create(
                        Enumerable.Range(0, height)
                            .Select(index => sequenceOfEmptinessAndStarts.Substring(index * width, width)));
        }
    }

    public enum GameStatus
    {
        Continue,
        GenerationIsEmpty,
        StillLife,
        OscillatorDetected
    }
}