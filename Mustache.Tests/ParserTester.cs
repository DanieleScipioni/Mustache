// ******************************************************************************
// Copyright (c) 2017 Daniele Scipioni
// 
// This code is licensed under the MIT License (MIT).
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.
// ******************************************************************************

using Microsoft.VisualStudio.TestTools.UnitTesting;
using Mustache.Elements;
using System.Collections.Generic;
using System.Linq;

namespace Mustache.Tests
{
    [TestClass]
    public class ParserTester
    {

        [TestMethod]
        [TestCategory(nameof(Parser))]
        public void HtmlEscapedRegexTest()
        {
            Assert.IsFalse(Parser.DoNotEscapeHtmlWithAmpersand.Match("aaaa").Success);
            Assert.IsFalse(Parser.DoNotEscapeHtmlWithAmpersand.Match("{aaaa}").Success);
            Assert.IsFalse(Parser.DoNotEscapeHtmlWithAmpersand.Match("{ aaaa }").Success);
            Assert.IsTrue(Parser.DoNotEscapeHtmlWithAmpersand.Match("&aaaa").Success);
            Assert.IsTrue(Parser.DoNotEscapeHtmlWithAmpersand.Match("& aaaa").Success);

            Assert.IsFalse(Parser.DoNotEscapeHtmlWithMustache.Match("aaaa").Success);
            Assert.IsTrue(Parser.DoNotEscapeHtmlWithMustache.Match("{aaaa}").Success);
            Assert.IsTrue(Parser.DoNotEscapeHtmlWithMustache.Match("{ aaaa }").Success);
            Assert.IsFalse(Parser.DoNotEscapeHtmlWithMustache.Match("&aaaa").Success);
            Assert.IsFalse(Parser.DoNotEscapeHtmlWithMustache.Match("& aaaa").Success);
        }

        [TestMethod]
        [TestCategory(nameof(Parser))]
        public void DoNotCreateEmptyPlainTextElementAsFirstElement()
        {
            IEnumerable<Element> enumerable = new Parser("{{ a a}}aaaaa").Parse();
            List<Element> elements = enumerable.ToList();
            Assert.AreEqual(3, elements.Count);

            Assert.IsInstanceOfType(elements[0], typeof(Template));

            Assert.IsInstanceOfType(elements[1], typeof(Variable));

            var variable = (Variable)elements[1];
            Assert.AreEqual("a a", variable.Key);

            Assert.IsInstanceOfType(elements[2], typeof(TextElement));

            var plainText = (TextElement)elements[2];
            Assert.AreEqual("aaaaa", plainText.Text);
        }

        [TestMethod]
        [TestCategory(nameof(Parser))]
        public void DoNotCreateEmptyPlainTextElementAsLastElement()
        {
            IEnumerable<Element> enumerable = new Parser("aaaaa{{ a a}}").Parse();
            List<Element> elements = enumerable.ToList();
            Assert.AreEqual(3, elements.Count);

            Assert.IsInstanceOfType(elements[0], typeof(Template));

            Assert.IsInstanceOfType(elements[1], typeof(TextElement));

            var plainText = (TextElement)elements[1];
            Assert.AreEqual("aaaaa", plainText.Text);

            Assert.IsInstanceOfType(elements[2], typeof(Variable));

            var variable = (Variable)elements[2];
            Assert.AreEqual("a a", variable.Key);
        }
    }
}
