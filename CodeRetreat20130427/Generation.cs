using System;
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
                    .AsParallel()
                    .AsUnordered()
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
                    .Select(probability => new Cell(probability.x, probability.y))
                    .AsSequential());
        }

        private static readonly
            IImmutableList<Func<ImmutableHashSet<Cell>, ImmutableQueue<ImmutableHashSet<Cell>>, GameStatus>>
            EndOfGameRules = ImmutableList.Create(
                new Func<ImmutableHashSet<Cell>, ImmutableQueue<ImmutableHashSet<Cell>>, GameStatus>[]
                    {
                        (currentGeneration, previousGenerations) => 
                            currentGeneration.IsEmpty
                                ? GameStatus.GenerationIsEmpty
                                : GameStatus.Continue,

                        (currentGeneration, previousGenerations) => 
                            previousGenerations.Any()
                            && previousGenerations.Last().SetEquals(currentGeneration)
                                ? GameStatus.StillLife
                                : GameStatus.Continue,

                        (currentGeneration, previousGenerations) => 
                            previousGenerations.Any(currentGeneration.SetEquals)
                            && previousGenerations.Any(currentGeneration.SetEquals)
                                ? GameStatus.OscillatorDetected
                                : GameStatus.Continue
                    });

        public static GameStatus IsEndOfGame(ImmutableHashSet<Cell> currentGeneration,
                                        ImmutableQueue<ImmutableHashSet<Cell>> previousGenerations)
        {
            return
                EndOfGameRules
                    .Select(criteria => criteria(currentGeneration, previousGenerations))
                    .FirstOrDefault(status => status != GameStatus.Continue);
        }

        public static ImmutableQueue<ImmutableHashSet<Cell>> AddRecentGenerationToHistoryQueue(ImmutableHashSet<Cell> currentGeneration, ImmutableQueue<ImmutableHashSet<Cell>> generationQueue)
        {
            var queueItemsList = generationQueue.Take(4).ToList();
            queueItemsList.Add(currentGeneration);
            return ImmutableQueue.Create(queueItemsList.AsEnumerable());
        }

        public static ImmutableList<int> Convert(ImmutableHashSet<Cell> generation, 
                                                    out int width, out int height)
        {
            var minX = generation.Min(cell => cell.X);
            var minY = generation.Min(cell => cell.Y);
            var maxX = generation.Max(cell => cell.X);
            var maxY = generation.Max(cell => cell.Y);

            var dimentions = new
                {
                    startX  = minX - 1,
                    startY  = minY - 1,
                    width   = maxX - minX + 3,
                    height  = maxY - minY + 3
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