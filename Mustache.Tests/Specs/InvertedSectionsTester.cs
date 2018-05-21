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

using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
using System.Collections.Generic;

namespace Mustache.Tests.Specs
{
    [TestClass]
    public class InvertedSectionsTester
    {
        [TestMethod]
        [TestCategory("SpecsInvertedSections")]
        public void Falsey()
        {
            const string template = "\"{{^boolean}}This should be rendered.{{/boolean}}\"";
            const string expected = "\"This should be rendered.\"";

            string templated =
                Template.Compile(template).Render(new Dictionary<string, object> {{"boolean", false}});
            Assert.AreEqual(expected, templated, "Falsey sections should have their contents rendered");
        }

        [TestMethod]
        [TestCategory("SpecsInvertedSections")]
        public void Truthy()
        {
            const string template = "\"{{^boolean}}This should not be rendered.{{/boolean}}\"";
            const string expected = "\"\"";

            string templated =
                Template.Compile(template).Render(new Dictionary<string, object> {{"boolean", true}});
            Assert.AreEqual(expected, templated, "Truthy sections should have their contents omitted");
        }

        [TestMethod]
        [TestCategory("SpecsInvertedSections")]
        public void Context()
        {
            const string template = "\"{{^context}}Hi {{name}}.{{/context}}\"";
            const string expected = "\"\"";

            string templated = Template.Compile(template)
                .Render(new Dictionary<string, object> {{"context", new Dictionary<string, object> {{"name", "Joe"}}}});
            Assert.AreEqual(expected, templated, "Objects and hashes should behave like truthy values");
        }

        [TestMethod]
        [TestCategory("SpecsInvertedSections")]
        public void List()
        {
            const string template = "\"{{^list}}Hi {{n}}.{{/list}}\"";
            const string expected = "\"\"";

            var data = new Dictionary<string, object>
            {
                {
                    "list", new List<Dictionary<string, object>>
                    {

                        new Dictionary<string, object> {{"n", "1"}},
                        new Dictionary<string, object> {{"n", "2"}},
                        new Dictionary<string, object> {{"n", "3"}}
                    }
                }
            };

            string templated = Template.Compile(template).Render(data);
            Assert.AreEqual(expected, templated, "Lists should behave like truthy values");
        }

        [TestMethod]
        [TestCategory("SpecsInvertedSections")]
        public void EmptyList()
        {
            const string template = "\"{{^list}}Yay lists!{{/list}}\"";
            const string expected = "\"Yay lists!\"";

            string templated =
                Template.Compile(template).Render(new Dictionary<string, object> {{"list", new List<object>()}});
            Assert.AreEqual(expected, templated, "Empty lists should behave like falsey values");
        }

        [TestMethod]
        [TestCategory("SpecsInvertedSections")]
        public void Doubled()
        {
            const string template = @"{{^bool}}
* first
{{/bool}}
* {{two}}
{{^bool}}
* third
{{/bool}}
";
            const string expected = @"* first
* second
* third
";

            string templated = Template.Compile(template)
                .Render(new Dictionary<string, object> {{"bool", false}, {"two", "second"}});
            Assert.AreEqual(expected, templated, "Multiple inverted sections per template should be permitted");
        }

        [TestMethod]
        [TestCategory("SpecsInvertedSections")]
        public void NestedFalsey()
        {
            const string template = @"| A {{^bool}}B {{^bool}}C{{/bool}} D{{/bool}} E |";
            const string expected = @"| A B C D E |";

            string templated = Template.Compile(template)
                .Render(new Dictionary<string, object> {{"bool", false}});
            Assert.AreEqual(expected, templated, "Nested falsey sections should have their contents rendered");
        }

        [TestMethod]
        [TestCategory("SpecsInvertedSections")]
        public void NestedTruthy()
        {
            const string template = @"| A {{^bool}}B {{^bool}}C{{/bool}} D{{/bool}} E |";
            const string expected = @"| A  E |";

            string templated = Template.Compile(template)
                .Render(new Dictionary<string, object> {{"bool", true}});
            Assert.AreEqual(expected, templated, "Nested truthy sections should be omitted");
        }

        [TestMethod]
        [TestCategory("SpecsInvertedSections")]
        public void ContextMisses()
        {
            const string template = @"[{{^missing}}Cannot find key 'missing'!{{/missing}}]";
            const string expected = @"[Cannot find key 'missing'!]";

            string templated = Template.Compile(template)
                .Render(new Dictionary<string, object>());
            Assert.AreEqual(expected, templated, "Failed context lookups should be considered falsey");
        }

        [TestMethod]
        [TestCategory("SpecsInvertedSections")]
        public void DottedNamesTruthy()
        {
            const string template = "\"{{^a.b.c}}Not Here{{/a.b.c}}\" == \"\"";
            const string expected = "\"\" == \"\"";

            string templated = Template.Compile(template)
                .Render(new Dictionary<string, object>
                {
                    {
                        "a", new Dictionary<string, object>
                        {
                            {"b", new Dictionary<string, object> {{"c", true}}}
                        }
                    }
                });
            Assert.AreEqual(expected, templated, "Dotted names should be valid for Inverted Section tags");
        }

