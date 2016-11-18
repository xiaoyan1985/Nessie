﻿using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Nessie.Services;
using Nessie.Services.Converters;

namespace Nessie.Tests
{
    [TestClass]
    public class MarkdownTests
    {
        [TestMethod]
        public void Convert_BulletedList_OutputsCorrectHtml()
        {
            var markdown = new MarkdownConverter();
            var result = markdown.Convert("- I'm a list item.\n- I'm another one.");
            Assert.AreEqual(
                "<ul>\r\n<li>I'm a list item.</li>\r\n<li>I'm another one.</li>\r\n</ul>\r\n",
                result);
        }
    }
}