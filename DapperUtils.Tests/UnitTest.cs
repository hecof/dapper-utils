using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DapperUtils.Tests
{
    [TestClass]
    public class UnitTest
    {
        [TestMethod]
        public void TestAdd()
        {
            var madrid = new Place("Madrid", 40.4381311, -3.8196197);
            var placeRepository = new PlaceRepository();
            placeRepository.Add(madrid);
        }
    }
}
