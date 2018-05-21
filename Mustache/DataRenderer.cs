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
    public class DataRenderer : IElementRenderer
    {
        private static readonly Regex NewLineRegex = new Regex("\n(?!$)");
        private static readonly Regex FirsNewLineRegex = new Regex("^\r?\n");

        private object _currentContext;
        private readonly Dictionary<string, PartialDefinition> _partialDefinitions;
        private readonly Stack<object> _parentContexts;
        private readonly StringBuilder _stringBuilder;
        private string _partialIndent;


        internal DataRenderer(object data, Dictionary<string, PartialDefinition> partialDefinitions)
        {
            _currentContext = data;
            _partialDefinitions = partialDefinitions;
            _parentContexts = new Stack<object>();
            _stringBuilder = new StringBuilder();
            _partialIndent = "";
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
            _partialIndent = string.Format("{0}{1}", _partialIndent, partial.Indent);

            PartialDefinition partialDefinition;
            _partialDefinitions.TryGetValue(partial.Key, out partialDefinition);
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

            List<string> keys = path.Split('.').ToList();

            return keys.Count == 0 ? null : GetValue(keys);
        }

        private object GetValue(List<string> keys)
        {
            object value = GetValue(_currentContext, keys[0]);

            if (keys.Count == 1)
            {
                if (value != null) return value;

                foreach (object context in _parentContexts)
                {
                    value = GetValue(context, keys[0]);
                    if (value != null) return value;
                }
                return null;
            }

            if (value == null) return null;

            _parentContexts.Push(_currentContext);
            _currentContext = value;
            value = GetValue(keys.GetRange(1, keys.Count - 1));
            _currentContext = _parentContexts.Pop();

            return value;
        }

        private static object GetValue(object dataContext, string key)
        {
            var dictionary = dataContext as IDictionary;
            if (dictionary != null)
            {
                return dictionary.Contains(key) ? dictionary[key] : null;
            }

            Type type = dataContext.GetType();

            FieldInfo fieldInfo = type.GetField(key);
            if (fieldInfo != null)
            {
                return fieldInfo.GetValue(dataContext);
            }

            PropertyInfo propertyInfo = type.GetProperty(key);
            return propertyInfo?.GetValue(dataContext);
        }

        private IEnumerable<object> GetValuesForSection(string path)
        {
            object value = GetValue(path);
            if (value == null) yield break;

            if (value is bool)
            {
                if ((bool)value)
                {
                    yield return value;
                }
            }
            else if (value is string || value is IDictionary) // string and IDictionary are IEnumerable, so it needs to check string befoer IEnumerable
            {
                yield return value;
            }
            else if (value is IEnumerable)
            {
                foreach (object item in (IEnumerable)value)
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
