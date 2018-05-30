// ******************************************************************************
// Copyright (c) 2018 Daniele Scipioni
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
using System.Collections;
using System.Collections.Generic;

namespace Mustache.Tests
{
    [TestClass]
    public class DataRendererTester
    {
        [TestMethod]
        [TestCategory(nameof(DataRenderer))]
        public void AnonymousObjetcs()
        {
            var data = new
            {
                Name = "Chris",
                Value = 10000,
                TaxedValue = 6000,
                Currency = "dollars",
                InCa = true
            };

            Render(data);
        }

        private class ClassWithFields
        {
#pragma warning disable 414
            public string Name;
            public string Value;
            public string TaxedValue;
            public string Currency;
            public string InCa;
#pragma warning restore 414
        }

        [TestMethod]
        [TestCategory(nameof(DataRenderer))]
        public void ConcreteClassWithFields()
        {
            var data = new ClassWithFields
            {
                Name = "Chris",
                Value = "10000",
                TaxedValue = "6000",
                Currency = "dollars",
                InCa = "InCa"
            };

            Render(data);
        }

        private class ClassWithProperties
        {
            // ReSharper disable once UnusedAutoPropertyAccessor.Local
            public string Name { get; set; }

            // ReSharper disable once UnusedAutoPropertyAccessor.Local
            public string Value { get; set; }

            // ReSharper disable once UnusedAutoPropertyAccessor.Local
            public string TaxedValue { get; set; }

            // ReSharper disable once UnusedAutoPropertyAccessor.Local
            public string Currency { get; set; }

            // ReSharper disable once UnusedAutoPropertyAccessor.Local
            public string InCa { get; set; }
        }

        [TestMethod]
        [TestCategory(nameof(DataRenderer))]
        public void ConcreteClassWithProperties()
        {
            var data = new ClassWithProperties
            {
                Name = "Chris",
                Value = "10000",
                TaxedValue = "6000",
                Currency = "dollars",
                InCa = "InCa"
            };

            Render(data);
        }

        [TestMethod]
        [TestCategory(nameof(DataRenderer))]
        public void GenericDictionary()
        {
            var data = new Dictionary<string, object>
            {
                {"Name", "Chris"},
                {"Value", "10000"},
                {"TaxedValue", 6000},
                {"Currency", "dollars"},
                {"InCa", true}
            };

            Render(data);
        }

        private static void Render(object data)
        {
            const string templateString = @"Hello {{Name}}
You have just won {{Value}} {{Currency}}!
{{#InCa}}
Well, {{TaxedValue}} {{Currency}}, after taxes.
{{/InCa}}";

            string templated = Template.Compile(templateString).Render(data);

            const string expected = @"Hello Chris
You have just won 10000 dollars!
Well, 6000 dollars, after taxes.
";
            Assert.AreEqual(expected, templated);
        }

        [TestMethod]
        [TestCategory(nameof(DataRenderer))]
        public void List()
        {
            const string templateString = @"Collection
{{#Collection}}
 - {{.}}
{{/Collection}}";

            var data = new
            {
                Collection = new List<object>
                {
                    "1",
                    2,
                    3.0
                }
            };

            string templated = Template.Compile(templateString).Render(data);

            const string expected = @"Collection
 - 1
 - 2
 - 3
";
            Assert.AreEqual(expected, templated);
        }

        [TestMethod]
        [TestCategory(nameof(DataRenderer))]
        public void Enumerable()
        {
            const string templateString = @"Collection
{{#Collection}}
 - {{.}}
{{/Collection}}";

            var list = new List<object>
            {
                "1",
                2,
                3.0
            };
            
            var data = new
            {
                Collection = ToEnumerable(list)
            };

            string templated = Template.Compile(templateString).Render(data);

            const string expected = @"Collection
 - 1
 - 2
 - 3
";
            Assert.AreEqual(expected, templated);
        }

        private static IEnumerable ToEnumerable(IEnumerable list)
        {
            foreach (object o in list)
            {
                yield return o;
            }
        }
    }
}
