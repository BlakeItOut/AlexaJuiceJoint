using Microsoft.VisualStudio.TestTools.UnitTesting;
using alexaJuiceJoint;
using System.Collections.Generic;
using System.Linq;

namespace alexaJuiceJointTests
{
    [TestClass]
    public class FunctionTests
    {
        static List<Function.SmoothieResource> allResources = Function.GetResources();
        static Function.SmoothieResource resource = allResources.FirstOrDefault();
        [TestMethod]
        public void GetSmoothies_ReturnsStringOfSmoothies_True()
        {
            var actual = Function.GetSmoothies(resource);
            var expected = "Great Gonzo, Maui Waui, Atomic Energy, Sweetart, Tutti Frutti, Jungle Juice, The Boss, and Blue Berry Yum Yum";
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void CombineElements_ReturnStringOfElementsWithAnd_True()
        {
            var target = resource.Smoothies["tutti-fruti"];
            var actual = Function.CombineElements(target.Ingredients);
            var expected = "Strawberry, Banana, Pineapple, and Raspberry";
            Assert.AreEqual(expected, actual);
        }
    }
}
