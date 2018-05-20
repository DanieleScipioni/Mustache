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

namespace Mustache.Tests.Specs
{
    [TestClass]
    public class CommentsTester
    {
        [TestMethod]
        [TestCategory("SpecsComments")]
        public void InlineComment()
        {
            string templated = Template.Compile("12345{{! Comment Block! }}67890").Render(null);
            Assert.AreEqual("1234567890", templated, "Comment blocks should be removed from the template");
        }

        [TestMethod]
        [TestCategory("SpecsComments")]
        public void MultilineComment()
        {
            string templated = Template.Compile(@"12345{{!
                        This is a
                    multi-line comment...
            }}67890").Render(null);
            Assert.AreEqual("1234567890", templated, "Multiline comments should be permitted");
        }

        [TestMethod]
        [TestCategory("SpecsComments")]
        public void StandaloneComment()
        {
            string templated = Template.Compile(@"Begin.
{{! Comment Block! }}
End.").Render(null);

            Assert.AreEqual(@"Begin.
End.", templated, "All standalone comment lines should be removed");
        }

        [TestMethod]
        [TestCategory("SpecsComments")]
        public void IndentedStandaloneComment()
        {
            string templated = Template.Compile(@"Begin.
    {{! Comment Block! }}
End.").Render(null);

            Assert.AreEqual(@"Begin.
End.", templated, "All standalone comment lines should be removed");
        }

        [TestMethod]
        [TestCategory("SpecsComments")]
        public void StandaloneLineEndingsComment()
        {
            string templated = Template.Compile("\r\n{{! Standalone Comment }}\r\n").Render(null);

            Assert.AreEqual("\r\n", templated, "'\\r\\n' should be considered a newline for standalone tags");
        }

        [TestMethod]
        [TestCategory("SpecsComments")]
        public void StandaloneWithoutPreviousLineComment()
        {
            string templated = Template.Compile("  {{! I'm Still Standalone }}\n!").Render(null);

            Assert.AreEqual("!", templated, @"'\r\n' should be considered a newline for standalone tags");
        }

        [TestMethod]
        [TestCategory("SpecsComments")]
        public void StandaloneWithoutNewlineComment()
        {
            string templated = Template.Compile("!\n  {{! I'm Still Standalone }}").Render(null);
            Assert.AreEqual("!\n", templated, "Standalone tags should not require a newline to follow them");
        }

        [TestMethod]
        [TestCategory("SpecsComments")]
        public void MultilineStandaloneComment()
        {
            const string template = @"Begin.
{{!
Something's going on here...
}}
End.";
            string templated = Template.Compile(template).Render(null);
            Assert.AreEqual(@"Begin.
End.", templated, "All standalone comment lines should be removed");
        }

        [TestMethod]
        [TestCategory("SpecsComments")]
        public void IndentedMultilineStandaloneComment()
        {
            const string template = @"Begin.
  {{!
      Something's going on here...
  }}
End.";
            string templated = Template.Compile(template).Render(null);
            Assert.AreEqual(@"Begin.
End.", templated, "All standalone comment lines should be removed");
        }

        [TestMethod]
        [TestCategory("SpecsComments")]
        public void IndentedInlineComment()
        {
            const string template = "  12 {{! 34 }}\n";
            string templated = Template.Compile(template).Render(null);
            Assert.AreEqual("  12 \n", templated, "Inline comments should not strip whitespace");
        }

        [TestMethod]
        [TestCategory("SpecsComments")]
        public void SurroundingWhitespaceComment()
        {
            const string template = "12345 {{! Comment Block! }} 67890";
            string templated = Template.Compile(template).Render(null);
            Assert.AreEqual("12345  67890", templated, "Comment removal should preserve surrounding whitespace");
        }
    }
}
