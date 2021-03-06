﻿using System.Linq;
using NUnit.Framework;
using Monopoly.Board;

namespace MonopolyUnitTests
{
    [TestFixture]
    class DiceUnitTests
    {
        Dice dice;

        [Test]
        public void Roll_Two_Dice_Six_Sided_Score_Is_GTE_2_LTE_12()
        {
            dice = new Dice();

            foreach (var rollValue in Enumerable.Repeat(0, 100))
            {
                dice.Roll();

                Assert.GreaterOrEqual(dice.Score, 2);
                Assert.LessOrEqual(dice.Score, 12);
            }
        }
    }
}
