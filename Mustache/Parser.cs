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
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Mustache
{
    internal class Parser
    {
        private const string MustacheOpenDelimiter = "{{";
        private const string MustacheCloseDelimiter = "}}";

        public static readonly Regex DoNotEscapeHtmlWithMustache = new Regex(@"^\{.*?\}$");
        public static readonly Regex DoNotEscapeHtmlWithAmpersand = new Regex(@"^\&");

        private static readonly Regex ChangeDelimitersRegex = new Regex(@"^=[\t ]*(\S+)[\t ]+(\S+)[\t ]*=$");

        private string _openDelimiter;
        private string _closeDelimiter;
        private Regex _openDelimiterRegex;
        private Regex _closeDelimiterRegex;

        private readonly Stack<Block> _blocks = new Stack<Block>();
        private readonly string _template;

        public Parser(string template)
        {
            _template = template;
            SetDelimiters(MustacheOpenDelimiter, MustacheCloseDelimiter);
        }

        private void SetDelimiters(string openDelimiter, string closeDelimiter)
        {
            _openDelimiter = openDelimiter;
            _closeDelimiter = closeDelimiter;

            _openDelimiterRegex = new Regex($@"(^|\r?\n)?([\t\f\v ]*)({Regex.Escape(_openDelimiter)})");
            _closeDelimiterRegex = _closeDelimiter == MustacheCloseDelimiter
                ? new Regex($@"{Regex.Escape("}")}*({Regex.Escape(_closeDelimiter)})([\t\f\v ]*(\r?\n|$))?")
                : new Regex($@"({Regex.Escape(_closeDelimiter)})([\t\f\v ]*(\r?\n|$))?");
        }

        internal IEnumerable<Element> Parse()
        {
            var currentPos = 0;
            var previousTagWasClosePartialDefinition = false;

            if (_blocks.Count == 0)
            {
                var template = new Template();
                _blocks.Push(template);
                yield return template;
            }

            while (currentPos < _template.Length)
            {
                Match openMatch = _openDelimiterRegex.Match(_template, currentPos);
                if (openMatch.Success)
                {
                    Group newLineBeforeTagGroup = openMatch.Groups[1];
                    Group whiteSpacesBeforeTagGroup = openMatch.Groups[2];
                    Group openDelimiterGroup = openMatch.Groups[3];
                    bool emptyStringBeforeTag = openDelimiterGroup.Index == currentPos;

                    StringBuilder plainTextBeforeTag =
                        new StringBuilder(_template.Substring(currentPos, openMatch.Index - currentPos))
                        .Append(newLineBeforeTagGroup);

                    currentPos = openMatch.Index + openMatch.Length;

                    Match closeMatch = _closeDelimiterRegex.Match(_template, currentPos);
                    Group closeDelimiterGroup = closeMatch.Groups[1];
                    Group newLineAfterTagGroup = closeMatch.Groups[3];

                    string tagKey = _template.Substring(currentPos, closeDelimiterGroup.Index - currentPos);

                    bool isStandalone = ((previousTagWasClosePartialDefinition || newLineBeforeTagGroup.Success) && whiteSpacesBeforeTagGroup.Success || emptyStringBeforeTag) &&
                                        newLineAfterTagGroup.Success;
                    Tag tag = CreateTag(tagKey, whiteSpacesBeforeTagGroup.Value, isStandalone);

                    isStandalone = isStandalone && !(tag is Variable);

                    if (!isStandalone) plainTextBeforeTag.Append(whiteSpacesBeforeTagGroup);

                    previousTagWasClosePartialDefinition = HandleClosePartialDefinition(tag);

                    currentPos = isStandalone || previousTagWasClosePartialDefinition
                        ? closeMatch.Index + closeMatch.Length
                        : closeDelimiterGroup.Index + closeDelimiterGroup.Length;

                    if (plainTextBeforeTag.Length != 0) yield return new TextElement(plainTextBeforeTag.ToString());

                    if (tag != null) yield return tag;
                }
                else
                {
                    yield return new TextElement(_template.Substring(currentPos, _template.Length - currentPos));
                    currentPos = _template.Length;
                }
            }
        }

        private bool HandleClosePartialDefinition(Tag tag)
        {
            switch (tag)
            {
                case Block block:
                    _blocks.Push(block);
                    break;
                case EndBlock _:
                {
                    if (!(_blocks.Pop() is PartialDefinition _)) return false;

                    SetDelimiters(MustacheOpenDelimiter, MustacheCloseDelimiter);
                    return true;
                }
            }

            return false;
        }

        private Tag CreateTag(string tagKey, string indentSpaces, bool isStandalone)
        {
            char tagType = tagKey[0];
            switch (tagType)
            {
                case '!': // comment
                    return null;
                case '#':
                    return new Section(tagKey.Substring(1).Trim());
                case '^':
                    return new InvertedSection(tagKey.Substring(1).Trim());
                case '<':
                    return new PartialDefinition(tagKey.Substring(1).Trim());
                case '>':
                    return new Partial(tagKey.Substring(1).Trim(), isStandalone ? indentSpaces : "");
                case '/':
                    return new EndBlock(tagKey.Substring(1).Trim());
                case '=': // set delimiter
                    Match match = ChangeDelimitersRegex.Match(tagKey);
                    Group newOpenDelimiter = match.Groups[1];
                    Group newCloseDelimiter = match.Groups[2];
                    if (newOpenDelimiter.Success && newCloseDelimiter.Success)
                    {
                        SetDelimiters(newOpenDelimiter.Value, newCloseDelimiter.Value);
                    }
                    return null;
                default:
                    bool doNotEscapeHtml = DoNotEscapeHtmlWithAmpersand.Match(tagKey).Success;
                    if (doNotEscapeHtml)
                    {
                        tagKey = tagKey.Substring(1);
                    }
                    else if (_openDelimiter == MustacheOpenDelimiter && _closeDelimiter == MustacheCloseDelimiter)
                    {
                        doNotEscapeHtml = DoNotEscapeHtmlWithMustache.Match(tagKey).Success;
                        if (doNotEscapeHtml)
                        {
                            tagKey = tagKey.Substring(1, tagKey.Length - 2);
                        }
                    }
                    return new Variable(tagKey.Trim(), !doNotEscapeHtml);
            }
        }
    }
}
