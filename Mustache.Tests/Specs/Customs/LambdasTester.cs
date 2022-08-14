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
using System.Collections.Generic;

namespace Mustache.Tests.Specs.Customs
{
    [TestClass]
    public class LambdasTester
    {
        [TestMethod]
        [TestCategory("CustomLamndas")]
        public void InterpolationTest()
        {
            const string expected = "Hello, world!";
            Template compiledTemplate = Template.Compile("Hello, {{Lambda}}!");

            var dictionary = new Dictionary<string, object>
            {
                {
                    "Lambda", (Func<string, object>) (rawText => "world")
                }
            };

            var classObject = new InterpolationTestLambdaObject();

            dynamic dynamicObject = new
            {
                Lambda = (Func<string, object>) (rawText => "world")
            };

            string templated = compiledTemplate.Render(dictionary);
            Assert.AreEqual(expected, templated, "dictionary");

            templated = compiledTemplate.Render(classObject);
            Assert.AreEqual(expected, templated, "classObject");

            templated = compiledTemplate.Render(dynamicObject);
            Assert.AreEqual(expected, templated, "dynamicObject");
        }

        [TestMethod]
        [TestCategory("CustomLamndas")]
        public void InterpolationExpansionTest()
        {
            const string expected = "Hello, world!";
            Template compiledTemplate = Template.Compile("Hello, {{Lambda}}!");

            var dictionary = new Dictionary<string, object>
            {
                {"Planet", "world"},
                {"Lambda", (Func<string, object>) (rawText => "{{Planet}}")}
            };

            var classObject = new InterpolationExpansionTestLambdaObject();

            dynamic dynamicObject = new
            {
                Planet = "world",
                Lambda = (Func<string, object>)(rawText => "{{Planet}}")
            };

            string templated = compiledTemplate.Render(dictionary);
            Assert.AreEqual(expected, templated, "dictionary");

            templated = compiledTemplate.Render(classObject);
            Assert.AreEqual(expected, templated, "classObject");

            templated = compiledTemplate.Render(dynamicObject);
            Assert.AreEqual(expected, templated, "dynamicObject");
        }

        [TestMethod]
        public void VoidLambdaTest()
        {
            const string expected = "Hello, world!";
            Template compiledTemplate = Template.Compile("Hello, {{Planet}}{{Lambda}}!");

            var dictionary = new Dictionary<string, object>
            {
                {"Planet", "world"},
                {"Lambda", (Action) (() => { })}
            };

            var classObject = new VoidLambdaObject();

            dynamic dynamicObject = new
            {
                Planet = "world",
                Lambda = (Action) (() => { })
            };

            string templated = compiledTemplate.Render(dictionary);
            Assert.AreEqual(expected, templated, "dictionary");

            templated = compiledTemplate.Render(classObject);
            Assert.AreEqual(expected, templated, "classObject");

            templated = compiledTemplate.Render(dynamicObject);
            Assert.AreEqual(expected, templated, "dynamicObject");
        }
    }

    internal class InterpolationTestLambdaObject
    {
        public string Lambda(string rawText)
        {
            return "world";
        }
    }

    internal class InterpolationExpansionTestLambdaObject
    {
        public string Planet = "world";

        public string Lambda(string rawText)
        {
            return "{{Planet}}";
        }
    }

    internal class VoidLambdaObject
    {
        public string Planet = "world";

        public void Lambda(){}
    }
}