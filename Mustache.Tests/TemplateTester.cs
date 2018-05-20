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

namespace Mustache.Tests
{
    [TestClass]
    public class TemplateTester
    {
        [TestMethod]
        [TestCategory(nameof(Template))]
        public void PartialPrecedence()
        {
            var data = new
            {
                Name = "Chris",
                Value = 10000,
                TaxedValue = 6000,
                Currency = "dollars",
                InCa = true
            };

            var partials = new Dictionary<string, string> { { "partial", "Well, {{TaxedValue}} {{Currency}}, after taxes.\r\n" } };

            const string templateString = @"{{<partial}}
Well, {{TaxedValue}} {{Currency}}, before tacses.
{{/partial}}
Hello {{Name}}
You have just won {{Value}} {{Currency}}!
{{#InCa}}
{{>partial}}
{{/InCa}}
";

            string templated = Template.Compile(templateString).Render(data, partials);

            const string expected = @"Hello Chris
You have just won 10000 dollars!
Well, 6000 dollars, after taxes.
";

            Assert.AreEqual(expected, templated, "Partials at render time have precedence on partials in template.");
        }

        [TestMethod]
        [TestCategory(nameof(Template))]
        public void PartiaSidEffect()
        {
            var data = new
            {
                Name = "Chris",
                Value = 10000,
                TaxedValue = 6000,
                Currency = "dollars",
                InCa = true
            };

            var partials = new Dictionary<string, string> { { "partial", "Well, {{TaxedValue}} {{Currency}}, after taxes.\r\n" } };

            const string templateString = @"{{<partial}}
Well, {{TaxedValue}} {{Currency}}, before tacses.
{{/partial}}
Hello {{Name}}
You have just won {{Value}} {{Currency}}!
{{#InCa}}
{{>partial}}
{{/InCa}}
";

            Template template = Template.Compile(templateString);

            // Firt render with external templates. Result ignored.
            // This is done only to test if there are side effects
            // on the template.
            template.Render(data, partials);

            string templated = template.Render(data);

            const string expected = @"Hello Chris
You have just won 10000 dollars!
Well, 6000 dollars, before tacses.
";
            Assert.AreEqual(expected, templated, "Partials at render time must not have side effects on partials inside the template.");
        }
    }
}
