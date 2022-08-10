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
using System.Collections.Generic;

namespace Mustache.Tests.Specs.Customs
{
    [TestClass]
    public class SectionsTester
    {
        [TestMethod]
        [TestCategory("SpecsSections")]
        public void NestedContext()
        {
            var dictionary = new Dictionary<string, object>
            {
                {
                    "Data", new Dictionary<string, object>
                    {
                        {"Bool", true},
                        {"Label", "label"},
                        {"Value", "value"}
                    }
                },
                {"user", "user  name"}
            };

            var classObject = new Dictionary<string, object>
            {
                {
                    "Data", new ObjectInfo
                    {
                        Bool = true,
                        Label = "label",
                        Value = "value"
                    }
                },
                {"user", "user  name"}
            };

            dynamic dynamicObject = new
            {
                Data = (dynamic) new
                {
                    Bool = true,
                    Label = "label",
                    Value = "value"
                }
            };

            // 1
            var expected = "'label' 'value'";
            Template compiledTemplate = Template.Compile("{{#Data.Bool}}'{{Data.Label}}' '{{Data.Value}}'{{/Data.Bool}}");

            string templated = compiledTemplate.Render(dictionary);
            Assert.AreEqual(expected, templated, "1a dictionary");

            templated = compiledTemplate.Render(classObject);
            Assert.AreEqual(expected, templated, "1a classObject");

            templated = compiledTemplate.Render(dynamicObject);
            Assert.AreEqual(expected, templated, "1a dynamicObject");

            // 2
            expected = "'' ''";
            compiledTemplate = Template.Compile("{{#Data.Bool}}'{{Label}}' '{{Value}}'{{/Data.Bool}}");

            templated = compiledTemplate.Render(dictionary);
            Assert.AreEqual(expected, templated, "1b dictionary");

            templated = compiledTemplate.Render(classObject);
            Assert.AreEqual(expected, templated, "1b classObject");

            templated = compiledTemplate.Render(dynamicObject);
            Assert.AreEqual(expected, templated, "1b dynamicObject");

            // 3
            expected = "'label' 'value'";
            compiledTemplate = Template.Compile("{{#Data}}{{#Bool}}'{{Data.Label}}' '{{Data.Value}}'{{/Bool}}{{/Data}}");

            templated = compiledTemplate.Render(dictionary);
            Assert.AreEqual(expected, templated, "2a dictionary");

            templated = compiledTemplate.Render(classObject);
            Assert.AreEqual(expected, templated, "2a classObject");

            templated = compiledTemplate.Render(dynamicObject);
            Assert.AreEqual(expected, templated, "2a dynamicObject");

            // 4
            expected = "'label' 'value'";
            compiledTemplate = Template.Compile("{{#Data}}{{#Bool}}'{{Label}}' '{{Value}}'{{/Bool}}{{/Data}}");

            templated = compiledTemplate.Render(dictionary);
            Assert.AreEqual(expected, templated, "2b dictionary");

            templated = compiledTemplate.Render(classObject);
            Assert.AreEqual(expected, templated, "2b classObject");

            templated = compiledTemplate.Render(dynamicObject);
            Assert.AreEqual(expected, templated, "2b dynamicObject");

            // 5
            expected = "'label' 'value'";
            compiledTemplate = Template.Compile("{{#Data}}{{#Value}}'{{Data.Label}}' '{{Data.Value}}'{{/Value}}{{/Data}}");

            templated = compiledTemplate.Render(dictionary);
            Assert.AreEqual(expected, templated, "3a dictionary");

            templated = compiledTemplate.Render(classObject);
            Assert.AreEqual(expected, templated, "3a classObject");

            templated = compiledTemplate.Render(dynamicObject);
            Assert.AreEqual(expected, templated, "3a dynamicObject");

            // 6
            expected = "'label' 'value'";
            compiledTemplate = Template.Compile("{{#Data}}{{#Value}}'{{Label}}' '{{Value}}'{{/Value}}{{/Data}}");

            templated = compiledTemplate.Render(dictionary);
            Assert.AreEqual(expected, templated, "3b dictionary");

            templated = compiledTemplate.Render(classObject);
            Assert.AreEqual(expected, templated, "3b classObject");

            templated = compiledTemplate.Render(dynamicObject);
            Assert.AreEqual(expected, templated, "3b dynamicObject");

            // 7
            expected = "'label' 'value'";
            compiledTemplate = Template.Compile("{{#Data.Value}}'{{Data.Label}}' '{{Data.Value}}'{{/Data.Value}}");

            templated = compiledTemplate.Render(dictionary);
            Assert.AreEqual(expected, templated, "4a dictionary");

            templated = compiledTemplate.Render(classObject);
            Assert.AreEqual(expected, templated, "4a classObject");

            templated = compiledTemplate.Render(dynamicObject);
            Assert.AreEqual(expected, templated, "4a dynamicObject");

            // 8
            expected = "'' ''";
            compiledTemplate = Template.Compile("{{#Data.Value}}'{{Label}}' '{{Value}}'{{/Data.Value}}");

            templated = compiledTemplate.Render(dictionary);
            Assert.AreEqual(expected, templated, "4b dictionary");

            templated = compiledTemplate.Render(classObject);
            Assert.AreEqual(expected, templated, "4b classObject");

            templated = compiledTemplate.Render(dynamicObject);
            Assert.AreEqual(expected, templated, "4b dynamicObject");
        }

        [TestMethod]
        public void NestedContextWithNullTest()
        {
            dynamic data = new
            {
                P1 = (dynamic) null,
                P2 = (dynamic) new {
                    P = "P2.P"
                }
            };

            const string expected = "\nP2.P";
            Template compiledTemplate = Template.Compile("{{#P1.P}}'{{.}}'{{/P1.P}}\n{{#P2.P}}{{.}}{{/P2.P}}");
            string templated = compiledTemplate.Render(data);
            Assert.AreEqual(expected, templated, "4b dynamicObject");
        }
    }

    internal class ObjectInfo
    {
        public bool Bool { get; set; }
        public string Value { get; set; }
        public string Label { get; set; }
    }
}
