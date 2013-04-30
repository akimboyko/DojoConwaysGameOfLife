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

        public static ImmutableList<int> Convert(ImmutableHashSet<Cell> generation, out int width, out int height)
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
                .ToList()
                .ForEach(cell => result[(cell.X - dimentions.startX)
                                        + ((cell.Y - dimentions.startY) * dimentions.width)] = 1);

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

            var s = stringBuilder.ToString();

            return ImmutableList.Create(
                        Enumerable.Range(0, height)
                            .Select(index => s.Substring(index * width, width)));
        }
    }
}