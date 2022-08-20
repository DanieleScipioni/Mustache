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
        private readonly Stack<object> _parentContexts;
        private readonly StringBuilder _stringBuilder;

        private object _currentContext;
        private string _partialIndent;

        private string _openDelimiter;
        private string _closeDelimiter;

        internal DataRenderer(object data, Dictionary<string, PartialDefinition> partialDefinitions)
        {
            _currentContext = data;
            _partialDefinitions = partialDefinitions;
            _parentContexts = new Stack<object>();
            _stringBuilder = new StringBuilder();
            _partialIndent = string.Empty;
            _openDelimiter = Parser.MustacheOpenDelimiter;
            _closeDelimiter = Parser.MustacheCloseDelimiter;
        }

        internal string Result => _stringBuilder.ToString();

        public void Render(Section section)
        {
            foreach ((object value, bool lambda) in GetValuesForSection(section.Key, section.RawText))
            {
                if (lambda && value is string stringValue)
                {
                    _stringBuilder.Append(stringValue);
                    continue;
                }

                _parentContexts.Push(_currentContext);
                _currentContext = value;

                foreach (Element element in section.Elements())
                {
                    element.Accept(this);
                }

                _currentContext = _parentContexts.Pop();
            }
        }

        public void Render(InvertedSection invertedSection)
        {
            IEnumerable<(object value, bool lambda)> enumerable = GetValuesForSection(invertedSection.Key, invertedSection.RawText);
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
            (bool _, object value) = GetValue(variable.Key, RenderType.Variable, null);
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

        public void Render(Delimiters delimiters)
        {
            _openDelimiter = delimiters.OpenDelimiter;
            _closeDelimiter = delimiters.CloseDelimiter;
        }

        private (bool lambda, object value) GetValue(string path, RenderType renderType, string rawText)
        {
            if (path == ".") return (false, _currentContext);

            string[] keys = path.Split('.');

            return keys.Length == 0 ? (false, null) : GetValue(new ArraySegment<string>(keys), renderType, rawText);
        }

        private (bool lambda, object value) GetValue(ArraySegment<string> keys, RenderType renderType, string rawText)
        {
            // ReSharper disable once PossibleNullReferenceException
            (bool keyFound, bool lambda, object value) = GetValueFromDatacontext(_currentContext, keys.Array[keys.Offset], renderType, rawText);

            if (keyFound)
            {
                if (value == null || keys.Count == 1) return (lambda, value);
                _parentContexts.Push(_currentContext);
                _currentContext = value;
                (lambda, value) = GetValue(new ArraySegment<string>(keys.Array, keys.Offset + 1, keys.Count - 1), renderType, rawText);
                _currentContext = _parentContexts.Pop();
                return (lambda, value);
            }

            foreach (object context in _parentContexts)
            {
                (keyFound, lambda, value) = GetValueFromDatacontext(context, keys.Array[keys.Offset], renderType, rawText);
                if (!keyFound) continue;

                if (value == null || keys.Count == 1) return (lambda, value);
                _parentContexts.Push(_currentContext);
                _currentContext = value;
                (lambda, value) = GetValue(new ArraySegment<string>(keys.Array, keys.Offset + 1, keys.Count - 1), renderType, rawText);
                _currentContext = _parentContexts.Pop();
                return (lambda, value);
            }

            return (false, null);
        }

        private (bool keyFound, bool lambda, object value) GetValueFromDatacontext(object dataContext, string key, RenderType renderType, string rawText)
        {
            if (dataContext is IDictionary dictionary)
            {
                if (!dictionary.Contains(key)) return (false, false, null);

                object value = dictionary[key];
                if (!(value is Delegate)) return (true, false, value);

                if (!(value is Func<string, object> lambda)) return (true, false, null);

                object lambdaResult = lambda(rawText);
                if (!(lambdaResult is string lambdaString)) return (true, true, lambdaResult);

                value = RenderLambdaResult(lambdaString, renderType);
                return (true, true, value);
            }

            Type type = dataContext.GetType();

            FieldInfo fieldInfo = type.GetField(key);
            if (fieldInfo != null)
            {
                return (true, false, fieldInfo.GetValue(dataContext));
            }

            PropertyInfo propertyInfo = type.GetProperty(key);
            if (propertyInfo != null)
            {
                object value = propertyInfo.GetValue(dataContext);
                if (!(value is Delegate)) return (true, false, value);

                if (!(value is Func<string, object> lambda)) return (true, false, null);

                object lambdaResult = lambda(rawText);
                if (!(lambdaResult is string lambdaString)) return (true, true, lambdaResult);

                value = RenderLambdaResult(lambdaString, renderType);
                return (true, true, value);
            }

            MethodInfo methodInfo = type.GetMethod(key, BindingFlags.Public | BindingFlags.Instance);
            if (methodInfo == null) return (false, false, null);

            ParameterInfo[] parameters = methodInfo.GetParameters();
            if (parameters.Length != 1 || parameters[0].ParameterType != typeof(string)) return (false, false, null);
            if (methodInfo.ReturnType == typeof(void)) return (false, false, null);

            {
                object lambdaResult = methodInfo.Invoke(dataContext, new object[] {rawText});
                if (!(lambdaResult is string lambdaString)) return (true, true, lambdaResult);

                string value = RenderLambdaResult(lambdaString, renderType);
                return (true, true, value);
            }
        }

        private string RenderLambdaResult(string template, RenderType renderType)
        {
            Template compiledTemplate;
            switch (renderType)
            {
                case RenderType.Variable:
                    compiledTemplate = Builder.Build(new Parser(template, Parser.MustacheOpenDelimiter, Parser.MustacheCloseDelimiter).Parse(), template);
                    break;
                case RenderType.Section:
                    compiledTemplate = Builder.Build(new Parser(template, _openDelimiter, _closeDelimiter).Parse(), template);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(renderType), renderType, null);
            }
            return compiledTemplate.Render(_currentContext);
        }

        private IEnumerable<(object value, bool lambda)> GetValuesForSection(string path, string rawText)
        {
            (bool lambda, object value) = GetValue(path, RenderType.Section, rawText);
            if (value == null) yield break;

            if (value is bool boolValue)
            {
                if (boolValue)
                {
                    yield return (true, lambda);
                }
            }
            else if (value is string || value is IDictionary) // string and IDictionary are IEnumerable, so it needs to check string befoer IEnumerable
            {
                yield return (value, lambda);
            }
            else if (value is IEnumerable enumerable)
            {
                foreach (object item in enumerable)
                {
                    yield return (item, lambda);
                }
            }
            else
            {
                yield return (value, lambda);
            }
        }
    }
}
