using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Xunit;
using Amazon.Lambda.Core;
using Amazon.Lambda.TestUtilities;

using alexaJuiceJointDetroit;


namespace alexaJuiceJointDetroit.Tests
{
    public class FunctionTest
    {

        [Fact]
        public void GetSmoothies_ReturnsStringOfSmoothies_True()
        {
            Function.resource = Function.GetResource();
            var actual = Function.GetSmoothies();
            var expected = "the boss, summer blast, pb and j, jungle juice, atomic energy, d's delight, blue nut, juicy fruit, basic bitch, and the rihanna";
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void CombineElements_ReturnStringOfElementsWithAnd_True()
        {
            Function.resource = Function.GetResource();
            var target = Function.resource.Smoothies["the boss"];
            var actual = Function.CombineElements(target.Ingredients);
            var expected = "matcha, mango, avocado, baby greens, banana, almond milk, and agave";
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void GetSmoothies_ReturntStringOfSmoothiesFilteredByIngredient_True()
        {
            Function.resource = Function.GetResource();
            var target = "mango";
            var actual = Function.GetSmoothies(smoothie => smoothie.Ingredients.Contains(target));
            var expected = "the boss, summer blast, jungle juice, atomic energy, d's delight, blue nut, juicy fruit, basic bitch, and the rihanna";
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void OpenNow_TooEarly_True()
        {
            Function.resource = Function.GetResource();
            var target = new DateTime(2018, 11, 12, 14, 0, 0);
            var actual = Function.OpenNow(target);
            var expected = "The juice joint is not open yet. It opens at 10:00 AM.";
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void OpenNow_TooLate_True()
        {
            Function.resource = Function.GetResource();
            var target = new DateTime(2018, 11, 13, 1, 0, 0);
            var actual = Function.OpenNow(target);
            var expected = $"Sorry, it's closed now. {Function.GetHours()}";
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void OpenNow_ClosedDayOfWeek_True()
        {
            Function.resource = Function.GetResource();
            var target = new DateTime(2018, 11, 11, 10, 0, 0);
            var actual = Function.OpenNow(target);
            var expected = $"Sorry, it's not open today. {Function.GetHours()}";
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void OpenNow_Open_True()
        {
            Function.resource = Function.GetResource();
            var target = new DateTime(2018, 11, 12, 17, 0, 0);
            var actual = Function.OpenNow(target);
            var expected = "The juice joint is open now! It closes at 7:00 PM.";
            Assert.Equal(expected, actual);
        }
    }
}
