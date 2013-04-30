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
        public void GenerationBlinkerPeriod2_ConvertGenerationOfCells_ResultArray()
        {
            var blinkerPeriod2 = ImmutableHashSet.Create(new[]
                {
                    new Cell(x: 1, y: 1),
                    new Cell(x: 2, y: 1),
                    new Cell(x: 3, y: 1)
                });

            int width;
            int height;
            var resultArray = Generation.Convert(blinkerPeriod2, out width, out height);

            width.Should().Be(5);
            height.Should().Be(3);
            resultArray.Should().Equal(
                new[]
                {
                    0, 0, 0, 0, 0,
                    0, 1, 1, 1, 0,
                    0, 0, 0, 0, 0
                });
        }

        [Test]
        public void GenerationBoat_ConvertGenerationOfCells_ResultArray()
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
            var resultArray = Generation.Convert(boat, out width, out height);

            width.Should().Be(6);
            height.Should().Be(6);
            resultArray.Should().Equal(
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
        public void GenerationWithNegativePositions_ConvertGenerationOfCells_ResultArray()
        {
            var blinkerPeriod2 = ImmutableHashSet.Create(new[]
                {
                    new Cell(x: -1, y: 1),
                    new Cell(x: -2, y: 1),
                    new Cell(x: -3, y: 1)
                });

            int width;
            int height;
            var resultArray = Generation.Convert(blinkerPeriod2, out width, out height);

            width.Should().Be(5);
            height.Should().Be(3);
            resultArray.Should().Equal(
                new[]
                {
                    0, 0, 0, 0, 0,
                    0, 1, 1, 1, 0,
                    0, 0, 0, 0, 0
                });
        }

        [Test]
        public void GenerationBlinkerPeriod2_ConvertGenerationOfCells_StringArray()
        {
            var blinkerPeriod2 = ImmutableHashSet.Create(new[]
                {
                    new Cell(x: 1, y: 1),
                    new Cell(x: 2, y: 1),
                    new Cell(x: 3, y: 1)
                });

            var resultArray = Generation.ConvertToString(blinkerPeriod2);

            resultArray.Should().Equal(
                new[]
                {
                    @"     ",
                    @" *** ",
                    @"     "
                });
        }

        [Test]
        public void GenerationBoat_ConvertGenerationOfCells_StringArray()
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

            var resultArray = Generation.ConvertToString(boat);

            resultArray.Should().Equal(
                new[]
                    {
                        @"      ",
                        @"  **  ",
                        @" *  * ",
                        @"  * * ",
                        @"   *  ",
                        @"      "
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

        [Test]
        public void GenerationEmpty_IsEndOfGame_Archived()
        {
            var previousAlmostDeadGenerations =
                ImmutableQueue.Create(
                    Generation.Init(
                        width: 3, height: 3,
                        cells: new[]
                                    {
                                        0, 0, 0,
                                        0, 1, 0,
                                        0, 0, 0
                                    }));

            var currentEmptyGeneration = Generation.Init(
                    width: 3, height: 3,
                    cells: new[]
                                {
                                    0, 0, 0,
                                    0, 0, 0,
                                    0, 0, 0
                                });

            GameStatus status;
            bool isEndOfGame = Generation.IsEndOfGame(currentEmptyGeneration, previousAlmostDeadGenerations, out status);

            isEndOfGame.Should().BeTrue();
            status.Should().Be(GameStatus.GenerationIsEmpty);
        }

        [Test]
        public void GenerationBlock_IsEndOfGame_StillLifesDetected()
        {
            var previousBlockGenerations = 
                ImmutableQueue.Create(
                    Generation.Init(
                        width: 4, height: 4,
                        cells: new[]
                                    {
                                        0, 0, 0, 0,
                                        0, 1, 1, 0,
                                        0, 1, 1, 0,
                                        0, 0, 0, 0
                                    }));

            var currentBlockGeneration = Generation.Init(
                        width: 4, height: 4,
                        cells: new[]
                                    {
                                        0, 0, 0, 0,
                                        0, 1, 1, 0,
                                        0, 1, 1, 0,
                                        0, 0, 0, 0
                                    });

            GameStatus status;
            bool isEndOfGame = Generation.IsEndOfGame(currentBlockGeneration, previousBlockGenerations, out status);

            isEndOfGame.Should().BeTrue();
            status.Should().Be(GameStatus.StillLife);
        }

        [Test]
        public void GenerationBeehive_IsEndOfGame_StillLifesDetected()
        {
            var previousBeehiveGenerations =
                ImmutableQueue.Create(
                    Generation.Init(
                        width: 6, height: 5,
                        cells: new[]
                                    {
                                        0, 0, 0, 0, 0, 0,
                                        0, 0, 1, 1, 0, 0,
                                        0, 1, 0, 0, 1, 0,
                                        0, 0, 1, 1, 0, 0,
                                        0, 0, 0, 0, 0, 0
                                    }));

            var currentBeehiveGeneration = Generation.Init(
                        width: 6, height: 5,
                        cells: new[]
                                    {
                                        0, 0, 0, 0, 0, 0,
                                        0, 0, 1, 1, 0, 0,
                                        0, 1, 0, 0, 1, 0,
                                        0, 0, 1, 1, 0, 0,
                                        0, 0, 0, 0, 0, 0
                                    });

            GameStatus status;
            bool isEndOfGame = Generation.IsEndOfGame(currentBeehiveGeneration, previousBeehiveGenerations, out status);

            isEndOfGame.Should().BeTrue();
            status.Should().Be(GameStatus.StillLife);
        }

        [Test]
        public void GenerationOscillatorBlinkerPeriod2_IsEndOfGame_Continue()
        {
            var previousOscillatorGenerations = 
                ImmutableQueue.Create(
                    Generation.Init(
                        width: 3, height: 3,
                        cells: new[]
                                    {
                                        0, 0, 0,
                                        1, 1, 1,
                                        0, 0, 0
                                    }));

            var currentOscillatorGeneration = Generation.Init(
                        width: 3, height: 3,
                        cells: new[]
                                    {
                                        0, 1, 0,
                                        0, 1, 0,
                                        0, 1, 0
                                    });

            GameStatus status;
            bool isEndOfGame = Generation.IsEndOfGame(currentOscillatorGeneration, previousOscillatorGenerations, out status);

            isEndOfGame.Should().BeFalse();
            status.Should().Be(GameStatus.Continue);
        }

        [Test]
        public void GenerationFirstGeneration_IsEndOfGame_Continue()
        {
            var previousNullGenerations = ImmutableQueue.Create<ImmutableHashSet<Cell>>();

            var currentOscillatorGeneration = 
                    Generation.Init(
                        width: 3, height: 3,
                        cells: new[]
                                    {
                                        0, 1, 0,
                                        0, 1, 0,
                                        0, 1, 0
                                    });

            GameStatus status;
            bool isEndOfGame = Generation.IsEndOfGame(currentOscillatorGeneration, previousNullGenerations, out status);

            isEndOfGame.Should().BeFalse();
            status.Should().Be(GameStatus.Continue);
        }

        [Test]
        public void GenerationFirstGeneration_IsEndOfGame_StringStatusContinue()
        {
            var previousNullGenerations = ImmutableQueue.Create<ImmutableHashSet<Cell>>();

            var currentOscillatorGeneration =
                    Generation.Init(
                        width: 3, height: 3,
                        cells: new[]
                                    {
                                        0, 1, 0,
                                        0, 1, 0,
                                        0, 1, 0
                                    });

            string status;
            bool isEndOfGame = Generation.IsEndOfGame(currentOscillatorGeneration, previousNullGenerations, out status);

            isEndOfGame.Should().BeFalse();
            status.Should().Be("Continue");
        }

        [Test]
        public void GenerationHistory_ManageGenerationQueue_RecentGenerations()
        {
            var generationQueue = ImmutableQueue.Create<ImmutableHashSet<Cell>>();
            var currentGeneration = ImmutableHashSet.Create<Cell>();

            generationQueue = Generation.AddRecentGenerationToHistoryQueue(currentGeneration, generationQueue);

            generationQueue.Should().Contain(currentGeneration).And.HaveCount(1);
        }
    }
}
