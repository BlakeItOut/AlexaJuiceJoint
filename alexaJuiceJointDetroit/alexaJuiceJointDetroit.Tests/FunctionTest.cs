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
            var expected = "Great Gonzo, Maui Waui, Atomic Energy, Sweetart, Tutti Frutti, Jungle Juice, The Boss, and Blue Berry Yum Yum";
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void CombineElements_ReturnStringOfElementsWithAnd_True()
        {
            var target = resource.Smoothies["tutti-frutti"];
            var actual = Function.CombineElements(target.Ingredients);
            var expected = "Strawberry, Banana, Pineapple, and Raspberry";
            Assert.Equal(expected, actual);
        }
    }
}
