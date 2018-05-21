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
using System.Collections.Generic;

namespace Mustache
{
    public class Template : PartialDefinition
    {
        private readonly Dictionary<string, PartialDefinition> _partialDefinitions =
            new Dictionary<string, PartialDefinition>();

        public static Template Compile(string template)
        {
            return Builder.Build(new Parser(template).Parse());
        }

        internal Template() : base(string.Empty) {}

        internal void Add(PartialDefinition partialDefinition)
        {
            _partialDefinitions.Add(partialDefinition.Key, partialDefinition);
        }

        public string Render(object data, Dictionary<string, string> partials = null)
        {
            var currentPartials = new Dictionary<string, PartialDefinition>(_partialDefinitions);
            if (partials != null)
            {
                foreach (KeyValuePair<string, string> keyValuePair in partials)
                {
                    currentPartials[keyValuePair.Key] = Compile(keyValuePair.Value);
                }
            }

            var dataRenderer = new DataRenderer(data, currentPartials);
            Accept(dataRenderer);
            return dataRenderer.Result;
        }

        internal override void Accept(IElementRenderer renderer)
        {
            renderer.Render(this);
        }
    }
}