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

namespace Mustache.Tests.Elements
{
    [TestClass]
    public class TemplateTester
    {
        [TestMethod]
        [TestCategory("TemplateElement")]
        public void SeparateCompileAndRender()
        {
            const string templateString = @"Hello {{name}}
You have just won {{value}} {{currency}}!
{{#in_ca}}
Well, {{taxed_value}} {{currency}}, after taxes.
{{/in_ca}}";

            Template template = Template.Compile(templateString);

            var data1 = new
            {
                name = "Chris",
                value = 10000,
                taxed_value = 6000,
                currency = "dollars",
                in_ca = true
            };

            const string expected1 = @"Hello Chris
You have just won 10000 dollars!
Well, 6000 dollars, after taxes.
";

            string templated1 = template.Render(data1);

            Assert.AreEqual(expected1, templated1);

            var data2 = new
            {
                name = "Daniele",
                value = 10000,
                taxed_value = 5000,
                currency = "euros",
                in_ca = true
            };

            const string expected2 = @"Hello Daniele
You have just won 10000 euros!
Well, 5000 euros, after taxes.
";

            string templated2 = template.Render(data2);

            Assert.AreEqual(expected2, templated2);
        }
    }
}