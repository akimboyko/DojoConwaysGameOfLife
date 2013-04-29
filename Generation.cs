using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace ConwaysGameOfLife
{
    public static class Generation
    {
        public static ImmutableSortedSet<Cell> Init(int width, int height, IEnumerable<int> cells)
        {
            return ImmutableSortedSet.Create(
                Enumerable.Range(0, width * height)
                          .Select(cells.ElementAtOrDefault)
                          .Select((value, index) => new { index, value })
                          .Where(cell => cell.value > default(int))
                          .Select(cell => new Cell(x: cell.index % width, y: cell.index / width)));
        }

        public static ImmutableList<int> Convert(ImmutableSortedSet<Cell> generation, out int width, out int height)
        {
            var dimentions = new
                {
                    width = generation.Max(cell => cell.X) - generation.Min(cell => cell.X) + 3,
                    height = generation.Max(cell => cell.Y) - generation.Min(cell => cell.Y) + 3
                };

            var result = new int[dimentions.width * dimentions.height];

            generation
                    .ToList()
                    .ForEach(cell => result[cell.X + (cell.Y * dimentions.width)] = 1);

            width = dimentions.width;
            height = dimentions.height;

            return ImmutableList.Create(result);
        }

        public static ImmutableSortedSet<Cell> Next(ImmutableSortedSet<Cell> generation)
        {
            return ImmutableSortedSet.Create(
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
    }
}