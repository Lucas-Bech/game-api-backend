using GameAPILibrary;
using GameAPILibrary.Resources;
using Moq;
using System;
using System.Collections.Generic;
using Xunit;

namespace UnitTests
{
    public class DLCTests
    {

        [Fact]
        public void TestPublishersStr()
        {
            var dlc = new DLC();
            dlc.Publishers = new List<Publisher>() { new Publisher("one"), new Publisher("two") };
            var publishers = string.Join("", dlc.PublishersStr);
            Assert.True(publishers == "onetwo");
        }

        [Fact]
        public void TestPolymorphism()
        {
            var dlc = new DLC();
            dlc.Name = "Name";
            var iApp = (IApp)dlc;
            Assert.True(iApp.Name == "Name");
        }
    }
}
