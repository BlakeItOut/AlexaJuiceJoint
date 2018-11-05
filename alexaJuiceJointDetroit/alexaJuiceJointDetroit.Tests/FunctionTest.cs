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

        static Function.SmoothieResource resource = Function.GetResource();
        [Fact]
        public void GetSmoothies_ReturnsStringOfSmoothies_True()
        {
            var actual = Function.GetSmoothies(resource);
            var expected = "great gonzo, maui waui, atomic energy, sweetart, tutti frutti, jungle juice, the boss, and blue berry yum yum";
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void CombineElements_ReturnStringOfElementsWithAnd_True()
        {
            var target = resource.Smoothies["tutti-frutti"];
            var actual = Function.CombineElements(target.Ingredients);
            var expected = "strawberry, banana, pineapple, and raspberry";
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void GetSmoothie_ReturnStringOfIngredientsForASmoothie_True()
        {
            var target = "tutti-frutti";
            var actual = Function.GetSmoothie(resource, target);
            var expected = "strawberry, banana, pineapple, and raspberry";
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void GetSmoothies_ReturntStringOfSmoothiesFilteredByIngredient_True()
        {
            var target = "mango";
            var actual = Function.GetSmoothies(resource, (smoothie => smoothie.Ingredients.Contains(target)));
            var expected = "maui waui, atomic energy, jungle juice, and the boss";
            Assert.Equal(expected, actual);
        }
    }
}
