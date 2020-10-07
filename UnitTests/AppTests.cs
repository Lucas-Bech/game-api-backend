using GameAPILibrary;
using GameAPILibrary.Resources;
using Moq;
using System;
using System.Collections.Generic;
using Xunit;

namespace UnitTests
{
    public class AppTests
    {

        [Fact]
        public void TestPublishersStr()
        {
            var app = new App();
            app.Publishers = new List<Publisher>() { new Publisher("one"), new Publisher("two") };
            var publishers = string.Join("", app.PublishersStr);
            Assert.True(publishers == "onetwo");
        }

        [Fact]
        public void TestPolymorphism()
        {
            var app = new App();
            app.Name = "Name";
            var iApp = (IApp)app;
            Assert.True(iApp.Name == "Name");
        }
    }
}
