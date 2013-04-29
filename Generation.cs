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
    }
}