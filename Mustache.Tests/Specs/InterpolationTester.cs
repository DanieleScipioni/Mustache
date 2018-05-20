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
    public class InterpolationTester
    {
        [TestMethod]
        [TestCategory("SpecsInterpolation")]
        public void NoInterpolation()
        {
            const string template = "Hello from { Mustache }!";
            const string expected = "Hello from { Mustache }!";

            string templated = Template.Compile(template).Render(null);
            Assert.AreEqual(expected, templated, "Mustache-free templates should render as-is");
        }

        [TestMethod]
        [TestCategory("SpecsInterpolation")]
        public void BasicInterpolation()
        {
            const string template = @"Hello, {{subject}}!";

            string templated = Template.Compile(template).Render(new { subject = "world"});
            Assert.AreEqual("Hello, world!", templated, "Unadorned tags should interpolate content into the template");
        }

        [TestMethod]
        [TestCategory("SpecsInterpolation")]
        public void HtmlEscaping()
        {
            const string template = @"These characters should be HTML escaped: {{forbidden}}";
            const string expected = "These characters should be HTML escaped: &amp; &quot; &lt; &gt;";
            string templated = Template.Compile(template).Render(new { forbidden = "& \" < >" });
            Assert.AreEqual(expected, templated, "Basic interpolation should be HTML escaped");
        }

        [TestMethod]
        [TestCategory("SpecsInterpolation")]
        public void TripleMustache()
        {
            const string template = @"These characters should not be HTML escaped: {{{forbidden}}}";
            const string expected = "These characters should not be HTML escaped: & \" < >";
            string templated = Template.Compile(template).Render(new { forbidden = "& \" < >" });
            Assert.AreEqual(expected, templated, "Triple mustaches should interpolate without HTML escaping");
        }

        [TestMethod]
        [TestCategory("SpecsInterpolation")]
        public void Ampersand()
        {
            const string template = "These characters should not be HTML escaped: {{&forbidden}}";
            const string expected = "These characters should not be HTML escaped: & \" < >";
            string templated = Template.Compile(template).Render(new { forbidden = "& \" < >" });
            Assert.AreEqual(expected, templated, "Ampersand should interpolate without HTML escaping");
        }

        [TestMethod]
        [TestCategory("SpecsInterpolation")]
        public void BasicIntegerInterpolation()
        {
            const string template = "\"{{mph}} miles an hour!\"";
            const string expected = "\"85 miles an hour!\"";
            string templated = Template.Compile(template).Render(new { mph = 85 });
            Assert.AreEqual(expected, templated, "Integers should interpolate seamlessly");
        }

        [TestMethod]
        [TestCategory("SpecsInterpolation")]
        public void TripleMustacheIntegerInterpolation()
        {
            const string template = "\"{{{mph}}} miles an hour!\"";
            const string expected = "\"85 miles an hour!\"";
            string templated = Template.Compile(template).Render(new { mph = 85 });
            Assert.AreEqual(expected, templated, "Integers should interpolate seamlessly");
        }

        [TestMethod]
        [TestCategory("SpecsInterpolation")]
        public void AmpersandIntegerInterpolation()
        {
            const string template = "\"{{&mph}} miles an hour!\"";
            const string expected = "\"85 miles an hour!\"";
            string templated = Template.Compile(template).Render(new { mph = 85 });
            Assert.AreEqual(expected, templated, "Integers should interpolate seamlessly");
        }

        [TestMethod]
        [TestCategory("SpecsInterpolation")]
        public void BasicDecimalInterpolation()
        {
            const string template = "\"{{power}} jiggawatts!\"";
            const string expected = "\"1.21 jiggawatts!\"";
            string templated = Template.Compile(template).Render(new { power = 1.210 });
            Assert.AreEqual(expected, templated, "Decimals should interpolate seamlessly with proper significance");
        }

        [TestMethod]
        [TestCategory("SpecsInterpolation")]
        public void TripleMustacheDecimalInterpolation()
        {
            const string template = "\"{{{power}}} jiggawatts!\"";
            const string expected = "\"1.21 jiggawatts!\"";
            string templated = Template.Compile(template).Render(new { power = 1.210 });
            Assert.AreEqual(expected, templated, "Decimals should interpolate seamlessly with proper significance");
        }

        [TestMethod]
        [TestCategory("SpecsInterpolation")]
        public void AmpersandDecimalInterpolation()
        {
            const string template = "\"{{&power}} jiggawatts!\"";
            const string expected = "\"1.21 jiggawatts!\"";
            string templated = Template.Compile(template).Render(new { power = 1.210 });
            Assert.AreEqual(expected, templated, "Decimals should interpolate seamlessly with proper significance");
        }

        [TestMethod]
        [TestCategory("SpecsInterpolation")]
        public void BasicContextMissInterpolation()
        {
            const string template = "I ({{cannot}}) be seen!";
            const string expected = "I () be seen!";
            string templated = Template.Compile(template).Render(new { });
            Assert.AreEqual(expected, templated, "Failed context lookups should default to empty strings");
        }

        [TestMethod]
        [TestCategory("SpecsInterpolation")]
        public void TripleMustacheContextMissInterpolation()
        {
            const string template = "I ({{{cannot}}}) be seen!";
            const string expected = "I () be seen!";
            string templated = Template.Compile(template).Render(new { });
            Assert.AreEqual(expected, templated, "Failed context lookups should default to empty strings");
        }

        [TestMethod]
        [TestCategory("SpecsInterpolation")]
        public void AmpersandContextMissInterpolation()
        {
            const string template = "I ({{&cannot}}) be seen!";
            const string expected = "I () be seen!";
            string templated = Template.Compile(template).Render(new { });
            Assert.AreEqual(expected, templated, "Failed context lookups should default to empty strings");
        }

        [TestMethod]
        [TestCategory("SpecsInterpolation")]
        public void DottedNamesBasicInterpolation()
        {
            const string template = "\"{{person.name}}\" == \"{{#person}}{{name}}{{/person}}\"";
            const string expected = "\"Joe\" == \"Joe\"";
            string templated = Template.Compile(template).Render(new { person = new { name = "Joe" } });
            Assert.AreEqual(expected, templated, "Dotted names should be considered a form of shorthand for sections");
        }

        [TestMethod]
        [TestCategory("SpecsInterpolation")]
        public void DottedNamesTripleMustacheInterpolation()
        {
            const string template = "\"{{{person.name}}}\" == \"{{#person}}{{{name}}}{{/person}}\"";
            const string expected = "\"Joe\" == \"Joe\"";
            string templated = Template.Compile(template).Render(new { person = new { name = "Joe" } });
            Assert.AreEqual(expected, templated, "Dotted names should be considered a form of shorthand for sections");
        }

        [TestMethod]
        [TestCategory("SpecsInterpolation")]
        public void DottedNamesAmpersandInterpolation()
        {
            const string template = "\"{{&person.name}}\" == \"{{#person}}{{&name}}{{/person}}\"";
            const string expected = "\"Joe\" == \"Joe\"";
            string templated = Template.Compile(template).Render(new { person = new { name = "Joe" } });
            Assert.AreEqual(expected, templated, "Dotted names should be considered a form of shorthand for sections");
        }

        [TestMethod]
        [TestCategory("SpecsInterpolation")]
        public void DottedNamesArbitraryDepth()
        {
            const string template = "\"{{a.b.c.d.e.name}}\" == \"Phil\"";
            const string expected = "\"Phil\" == \"Phil\"";
            string templated = Template.Compile(template).Render(new { a = new { b = new { c = new { d = new { e = new { name = "Phil" } } } } } });
            Assert.AreEqual(expected, templated, "Dotted names should be functional to any level of nesting");
        }

        [TestMethod]
        [TestCategory("SpecsInterpolation")]
        public void DottedNamesBrokenChains()
        {
            const string template = "\"{{a.b.c}}\" == \"\"";
            const string expected = "\"\" == \"\"";
            string templated = Template.Compile(template).Render(new { a = new { } });
            Assert.AreEqual(expected, templated, "Any falsey value prior to the last part of the name should yield ''");
        }

        [TestMethod]
        [TestCategory("SpecsInterpolation")]
        public void DottedNamesBrokenChainResolution()
        {
            const string template = "\"{{a.b.c.name}}\" == \"\"";
            const string expected = "\"\" == \"\"";
            string templated = Template.Compile(template).Render(new { c = new { name = "Jim" } });
            Assert.AreEqual(expected, templated, "Each part of a dotted name should resolve only against its parent");
        }

        [TestMethod]
        [TestCategory("SpecsInterpolation")]
        public void DottedNamesInitialResolution()
        {
            const string template = "\"{{#a}}{{b.c.d.e.name}}{{/a}}\" == \"Phil\"";
            const string expected = "\"Phil\" == \"Phil\"";
            var data = new
            {
                a = new { b = new { c = new { d = new { e = new { name = "Phil" } } } } },
                b = new { c = new { d = new { e = new { name = "Wrong" } } } }
            };
            string templated = Template.Compile(template).Render(data);
            Assert.AreEqual(expected, templated, "The first part of a dotted name should resolve as any other name");
        }

        [TestMethod]
        [TestCategory("SpecsInterpolation")]
        public void DottedNamesContextPrecedence()
        {
            const string template = "{{#a}}{{b.c}}{{/a}}";
            const string expected = "";
            var data = new
            {
                a = new { b = new { } },
                b = new { c = "ERROR" }
            };
            string templated = Template.Compile(template).Render(data);
            Assert.AreEqual(expected, templated, "Dotted names should be resolved against former resolutions");
        }

        [TestMethod]
        [TestCategory("SpecsInterpolation")]
        public void InterpolationSurroundingWhitespace()
        {
            const string template = "| {{text}} |";
            const string expected = "| --- |";
            var data = new { text = "---" };
            string templated = Template.Compile(template).Render(data);
            Assert.AreEqual(expected, templated, "Interpolation should not alter surrounding whitespace");
        }

        [TestMethod]
        [TestCategory("SpecsInterpolation")]
        public void TripleMustacheSurroundingWhitespace()
        {
            const string template = "| {{{text}}} |";
            const string expected = "| --- |";
            var data = new { text = "---" };
            string templated = Template.Compile(template).Render(data);
            Assert.AreEqual(expected, templated, "Interpolation should not alter surrounding whitespace");
        }

        [TestMethod]
        [TestCategory("SpecsInterpolation")]
        public void AmpersandSurroundingWhitespace()
        {
            const string template = "| {{&text}} |";
            const string expected = "| --- |";
            var data = new { text = "---" };
            string templated = Template.Compile(template).Render(data);
            Assert.AreEqual(expected, templated, "Interpolation should not alter surrounding whitespace");
        }

        [TestMethod]
        [TestCategory("SpecsInterpolation")]
        public void InterpolationStandalone()
        {
            const string template = "  {{text}}\n";
            const string expected = "  ---\n";
            var data = new { text = "---" };
            string templated = Template.Compile(template).Render(data);
            Assert.AreEqual(expected, templated, "Standalone interpolation should not alter surrounding whitespace");
        }

        [TestMethod]
        [TestCategory("SpecsInterpolation")]
        public void TripleMustacheStandalone()
        {
            const string template = "  {{{text}}}\n";
            const string expected = "  ---\n";
            var data = new { text = "---" };
            string templated = Template.Compile(template).Render(data);
            Assert.AreEqual(expected, templated, "Standalone interpolation should not alter surrounding whitespace");
        }

        [TestMethod]
        [TestCategory("SpecsInterpolation")]
        public void AmpersandStandalone()
        {
            const string template = "  {{&text}}\n";
            const string expected = "  ---\n";
            var data = new { text = "---" };
            string templated = Template.Compile(template).Render(data);
            Assert.AreEqual(expected, templated, "Standalone interpolation should not alter surrounding whitespace");
        }

        [TestMethod]
        [TestCategory("SpecsInterpolation")]
        public void InterpolationWithPadding()
        {
            const string template = "|{{ text }}|";
            const string expected = "|---|";
            var data = new { text = "---" };
            string templated = Template.Compile(template).Render(data);
            Assert.AreEqual(expected, templated, "Superfluous in-tag whitespace should be ignored");
        }

        [TestMethod]
        [TestCategory("SpecsInterpolation")]
        public void TripleMustacheWithPadding()
        {
            const string template = "|{{{ text }}}|";
            const string expected = "|---|";
            var data = new { text = "---" };
            string templated = Template.Compile(template).Render(data);
            Assert.AreEqual(expected, templated, "Superfluous in-tag whitespace should be ignored");
        }

        [TestMethod]
        [TestCategory("SpecsInterpolation")]
        public void AmpersandWithPadding()
        {
            const string template = "|{{& text }}|";
            const string expected = "|---|";
            var data = new { text = "---" };
            string templated = Template.Compile(template).Render(data);
            Assert.AreEqual(expected, templated, "Superfluous in-tag whitespace should be ignored");
        }
    }
}
