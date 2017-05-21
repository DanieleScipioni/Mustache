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

using System;
using System.Collections.Generic;
using System.Linq;
using Mustache.Elements;

namespace Mustache
{
    public static class Builder
    {
        public static Template Build(IEnumerable<Element> parts)
        {
            if (parts == null) throw new ArgumentNullException(nameof(parts));

            var template = new Template();
            var blocks = new Stack<Block>();

            var currentBlock = (Block) template;

            foreach (Element element in parts)
            {
                var endBlock = element as EndBlock;
                if (endBlock == null)
                {
                    var partialDefinition = element as PartialDefinition;
                    if (partialDefinition != null)
                    {
                        template.Add(partialDefinition);
                    }
                    currentBlock.Add(element);
                    var block = element as Block;
                    if (block == null) continue;
                    blocks.Push(currentBlock);
                    currentBlock = block;
                }
                else
                {
                    if (endBlock.Key != currentBlock.Key)
                    {
                        throw new MustacheException($"End section tag {endBlock.Key} does not match section start tag");
                    }
                    currentBlock = blocks.Pop();
                }
            }

            if (blocks.Count > 0)
            {
                throw new MustacheException(string.Format("Sections '{0}' are without end section",
                    string.Join(", ", blocks.Select(s => s.Key))));
            }

            return template;
        }
    }
}