        [TestMethod]
        [TestCategory("SpecsInvertedSections")]
        public void DottedNamesFalsey()
        {
            const string template = "\"{{^a.b.c}}Not Here{{/a.b.c}}\" == \"Not Here\"";
            const string expected = "\"Not Here\" == \"Not Here\"";

            string templated = Template.Compile(template)
                .Render(new Dictionary<string, object>
                {
                    {
                        "a", new Dictionary<string, object>
                        {
                            {"b", new Dictionary<string, object> {{"c", false}}}
                        }
                    }
                });
            Assert.AreEqual(expected, templated, "Dotted names should be valid for Inverted Section tags");
        }

        [TestMethod]
        [TestCategory("SpecsInvertedSections")]
        public void DottedNamesBrokenChains()
        {
            const string template = "\"{{^a.b.c}}Not Here{{/a.b.c}}\" == \"Not Here\"";
            const string expected = "\"Not Here\" == \"Not Here\"";

            string templated = Template.Compile(template)
                .Render(new Dictionary<string, object>
                {
                    {
                        "a", new Dictionary<string, object>()
                    }
                });
            Assert.AreEqual(expected, templated, "Dotted names that cannot be resolved should be considered falsey");
        }

        [TestMethod]
        [TestCategory("SpecsInvertedSections")]
        public void SurroundingWhitespace()
        {
            const string template = " | {{^boolean}}\t|\t{{/boolean}} | \n";
            const string expected = " | \t|\t | \n";

            string templated = Template.Compile(template)
                .Render(new Dictionary<string, object>
                {
                    {
                        "boolean", false
                    }
                });
            Assert.AreEqual(expected, templated, "Inverted sections should not alter surrounding whitespace");
        }

        [TestMethod]
        [TestCategory("SpecsInvertedSections")]
        public void InternalWhitespace()
        {
            const string template = " | {{^boolean}} {{! Important Whitespace }}\n {{/boolean}} | \n";
            const string expected = " |  \n  | \n";

            string templated = Template.Compile(template)
                .Render(new Dictionary<string, object>
                {
                    {
                        "boolean", false
                    }
                });
            Assert.AreEqual(expected, templated, "Inverted should not alter internal whitespace");
        }

        [TestMethod]
        [TestCategory("SpecsInvertedSections")]
        public void IndentedInlineSections()
        {
            const string template = " {{^boolean}}NO{{/boolean}}\n {{^boolean}}WAY{{/boolean}}\n";
            const string expected = " NO\n WAY\n";

            string templated = Template.Compile(template)
                .Render(new Dictionary<string, object>
                {
                    {
                        "boolean", false
                    }
                });
            Assert.AreEqual(expected, templated, "Single-line sections should not alter surrounding whitespace");
        }

        [TestMethod]
        [TestCategory("SpecsInvertedSections")]
        public void StandaloneLines()
        {
            const string template = @"| This Is
{{^ boolean}}
|
{{/boolean}}
| A Line";
            const string expected = @"| This Is
|
| A Line";

            string templated = Template.Compile(template)
                .Render(new Dictionary<string, object>
                {
                    {
                        "boolean", false
                    }
                });
            Assert.AreEqual(expected, templated, "Standalone lines should be removed from the template");
        }

        [TestMethod]
        [TestCategory("SpecsInvertedSections")]
        public void StandaloneIndentedLines()
        {
            const string template = @"| This Is
   {{^ boolean}}
|
   {{/boolean}}
| A Line";
            const string expected = @"| This Is
|
| A Line";

            string templated = Template.Compile(template)
                .Render(new Dictionary<string, object>
                {
                    {
                        "boolean", false
                    }
                });
            Assert.AreEqual(expected, templated, "Standalone indented lines should be removed from the template");
        }

        [TestMethod]
        [TestCategory("SpecsInvertedSections")]
        public void StandaloneLineEndings()
        {
            const string template = "|\r\n{{^boolean}}\r\n{{/boolean}}\r\n|";
            const string expected = "|\r\n|";

            string templated = Template.Compile(template)
                .Render(new Dictionary<string, object>
                {
                    {
                        "boolean", false
                    }
                });
            Assert.AreEqual(expected, templated, "\"\\r\\n\" should be considered a newline for standalone tags");
        }

        [TestMethod]
        [TestCategory("SpecsInvertedSections")]
        public void StandaloneWithoutPreviousLine()
        {
            const string template = "  {{^boolean}}\n^{{/boolean}}\n/";
            const string expected = "^\n/";

            string templated = Template.Compile(template)
                .Render(new Dictionary<string, object>
                {
                    {
                        "boolean", false
                    }
                });
            Assert.AreEqual(expected, templated, "Standalone tags should not require a newline to precede them");
        }

        [TestMethod]
        [TestCategory("SpecsInvertedSections")]
        public void StandaloneWithoutNewline()
        {
            const string template = "^{{^boolean}}\n/\n  {{/boolean}}";
            const string expected = "^\n/\n";

            string templated = Template.Compile(template)
                .Render(new Dictionary<string, object>
                {
                    {
                        "boolean", false
                    }
                });
            Assert.AreEqual(expected, templated, "Standalone tags should not require a newline to follow them");
        }

        [TestMethod]
        [TestCategory("SpecsInvertedSections")]
        public void Padding()
        {
            const string template = @"|{{^ boolean }}={{/ boolean }}|";
            const string expected = @"|=|";

            string templated = Template.Compile(template)
                .Render(new Dictionary<string, object>
                {
                    {
                        "boolean", false
                    }
                });
            Assert.AreEqual(expected, templated, "Superfluous in-tag whitespace should be ignored");
        }
    }
}