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
    public class PartialsTester
    {
        [TestMethod]
        [TestCategory("SpecsPartials")]
        public void BasicBehavior()
        {
            const string template = "\"{{>text}}\"";
            var partials = new Dictionary<string, string> {{"text", "from partials"}};
            const string expected = "\"from partials\"";

            string templated = Template.Compile(template).Render(null, partials);
            Assert.AreEqual(expected, templated, "The greater-than operator should expand to the named partial");
        }

        [TestMethod]
        [TestCategory("SpecsPartials")]
        public void FailedLookup()
        {
            const string template = "\"{{>text}}\"";
            var partials = new Dictionary<string, string>();
            const string expected = "\"\"";

            string templated = Template.Compile(template).Render(null, partials);
            Assert.AreEqual(expected, templated, "The empty string should be used when the named partial is not found");
        }

        [TestMethod]
        [TestCategory("SpecsPartials")]
        public void Context()
        {
            const string template = "\"{{>partial}}\"";
            var partials = new Dictionary<string, string> {{"partial", "*{{text}}*"}};
            const string expected = "\"*content*\"";

            var data = new Dictionary<string, object>
            {
                {"text", "content"}
            };

            string templated = Template.Compile(template).Render(data, partials);
            Assert.AreEqual(expected, templated, "The greater-than operator should operate within the current context");
        }

        [TestMethod]
        [TestCategory("SpecsPartials")]
        public void Recursion()
        {
            const string template = "{{>node}}";
            var partials = new Dictionary<string, string> {{"node", "{{content}}<{{#nodes}}{{>node}}{{/nodes}}>"}};
            const string expected = "X<Y<>>";

            var data = new Dictionary<string, object>
            {
                {"content", "X"},
                {
                    "nodes", new List<object>
                    {
                        new {content = "Y", nodes = new List<string>()}
                    }
                }
            };

            string templated = Template.Compile(template).Render(data, partials);
            Assert.AreEqual(expected, templated, "The greater-than operator should properly recurse");
        }

        [TestMethod]
        [TestCategory("SpecsPartials")]
        public void SurroundingWhitespace()
        {
            const string template = "| {{>partial}} |";
            var partials = new Dictionary<string, string> {{"partial", "\t|\t"}};
            const string expected = "| \t|\t |";

            string templated = Template.Compile(template).Render(null, partials);
            Assert.AreEqual(expected, templated, "The greater-than operator should properly recurse");
        }

        [TestMethod]
        [TestCategory("SpecsPartials")]
        public void InlineIndentation()
        {
            const string template = "  {{data}}  {{> partial}}\n";
            var partials = new Dictionary<string, string> {{"partial", ">\n>"}};
            const string expected = "  |  >\n>\n";

            string templated = Template.Compile(template).Render(new {data = "|"}, partials);
            Assert.AreEqual(expected, templated, "Whitespace should be left untouched");
        }

        [TestMethod]
        [TestCategory("SpecsPartials")]
        public void StandaloneLineEndings()
        {
            const string template = "|\r\n{{>partial}}\r\n|";
            var partials = new Dictionary<string, string> {{"partial", ">"}};
            const string expected = "|\r\n>|";

            string templated = Template.Compile(template).Render(null, partials);
            Assert.AreEqual(expected, templated, "\"\\r\\n\" should be considered a newline for standalone tags");
        }

        [TestMethod]
        [TestCategory("SpecsPartials")]
        public void StandaloneWithoutPreviousLine()
        {
            const string template = "  {{>partial}}\n>";
            var partials = new Dictionary<string, string> { { "partial", ">\n>" } };
            const string expected = "  >\n  >>";

            string templated = Template.Compile(template).Render(null, partials);
            Assert.AreEqual(expected, templated, "Standalone tags should not require a newline to precede them");
        }

        [TestMethod]
        [TestCategory("SpecsPartials")]
        public void StandaloneWithoutNewline()
        {
            const string template = ">\n  {{>partial}}";
            var partials = new Dictionary<string, string> { { "partial", ">\n>" } };
            const string expected = ">\n  >\n  >";

            string templated = Template.Compile(template).Render(null, partials);
            Assert.AreEqual(expected, templated, "Standalone tags should not require a newline to follow them");
        }

        [TestMethod]
        [TestCategory("SpecsPartials")]
        public void StandaloneIndentation()
        {
            const string template = @"\
 {{>partial}}
/";
            var partials = new Dictionary<string, string> { { "partial", @"|
{{{content}}}
|
" } };
            const string expected = @"\
 |
 <
->
 |
/";

            var data = new Dictionary<string, object>
            {
                {"content", "<\r\n->"}
            };

            string templated = Template.Compile(template).Render(data, partials);
            Assert.AreEqual(expected, templated, "Each line of the partial should be indented before rendering");
        }

        [TestMethod]
        [TestCategory("SpecsPartials")]
        public void PaddingWhitespace()
        {
            const string template = "|{{> partial }}|";
            var partials = new Dictionary<string, string> { { "partial", "[]" } };
            const string expected = "|[]|";

            var data = new Dictionary<string, object>
            {
                {"boolean", true}
            };

            string templated = Template.Compile(template).Render(data, partials);
            Assert.AreEqual(expected, templated, "Superfluous in-tag whitespace should be ignored");
        }
    }
}
