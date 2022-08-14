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

using Mustache.Elements;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

namespace Mustache
{
    internal class DataRenderer : IElementRenderer
    {
        private static readonly Regex NewLineRegex = new Regex("\n(?!$)");
        private static readonly Regex FirsNewLineRegex = new Regex("^\r?\n");

        private readonly Dictionary<string, PartialDefinition> _partialDefinitions;
        private readonly Dictionary<string, Template> _lambdaTemplates;
        private readonly Stack<object> _parentContexts;
        private readonly StringBuilder _stringBuilder;

        private object _currentContext;
        private string _partialIndent;

        internal DataRenderer(object data, Dictionary<string, PartialDefinition> partialDefinitions, Dictionary<string, Template> lambdaTemplates)
        {
            _currentContext = data;
            _partialDefinitions = partialDefinitions;
            _lambdaTemplates = lambdaTemplates;
            _parentContexts = new Stack<object>();
            _stringBuilder = new StringBuilder();
            _partialIndent = string.Empty;
        }

        internal string Result => _stringBuilder.ToString();

        public void Render(Section section)
        {
            foreach (object item in GetValuesForSection(section.Key))
            {
                _parentContexts.Push(_currentContext);
                _currentContext = item;

                foreach (Element element in section.Elements())
                {
                    element.Accept(this);
                }

                _currentContext = _parentContexts.Pop();
            }
        }

        public void Render(InvertedSection invertedSection)
        {
            IEnumerable<object> enumerable = GetValuesForSection(invertedSection.Key);
            if (enumerable.Any()) return;

            foreach (Element element in invertedSection.Elements())
            {
                element.Accept(this);
            }
        }

        public void Render(TextElement textElement)
        {
            string text = NewLineRegex.Replace(textElement.Text, $"\n{_partialIndent}");
            if (!FirsNewLineRegex.Match(textElement.Text).Success)
            {
                text = $"{_partialIndent}{text}";
            }
            _stringBuilder.Append(text);
        }

        public void Render(EndBlock endBlocks)
        {
            throw new NotImplementedException();
        }

        public void Render(Partial partial)
        {
            string oldIndent = _partialIndent;
            _partialIndent = $"{_partialIndent}{partial.Indent}";

            _partialDefinitions.TryGetValue(partial.Key, out PartialDefinition partialDefinition);
            partialDefinition?.Accept(this);

            _partialIndent = oldIndent;
        }

        public void Render(PartialDefinition partialDefinition)
        {
            foreach (Element element in partialDefinition.Elements())
            {
                element.Accept(this);
            }
        }

        public void Render(Variable variable)
        {
            object value = GetValue(variable.Key);
            if (value == null) return;

            string text = variable.EscapeHtml
                ? WebUtility.HtmlEncode(value.ToString())
                : value.ToString();

            _stringBuilder.Append($"{_partialIndent}{text}");
        }

        public void Render(Template template)
        {
            foreach (Element element in template.Elements())
            {
                element.Accept(this);
            }
        }

        private object GetValue(string path)
        {
            if (path == ".") return _currentContext;

            string[] keys = path.Split('.');

            return keys.Length == 0 ? null : GetValue(new ArraySegment<string>(keys));
        }

        private object GetValue(ArraySegment<string> keys)
        {
            // ReSharper disable once PossibleNullReferenceException
            (bool keyFound, object value) = GetValueFromDatacontext(_currentContext, keys.Array[keys.Offset]);

            if (keyFound)
            {
                if (value == null || keys.Count == 1) return value;
                _parentContexts.Push(_currentContext);
                _currentContext = value;
                value = GetValue(new ArraySegment<string>(keys.Array, keys.Offset + 1, keys.Count - 1));
                _currentContext = _parentContexts.Pop();
                return value;
            }

            foreach (object context in _parentContexts)
            {
                (keyFound, value) = GetValueFromDatacontext(context, keys.Array[keys.Offset]);
                if (!keyFound) continue;

                if (value == null || keys.Count == 1) return value;
                _parentContexts.Push(_currentContext);
                _currentContext = value;
                value = GetValue(new ArraySegment<string>(keys.Array, keys.Offset + 1, keys.Count - 1));
                _currentContext = _parentContexts.Pop();
                return value;
            }

            return null;
        }

        private (bool keyFound, object value) GetValueFromDatacontext(object dataContext, string key)
        {
            if (dataContext is IDictionary dictionary)
            {
                if (!dictionary.Contains(key)) return (false, null);

                object value = dictionary[key];
                if (!(value is Delegate)) return (true, value);

                if (!(value is Func<string, object> lambda)) return (true, null);

                object lambdaResult = lambda(null);
                if (!(lambdaResult is string lambdaString)) return (true, lambdaResult);

                value = RenderLambdaResult(lambdaString);
                return (true, value);
            }

            Type type = dataContext.GetType();

            FieldInfo fieldInfo = type.GetField(key);
            if (fieldInfo != null)
            {
                return (true, fieldInfo.GetValue(dataContext));
            }

            PropertyInfo propertyInfo = type.GetProperty(key);
            if (propertyInfo != null)
            {
                object value = propertyInfo.GetValue(dataContext);
                if (!(value is Delegate)) return (true, value);

                if (!(value is Func<string, object> lambda)) return (true, null);

                object lambdaResult = lambda(null);
                if (!(lambdaResult is string lambdaString)) return (true, lambdaResult);

                value = RenderLambdaResult(lambdaString);
                return (true, value);
            }

            MethodInfo methodInfo = type.GetMethod(key, BindingFlags.Public | BindingFlags.Instance);
            if (methodInfo == null) return (false, null);

            ParameterInfo[] parameters = methodInfo.GetParameters();
            if (parameters.Length != 1 || parameters[0].ParameterType != typeof(string)) return (false, null);
            if (methodInfo.ReturnType == typeof(void)) return (false, null);

            {
                object lambdaResult = methodInfo.Invoke(dataContext, new object[] {null});
                if (!(lambdaResult is string lambdaString)) return (true, lambdaResult);

                string value = RenderLambdaResult(lambdaString);
                return (true, value);
            }
        }

        private string RenderLambdaResult(string template)
        {
            if (!_lambdaTemplates.TryGetValue(template, out Template compiledTemplate))
            {
                _lambdaTemplates[template] = compiledTemplate = Template.Compile(template);
            }
            return compiledTemplate.Render(_currentContext);
        }

        private IEnumerable<object> GetValuesForSection(string path)
        {
            object value = GetValue(path);
            if (value == null) yield break;

            if (value is bool boolValue)
            {
                if (boolValue)
                {
                    yield return true;
                }
            }
            else if (value is string || value is IDictionary) // string and IDictionary are IEnumerable, so it needs to check string befoer IEnumerable
            {
                yield return value;
            }
            else if (value is IEnumerable enumerable)
            {
                foreach (object item in enumerable)
                {
                    yield return item;
                }
            }
            else
            {
                yield return value;
            }
        }
    }
}
