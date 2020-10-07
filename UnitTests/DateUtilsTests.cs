using GameAPILibrary;
using GameAPILibrary.Resources;
using GameAPILibrary.Utilities;
using Moq;
using System;
using System.Collections.Generic;
using Xunit;

namespace UnitTests
{
    public class DateUtilsTest
    {

        [Theory]
        [InlineData("3 Dec, 2019", "03/12/2019")]
        [InlineData("3 Dec 2019", "03/12/2019")]
        [InlineData("9 Sep, 2019", "09/09/2019")]
        [InlineData("9 Sep 2019", "09/09/2019")]
        public void TestPublishersStr(string inputDate, string expectedDate)
        {
            var date = DateUtils.SteamDateToDateTime(inputDate);
            string dateStr = date.ToString("dd/MM/yyyy");
            Assert.True(expectedDate == dateStr);
        }
    }
}
