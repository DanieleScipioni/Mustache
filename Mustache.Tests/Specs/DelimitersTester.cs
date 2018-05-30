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

namespace Mustache.Tests.Specs
{
    [TestClass]
    public class DelimitersTester
    {
        [TestMethod]
        [TestCategory("SpecsDelimiters")]
        public void PairBehavior()
        {
            string templated = Template.Compile("{{=<% %>=}}(<%text%>)").Render(new { text = "Hey!" });
            Assert.AreEqual("(Hey!)", templated, "The equals sign (used on both sides) should permit delimiter changes");
        }

        [TestMethod]
        [TestCategory("SpecsDelimiters")]
        public void SpecialCharacters()
        {
            string templated = Template.Compile("({{=[ ]=}}[text])").Render(new { text = "It worked!" });
            Assert.AreEqual("(It worked!)", templated, "Characters with special meaning regexen should be valid delimiters");
        }

        [TestMethod]
        [TestCategory("SpecsDelimiters")]
        public void Sections()
        {
            const string template = @"
[
{{#section}}
    {{data}}
    |data|
{{/section}}

{{= | | =}}
|#section|
    {{data}}
    |data|
|/section|
]";
            const string expected = @"
[
    I got interpolated.
    |data|

    {{data}}
    I got interpolated.
]";
            string templated = Template.Compile(template).Render(new { section = true, data = "I got interpolated." });
            Assert.AreEqual(expected, templated, "Delimiters set outside sections should persist");
        }

        [TestMethod]
        [TestCategory("SpecsDelimiters")]
        public void InvertedSections()
        {
            const string template = @"[
{{^section}}
    {{data}}
    |data|
{{/section}}
{{= | | =}}
|^section|
    {{data}}
    |data|
|/section|
]";

            const string expected = @"[
    I got interpolated.
    |data|
    {{data}}
    I got interpolated.
]";

            string templated = Template.Compile(template).Render(new { section = false, data = "I got interpolated." });
            Assert.AreEqual(expected, templated, "Delimiters set outside inverted sections should persist");
        }

        [TestMethod]
        [TestCategory("SpecsDelimiters")]
        public void PartialInheritence()
        {
            const string template = @"{{<include}}.{{value}}.{{/include}}
[ {{>include}} ]
{{= | | =}}
[ |>include| ]";

            const string expected = @"[ .yes. ]
[ .yes. ]";
            string templated = Template.Compile(template).Render(new { value = "yes" });
            Assert.AreEqual(expected, templated, "Delimiters set in a parent template should not affect a partial");
        }

        [TestMethod]
        [TestCategory("SpecsDelimiters")]
        public void PostPartialBehavior()
        {
            const string template = @"{{<include}}.{{value}}. {{= | | =}} .|value|.|/include|
[ {{>include}} ]
[ .{{value}}.  .|value|. ]";
            const string expected = @"[ .yes.  .yes. ]
[ .yes.  .|value|. ]";

            string templated = Template.Compile(template).Render(new { value = "yes" });
            Assert.AreEqual(expected, templated, "Delimiters set in a partial should not affect the parent template");
        }

        [TestMethod]
        [TestCategory("SpecsDelimiters")]
        public void SurroundingWhitespace()
        {
            const string template = "| {{=@ @=}} |";
            const string expected = "|  |";

            string templated = Template.Compile(template).Render(null);
            Assert.AreEqual(expected, templated, "Surrounding whitespace should be left untouched");
        }

        [TestMethod]
        [TestCategory("SpecsDelimiters")]
        public void OutlyingWhitespaceInline()
        {
            const string template = " | {{=@ @=}}\n";
            const string expected = " | \n";

            string templated = Template.Compile(template).Render(null);
            Assert.AreEqual(expected, templated, "Whitespace should be left untouched");
        }

        [TestMethod]
        [TestCategory("SpecsDelimiters")]
        public void StandaloneTag()
        {
            const string template = @"|
Begin.
{{=@ @=}}
End.";
            const string expected = @"|
Begin.
End.";

            string templated = Template.Compile(template).Render(null);
            Assert.AreEqual(expected, templated, "Standalone lines should be removed from the template");
        }

        [TestMethod]
        [TestCategory("SpecsDelimiters")]
        public void IndentedStandaloneTag()
        {
            const string template = @"|
Begin.
    {{=@ @=}}
End.";
            const string expected = @"|
Begin.
End.";

            string templated = Template.Compile(template).Render(null);
            Assert.AreEqual(expected, templated, "Indented standalone lines should be removed from the template");
        }

        [TestMethod]
        [TestCategory("SpecsDelimiters")]
        public void StandaloneLineEndings()
        {
            const string template = "|\r\n{{= @ @ =}}\r\n|";
            const string expected = "|\r\n|";

            string templated = Template.Compile(template).Render(null);
            Assert.AreEqual(expected, templated, "'\\r\\n' should be considered a newline for standalone tags");
        }

        [TestMethod]
        [TestCategory("SpecsDelimiters")]
        public void StandaloneWithoutPreviousLine()
        {
            const string template = "  {{=@ @=}}\n=";
            const string expected = "=";

            string templated = Template.Compile(template).Render(null);
            Assert.AreEqual(expected, templated, "Standalone tags should not require a newline to precede them");
        }

        [TestMethod]
        [TestCategory("SpecsDelimiters")]
        public void StandaloneWithoutNewline()
        {
            const string template = "=\n  {{=@ @=}}";
            const string expected = "=\n";

            string templated = Template.Compile(template).Render(null);
            Assert.AreEqual(expected, templated, "Standalone tags should not require a newline to follow them");
        }

        [TestMethod]
        [TestCategory("SpecsDelimiters")]
        public void PairWithPadding()
        {
            const string template = "|{{= @   @ =}}|";
            const string expected = "||";

            string templated = Template.Compile(template).Render(null);
            Assert.AreEqual(expected, templated, "Superfluous in-tag whitespace should be ignored");
        }
    }
}
