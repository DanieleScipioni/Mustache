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

namespace Mustache.Tests.Specs
{
    [TestClass]
    public class SectionsTester
    {
        [TestMethod]
        [TestCategory("SpecsSections")]
        public void Truthy()
        {
            var data = new Dictionary<string, object> {{"boolean", true}};
            const string template = "\"{{#boolean}}This should be rendered.{{/boolean}}\"";
            const string expected = "\"This should be rendered.\"";

            string templated = Template.Compile(template).Render(data);
            Assert.AreEqual(expected, templated, "Truthy sections should have their contents rendered");
        }

        [TestMethod]
        [TestCategory("SpecsSections")]
        public void Falsey()
        {
            var data = new Dictionary<string, object> { { "boolean", false } };
            const string template = "\"{{#boolean}}This should not be rendered.{{/boolean}}\"";
            const string expected = "\"\"";

            string templated = Template.Compile(template).Render(data);
            Assert.AreEqual(expected, templated, "Falsey sections should have their contents omitted");
        }

        [TestMethod]
        [TestCategory("SpecsSections")]
        public void Context()
        {
            var data = new Dictionary<string, object> {{"context", new Dictionary<string, object> {{"name", "Joe"}}}};
            const string template = "\"{{#context}}Hi {{name}}.{{/context}}\"";
            const string expected = "\"Hi Joe.\"";

            string templated = Template.Compile(template).Render(data);
            Assert.AreEqual(expected, templated, "Objects and hashes should be pushed onto the context stack");
        }

