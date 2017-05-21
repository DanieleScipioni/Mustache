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

using System;
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
using Mustache.Elements;

namespace Mustache.Tests
{
    [TestClass]
    public class BuilderTester
    {
        [TestMethod]
        [TestCategory(nameof(Builder))]
        public void ExceptionWithNulls()
        {
            Assert.ThrowsException<ArgumentNullException>(() => Builder.Build(null));
        }

        [TestMethod]
        [TestCategory(nameof(Builder))]
        public void ExceptionWhenStartSectionDoesNotHaveEndSection()
        {
            Assert.ThrowsException<MustacheException>(
                () => Builder.Build(new Element[]
                {
                    new TextElement("some text"),
                    new Section("section"),
                    new TextElement("some more text"),
                    new EndBlock("endsection"),
                    new TextElement("thats all folks")
                }));
        }

        [TestMethod]
        [TestCategory(nameof(Builder))]
        public void ExceptionWithStartSectionAndWithoutEndSection()
        {
            Assert.ThrowsException<MustacheException>(
                () => Builder.Build(new Element[]
                {
                    new TextElement("some text"),
                    new Section("section"),
                    new TextElement("some more text"),
                    new TextElement("thats all folks")
                }));
        }

        [TestMethod]
        [TestCategory(nameof(Builder))]
        public void ExceptionWithoutStartSectionAndWithEndSection()
        {
            Assert.ThrowsException<MustacheException>(
                () => Builder.Build(
                    new Element[]
                    {
                        new TextElement("some text"),
                        new EndBlock("section"),
                        new TextElement("some other text")
                    }));
        }
    }
}
