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
    public class EmbeddedPartialsTester
    {
        [TestMethod]
        [TestCategory("SpecPartials_Embedded")]
        public void BasicBehavior()
        {
            const string template = "\"{{<text}}from partials{{/text}}{{>text}}\"";
            const string expected = "\"from partials\"";

            string templated = Template.Compile(template).Render(null);
            Assert.AreEqual(expected, templated, "The greater-than operator should expand to the named partial");
        }

        [TestMethod]
        [TestCategory("SpecPartials_Embedded")]
        public void FailedLookup()
        {
            const string template = "\"{{>text}}\"";
            const string expected = "\"\"";

            string templated = Template.Compile(template).Render(null);
            Assert.AreEqual(expected, templated, "The empty string should be used when the named partial is not found");
        }

        [TestMethod]
        [TestCategory("SpecPartials_Embedded")]
        public void Context()
        {
            const string template = "\"{{<partial}}*{{text}}*{{/partial}}{{>partial}}\"";
            const string expected = "\"*content*\"";

            var data = new Dictionary<string, object>
            {
                {"text", "content"}
            };

            string templated = Template.Compile(template).Render(data);
            Assert.AreEqual(expected, templated, "The greater-than operator should operate within the current context");
        }

        [TestMethod]
        [TestCategory("SpecPartials_Embedded")]
        public void Recursion()
        {
            const string template = "{{<node}}{{content}}<{{#nodes}}{{>node}}{{/nodes}}>{{/node}}{{>node}}";
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

            string templated = Template.Compile(template).Render(data);
            Assert.AreEqual(expected, templated, "The greater-than operator should properly recurse");
        }

        [TestMethod]
        [TestCategory("SpecPartials_Embedded")]
        public void SurroundingWhitespace()
        {
            const string template = "{{<partial}}\t|\t{{/partial}}| {{>partial}} |";
            const string expected = "| \t|\t |";

            string templated = Template.Compile(template).Render(null);
            Assert.AreEqual(expected, templated, "The greater-than operator should properly recurse");
        }

        [TestMethod]
        [TestCategory("SpecPartials_Embedded")]
        public void InlineIndentation()
        {
            const string template = "{{<partial}}>\n>{{/partial}}  {{data}}  {{> partial}}\n";
            const string expected = "  |  >\n>\n";

            string templated = Template.Compile(template).Render(new {data = "|"});
            Assert.AreEqual(expected, templated, "Whitespace should be left untouched");
        }

        [TestMethod]
        [TestCategory("SpecPartials_Embedded")]
        public void StandaloneLineEndings()
        {
            const string template = "{{<partial}}>{{/partial}}|\r\n{{>partial}}\r\n|";
            const string expected = "|\r\n>|";

            string templated = Template.Compile(template).Render(null);
            Assert.AreEqual(expected, templated, "\"\\r\\n\" should be considered a newline for standalone tags");
        }

        [TestMethod]
        [TestCategory("SpecPartials_Embedded")]
        public void StandaloneWithoutPreviousLine()
        {
            const string template = "{{<partial}}>\n>{{/partial}}  {{>partial}}\n>";
            const string expected = "  >\n  >>";

            string templated = Template.Compile(template).Render(null);
            Assert.AreEqual(expected, templated, "Standalone tags should not require a newline to precede them");
        }

        [TestMethod]
        [TestCategory("SpecPartials_Embedded")]
        public void StandaloneWithoutNewline()
        {
            const string template = "{{<partial}}>\n>{{/partial}}>\n  {{>partial}}";
            const string expected = ">\n  >\n  >";

            string templated = Template.Compile(template).Render(null);
            Assert.AreEqual(expected, templated, "Standalone tags should not require a newline to follow them");
        }

        [TestMethod]
        [TestCategory("SpecPartials_Embedded")]
        public void StandaloneIndentation()
        {
            const string template = @"{{<partial}}
|
{{{content}}}
|
{{/partial}}
\
 {{>partial}}
/";
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

            string templated = Template.Compile(template).Render(data);
            Assert.AreEqual(expected, templated, "Each line of the partial should be indented before rendering");
        }

        [TestMethod]
        [TestCategory("SpecPartials_Embedded")]
        public void PaddingWhitespace()
        {
            const string template = "|{{<partial}}[]{{/partial}}{{> partial }}|";
            const string expected = "|[]|";

            var data = new Dictionary<string, object>
            {
                {"boolean", true}
            };

            string templated = Template.Compile(template).Render(data);
            Assert.AreEqual(expected, templated, "Superfluous in-tag whitespace should be ignored");
        }
    }
}