        [TestMethod]
        [TestCategory("SpecsSections")]
        public void DeeplyNestedContexts()
        {
            var data = new Dictionary<string, object>
            {
                { "a", new Dictionary<string, object> { { "one", 1 } } },
                { "b", new Dictionary<string, object> { { "two", 2 } } },
                { "c", new Dictionary<string, object> { { "three", 3 } } },
                { "d", new Dictionary<string, object> { { "four", 4 } } },
                { "e", new Dictionary<string, object> { { "five", 5 } } }
            };

            const string template = @"{{#a}}
{{one}}
{{#b}}
{{one}}{{two}}{{one}}
{{#c}}
{{one}}{{two}}{{three}}{{two}}{{one}}
{{#d}}
{{one}}{{two}}{{three}}{{four}}{{three}}{{two}}{{one}}
{{#e}}
{{one}}{{two}}{{three}}{{four}}{{five}}{{four}}{{three}}{{two}}{{one}}
{{/e}}
{{one}}{{two}}{{three}}{{four}}{{three}}{{two}}{{one}}
{{/d}}
{{one}}{{two}}{{three}}{{two}}{{one}}
{{/c}}
{{one}}{{two}}{{one}}
{{/b}}
{{one}}
{{/a}}
";
            const string expected = @"1
121
12321
1234321
123454321
1234321
12321
121
1
";

            string templated = Template.Compile(template).Render(data);
            Assert.AreEqual(expected, templated, "All elements on the context stack should be accessible");
        }

        [TestMethod]
        [TestCategory("SpecsSections")]
        public void List()
        {
            var data = new Dictionary<string, object>
            {
                {
                    "list", new List<Dictionary<string, object>>
                    {
                        new Dictionary<string, object> {{"item", 1}},
                        new Dictionary<string, object> {{"item", 2}},
                        new Dictionary<string, object> {{"item", 3}}
                    }
                }
            };

            const string template = "\"{{#list}}{{item}}{{/list}}\"";
            const string expected = "\"123\"";

            string templated = Template.Compile(template).Render(data);
            Assert.AreEqual(expected, templated, "Lists should be iterated; list items should visit the context stack");
        }

        [TestMethod]
        [TestCategory("SpecsSections")]
        public void EmptyList()
        {
            var data = new Dictionary<string, object>
            {
                {
                    "list", new List<Dictionary<string, object>>()
                }
            };

            const string template = "\"{{#list}}Yay lists!{{/list}}\"";
            const string expected = "\"\"";

            string templated = Template.Compile(template).Render(data);
            Assert.AreEqual(expected, templated, "Empty lists should behave like falsey values");
        }

        [TestMethod]
        [TestCategory("SpecsSections")]
        public void Doubled()
        {
            var data = new Dictionary<string, object>
            {
                {"bool", true},
                {"two", "second"}
            };

            const string template = @"{{#bool}}
* first
{{/bool}}
* {{two}}
{{#bool}}
* third
{{/bool}}
";
            const string expected = @"* first
* second
* third
";


            string templated = Template.Compile(template).Render(data);
            Assert.AreEqual(expected, templated, "Multiple sections per template should be permitted");
        }

        [TestMethod]
        [TestCategory("SpecsSections")]
        public void NestedTruthy()
        {
            var data = new Dictionary<string, object>
            {
                {"bool", true}
            };

            const string template = "| A {{#bool}}B {{#bool}}C{{/bool}} D{{/bool}} E |";
            const string expected = "| A B C D E |";

            string templated = Template.Compile(template).Render(data);
            Assert.AreEqual(expected, templated, "Nested truthy sections should have their contents rendered");
        }

        [TestMethod]
        [TestCategory("SpecsSections")]
        public void NestedFalsey()
        {
            var data = new Dictionary<string, object>
            {
                {"bool", false}
            };

            const string template = "| A {{#bool}}B {{#bool}}C{{/bool}} D{{/bool}} E |";
            const string expected = "| A  E |";

            string templated = Template.Compile(template).Render(data);
            Assert.AreEqual(expected, templated, "Nested falsey sections should be omitted");
        }

        [TestMethod]
        [TestCategory("SpecsSections")]
        public void ContextMisses()
        {
            var data = new Dictionary<string, object>();

            const string template = "[{{#missing}}Found key 'missing'!{{/missing}}]";
            const string expected = "[]";

            string templated = Template.Compile(template).Render(data);
            Assert.AreEqual(expected, templated, "Nested falsey sections should be omitted");
        }

        [TestMethod]
        [TestCategory("SpecsSections")]
        public void ImplicitIteratorString()
        {
            var data = new { list = new List<string> { "a", "b", "c", "d", "e" } };

            const string template = "\"{{#list}}({{.}}){{/list}}\"";
            const string expected = "\"(a)(b)(c)(d)(e)\"";

            string templated = Template.Compile(template).Render(data);
            Assert.AreEqual(expected, templated, "Implicit iterators should directly interpolate strings");
        }

        [TestMethod]
        [TestCategory("SpecsSections")]
        public void ImplicitIteratorInteger()
        {
            var data = new { list = new List<int> { 1, 2, 3, 4, 5 } };

            const string template = "\"{{#list}}({{.}}){{/list}}\"";
            const string expected = "\"(1)(2)(3)(4)(5)\"";

            string templated = Template.Compile(template).Render(data);
            Assert.AreEqual(expected, templated, "Implicit iterators should directly interpolate strings");
        }

        [TestMethod]
        [TestCategory("SpecsSections")]
        public void ImplicitIteratorDecimal()
        {
            var data = new { list = new List<decimal> { 1.1m, 2.20m, 3.300m, 4.4000m, 5.50000m } };

            const string template = "\"{{#list}}({{.}}){{/list}}\"";
            const string expected = "\"(1.1)(2.20)(3.300)(4.4000)(5.50000)\"";

            string templated = Template.Compile(template).Render(data);
            Assert.AreEqual(expected, templated, "Implicit iterators should cast decimals to strings and interpolate");
        }

        [TestMethod]
        [TestCategory("SpecsSections")]
        public void ImplicitIteratorArray()
        {
            var data = new { list = new List<object> { new List<int> { 1, 2, 3 }, new List<char> { 'a', 'b', 'c' } } };

            const string template = "\"{{#list}}({{#.}}{{.}}{{/.}}){{/list}}\"";
            const string expected = "\"(123)(abc)\"";

            string templated = Template.Compile(template).Render(data);
            Assert.AreEqual(expected, templated, "Implicit iterators should allow iterating over nested arrays");
        }

        [TestMethod]
        [TestCategory("SpecsSections")]
        public void DottedNamesTruthy()
        {
            var data = new { a = new { b = new { c = true } } };

            const string template = "\"{{#a.b.c}}Here{{/a.b.c}}\" == \"Here\"";
            const string expected = "\"Here\" == \"Here\"";

            string templated = Template.Compile(template).Render(data);
            Assert.AreEqual(expected, templated, "Dotted names should be valid for Section tags");
        }

        [TestMethod]
        [TestCategory("SpecsSections")]
        public void DottedNamesFalsey()
        {
            var data = new { a = new { b = new { c = false } } };

            const string template = "\"{{#a.b.c}}Here{{/a.b.c}}\" == \"\"";
            const string expected = "\"\" == \"\"";

            string templated = Template.Compile(template).Render(data);
            Assert.AreEqual(expected, templated, "Dotted names should be valid for Section tags");
        }

        [TestMethod]
        [TestCategory("SpecsSections")]
        public void DottedNamesBrokenChains()
        {
            var data = new { a = new { } };

            const string template = "\"{{#a.b.c}}Here{{/a.b.c}}\" == \"\"";
            const string expected = "\"\" == \"\"";

            string templated = Template.Compile(template).Render(data);
            Assert.AreEqual(expected, templated, "Dotted names that cannot be resolved should be considered falsey");
        }

        [TestMethod]
        [TestCategory("SpecsSections")]
        public void SurroundingWhitespace()
        {
            var data = new { boolean = true };

            const string template = " | {{#boolean}}\t|\t{{/boolean}} | \n";
            const string expected = " | \t|\t | \n";

            string templated = Template.Compile(template).Render(data);
            Assert.AreEqual(expected, templated, "Sections should not alter surrounding whitespace");
        }

        [TestMethod]
        [TestCategory("SpecsSections")]
        public void InternalWhitespace()
        {
            var data = new { boolean = true };

            const string template = " | {{#boolean}} {{! Important Whitespace }}\n {{/boolean}} | \n";
            const string expected = " |  \n  | \n";

            string templated = Template.Compile(template).Render(data);
            Assert.AreEqual(expected, templated, "Sections should not alter internal whitespace");
        }

        [TestMethod]
        [TestCategory("SpecsSections")]
        public void IndentedInlineSections()
        {
            var data = new { boolean = true };

            const string template = " {{#boolean}}YES{{/boolean}}\n {{#boolean}}GOOD{{/boolean}}\n";
            const string expected = " YES\n GOOD\n";

            string templated = Template.Compile(template).Render(data);
            Assert.AreEqual(expected, templated, "Single-line sections should not alter surrounding whitespace");
        }

        [TestMethod]
        [TestCategory("SpecsSections")]
        public void StandaloneLines()
        {
            var data = new { boolean = true };

            const string template = @"| This Is
{{#boolean}}
|
{{/boolean}}
| A Line";
            const string expected = @"| This Is
|
| A Line";

            string templated = Template.Compile(template).Render(data);
            Assert.AreEqual(expected, templated, "Standalone lines should be removed from the template");
        }

        [TestMethod]
        [TestCategory("SpecsSections")]
        public void IndentedStandaloneLines()
        {
            var data = new { boolean = true };

            const string template = @"| This Is
  {{#boolean}}
|
  {{/boolean}}
| A Line";
            const string expected = @"| This Is
|
| A Line";

            string templated = Template.Compile(template).Render(data);
            Assert.AreEqual(expected, templated, "Indented standalone lines should be removed from the template");
        }

        [TestMethod]
        [TestCategory("SpecsSections")]
        public void StandaloneLineEndings()
        {
            var data = new { boolean = true };

            const string template = "|\r\n{{#boolean}}\r\n{{/boolean}}\r\n|";
            const string expected = "|\r\n|";

            string templated = Template.Compile(template).Render(data);
            Assert.AreEqual(expected, templated, "\"\\r\\n\" should be considered a newline for standalone tags");
        }

        [TestMethod]
        [TestCategory("SpecsSections")]
        public void StandaloneWithoutPreviousLine()
        {
            var data = new { boolean = true };

            const string template = "  {{#boolean}}\n#{{/boolean}}\n/";
            const string expected = "#\n/";

            string templated = Template.Compile(template).Render(data);
            Assert.AreEqual(expected, templated, "Standalone tags should not require a newline to precede them");
        }

        [TestMethod]
        [TestCategory("SpecsSections")]
        public void StandaloneWithoutNewline()
        {
            var data = new { boolean = true };

            const string template = "#{{#boolean}}\n/\n  {{/boolean}}";
            const string expected = "#\n/\n";

            string templated = Template.Compile(template).Render(data);
            Assert.AreEqual(expected, templated, "Standalone tags should not require a newline to follow them");
        }

        [TestMethod]
        [TestCategory("SpecsSections")]
        public void Padding()
        {
            var data = new { boolean = true };

            const string template = "|{{# boolean }}={{/ boolean }}|";
            const string expected = "|=|";

            string templated = Template.Compile(template).Render(data);
            Assert.AreEqual(expected, templated, "Superfluous in-tag whitespace should be ignored");
        }
    }
}

