using System.Collections.Immutable;
using FluentAssertions;
using NUnit.Framework;

namespace ConwaysGameOfLife
{
    [TestFixture]
    public class GameOfLifeTests
    {
        [Test]
        public void GenerationBlinkerPeriod2_InitGenerationOfCells_GenerationOfLivingCells()
        {
            var blinkerPeriod2 = Generation.Init(
                width: 3, height: 3,
                cells: new[]
                            {
                                0, 0, 0,
                                1, 1, 1,
                                0, 0, 0
                            });

            blinkerPeriod2.Should().BeEquivalentTo(
                new[]
                {
                    new Cell(x: 0, y: 1),
                    new Cell(x: 1, y: 1),
                    new Cell(x: 2, y: 1)
                });
        }

        [Test]
        public void GenerationBlinkerBoat_InitGenerationOfCells_GenerationOfLivingCells()
        {
            var boat = Generation.Init(
                width: 6, height: 6,
                cells: new[]
                            {
                                0, 0, 0, 0, 0, 0,
                                0, 0, 1, 1, 0, 0,
                                0, 1, 0, 0, 1, 0,
                                0, 0, 1, 0, 1, 0,
                                0, 0, 0, 1, 0, 0,
                                0, 0, 0, 0, 0, 0
                            });

            boat.Should().BeEquivalentTo(
                new[]
                {
                    new Cell(x: 2, y: 1),
                    new Cell(x: 3, y: 1),
                    new Cell(x: 1, y: 2),
                    new Cell(x: 4, y: 2),
                    new Cell(x: 2, y: 3),
                    new Cell(x: 4, y: 3),
                    new Cell(x: 3, y: 4)
                });
        }

        [Test]
        public void GenerationBlinkerPeriod2_ConvertGenerationOfCells_InputArray()
        {
            var blinkerPeriod2 = ImmutableHashSet.Create(new[]
                {
                    new Cell(x: 1, y: 1),
                    new Cell(x: 2, y: 1),
                    new Cell(x: 3, y: 1)
                });

            int width;
            int height;
            var inputArray = Generation.Convert(blinkerPeriod2, out width, out height);

            width.Should().Be(5);
            height.Should().Be(3);
            inputArray.Should().Equal(
                new[]
                {
                    0, 0, 0, 0, 0,
                    0, 1, 1, 1, 0,
                    0, 0, 0, 0, 0
                });
        }

        [Test]
        public void GenerationBoat_ConvertGenerationOfCells_InputArray()
        {
            var boat = ImmutableHashSet.Create(new[]
                {
                    new Cell(x: 2, y: 1),
                    new Cell(x: 3, y: 1),
                    new Cell(x: 1, y: 2),
                    new Cell(x: 4, y: 2),
                    new Cell(x: 2, y: 3),
                    new Cell(x: 4, y: 3),
                    new Cell(x: 3, y: 4)
                });

            int width;
            int height;
            var inputArray = Generation.Convert(boat, out width, out height);

            width.Should().Be(6);
            height.Should().Be(6);
            inputArray.Should().Equal(
                new[]
                    {
                        0, 0, 0, 0, 0, 0,
                        0, 0, 1, 1, 0, 0,
                        0, 1, 0, 0, 1, 0,
                        0, 0, 1, 0, 1, 0,
                        0, 0, 0, 1, 0, 0,
                        0, 0, 0, 0, 0, 0
                    });
        }

        [Test]
        public void GenerationOscillators_NextGenerationOfCells_BlinkerPeriod2Gen2()
        {
            var blinkerPeriod2Step1 = ImmutableHashSet.Create(new[]
                {
                    new Cell(x: 0, y: 1),
                    new Cell(x: 1, y: 1),
                    new Cell(x: 2, y: 1)
                });

            var blinkerPeriod2Step2 = Generation.Next(blinkerPeriod2Step1);

            blinkerPeriod2Step2.Should().BeEquivalentTo(new[]
                {
                    new Cell(x: 1, y: 0),
                    new Cell(x: 1, y: 1),
                    new Cell(x: 1, y: 2)
                });
        }

        [Test]
        public void GenerationStillLifes_NextGenerationOfCells_Boat()
        {
            var boat = ImmutableHashSet.Create(new[]
                {
                    new Cell(x: 2, y: 1),
                    new Cell(x: 3, y: 1),
                    new Cell(x: 1, y: 2),
                    new Cell(x: 4, y: 2),
                    new Cell(x: 2, y: 3),
                    new Cell(x: 4, y: 3),
                    new Cell(x: 3, y: 4)
                });

            var stillLife = Generation.Next(boat);

            stillLife.Should().BeEquivalentTo(boat);
        }
    }
}
