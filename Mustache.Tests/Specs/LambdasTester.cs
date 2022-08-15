// ******************************************************************************
// Copyright (c) 2022 Daniele Scipioni
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
using System;

namespace Mustache.Tests.Specs
{
    [TestClass]
    public class LambdasTester
    {
        [TestMethod]
        [TestCategory("SpecsLamndas")]
        public void InterpolationTest()
        {
            dynamic data = new
            {
                lambda = (Func<string, object>)(rawText => "world")
            };
            const string expected = "Hello, world!";

            string templated = Template.Compile("Hello, {{lambda}}!").Render(data);
            Assert.AreEqual(expected, templated, "A lambda's return value should be interpolated");
        }

        [TestMethod]
        [TestCategory("SpecsLamndas")]
        public void InterpolationExpansionTest()
        {
            dynamic data = new
            {
                planet = "world",
                lambda = (Func<string, object>)(rawText => "{{planet}}")
            };
            const string template = "Hello, {{lambda}}!";
            const string expected = "Hello, world!";

            string templated = Template.Compile(template).Render(data);
            Assert.AreEqual(expected, templated, "A lambda's return value should be parsed");
        }

        [TestMethod]
        [TestCategory("SpecsLamndas")]
        public void InterpolationAlternateDelimitersTest()
        {
            dynamic data = new
            {
                planet = "world",
                lambda = (Func<string, object>)(rawText => "|planet| => {{planet}}")
            };
            const string template = "{{= | | =}}\nHello, (|&lambda|)!";
            const string expected = "Hello, (|planet| => world)!";

            string templated = Template.Compile(template).Render(data);
            Assert.AreEqual(expected, templated, "A lambda's return value should parse with the default delimiters");
        }

        [TestMethod]
        [TestCategory("SpecsLamndas")]
        public void InterpolationMultipleCallsTest()
        {
            var calls = 0;
            dynamic data = new 
            {
                planet = "world",
                lambda = (Func<string, object>)(rawText => ++calls)
            };

            const string template = "{{lambda}} == {{{lambda}}} == {{lambda}}";
            const string expected = "1 == 2 == 3";

            string templated = Template.Compile(template).Render(data);
            Assert.AreEqual(expected, templated, "Interpolated lambdas should not be cached");
        }

        [TestMethod]
        [TestCategory("SpecsLamndas")]
        public void EscapingTest()
        {
            dynamic data = new 
            {
                lambda = (Func<string, object>)(rawText => ">")
            };

            const string template = "<{{lambda}}{{{lambda}}}";
            const string expected = "<&gt;>";

            string templated = Template.Compile(template).Render(data);
            Assert.AreEqual(expected, templated, "Lambda results should be appropriately escaped");
        }

        [TestMethod]
        [TestCategory("SpecsLamndas")]
        public void SectionTest()
        {
            dynamic data = new 
            {
                lambda = (Func<string, object>)(rawText => rawText == "{{x}}" ? "yes" : "no")
            };

            const string template = "<{{#lambda}}{{x}}{{/lambda}}>";
            const string expected = "<yes>";

            string templated = Template.Compile(template).Render(data);
            Assert.AreEqual(expected, templated, "Lambdas used for sections should receive the raw section string");
        }

        [TestMethod]
        [TestCategory("SpecsLamndas")]
        public void SectionExpansionTest()
        {
            dynamic data = new 
            {
                planet = "Earth",
                lambda = (Func<string, object>)(rawText => $"{rawText}{{{{planet}}}}{rawText}")
            };

            const string template = "<{{#lambda}}-{{/lambda}}>";
            const string expected = "<-Earth->";

            string templated = Template.Compile(template).Render(data);
            Assert.AreEqual(expected, templated, "Lambdas used for sections should have their results parsed");
        }
    }
}
