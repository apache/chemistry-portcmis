/*
* Licensed to the Apache Software Foundation (ASF) under one
* or more contributor license agreements. See the NOTICE file
* distributed with this work for additional information
* regarding copyright ownership. The ASF licenses this file
* to you under the Apache License, Version 2.0 (the
* "License"); you may not use this file except in compliance
* with the License. You may obtain a copy of the License at
*
* http://www.apache.org/licenses/LICENSE-2.0
*
* Unless required by applicable law or agreed to in writing,
* software distributed under the License is distributed on an
* "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY
* Kind, either express or implied. See the License for the
* specific language governing permissions and limitations
* under the License.
*/

using PortCMIS.Binding.AtomPub;
using PortCMIS.Data.Extensions;
using PortCMIS.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using PortCMIS.Enums;
using System.Globalization;

namespace PortCMIS.Binding.AtomPub
{
    internal abstract class XMLWalker<T>
    {

        public T walk(XmlReader parser)
        {
            T result = prepareTarget(parser, parser.LocalName, parser.NamespaceURI);

            XmlUtils.next(parser);

            // walk through all tags
            while (true)
            {
                XmlNodeType nodeType = parser.NodeType;
                if (nodeType == XmlNodeType.Element)
                {
                    if (!read(parser, parser.LocalName, parser.NamespaceURI, result))
                    {
                        if (result is IExtensionsData)
                        {
                            handleExtension(parser, (IExtensionsData)result);
                        }
                        else
                        {
                            XmlUtils.skip(parser);
                        }
                    }
                }
                else if (nodeType == XmlNodeType.EndElement)
                {
                    break;
                }
                else
                {
                    if (!XmlUtils.next(parser))
                    {
                        break;
                    }
                }
            }

            XmlUtils.next(parser);

            return result;
        }

        protected bool isCmisNamespace(string ns)
        {
            return XmlConstants.NAMESPACE_CMIS == ns;
        }

        protected bool isAtomNamespace(string ns)
        {
            return XmlConstants.NAMESPACE_ATOM == ns;
        }

        protected bool isTag(string name, string tag)
        {
            return name == tag;
        }

        protected void handleExtension(XmlReader parser, IExtensionsData extData)
        {
            IList<ICmisExtensionElement> extensions = extData.Extensions;
            if (extensions == null)
            {
                extensions = new List<ICmisExtensionElement>();
                extData.Extensions = extensions;
            }

            if (extensions.Count + 1 > XMLConstraints.MAX_EXTENSIONS_WIDTH)
            {
                throw new CmisInvalidArgumentException("Too many extensions!");
            }

            extensions.Add(handleExtensionLevel(parser, 0));
        }

        private ICmisExtensionElement handleExtensionLevel(XmlReader parser, int level)
        {
            string localname = parser.Name;
            string ns = parser.NamespaceURI;
            IDictionary<string, string> attributes = null;
            StringBuilder sb = new StringBuilder();
            IList<ICmisExtensionElement> children = null;

            if (parser.HasAttributes)
            {
                attributes = new Dictionary<string, string>();

                while (parser.MoveToNextAttribute())
                {
                    attributes.Add(parser.Name, parser.Value);
                }
            }

            XmlUtils.next(parser);

            while (true)
            {
                XmlNodeType nodeType = parser.NodeType;
                if (nodeType == XmlNodeType.EndElement)
                {
                    break;
                }
                else if (nodeType == XmlNodeType.Text)
                {
                    string s = parser.Value;
                    if (s != null)
                    {
                        if (sb.Length + s.Length > XMLConstraints.MAX_STRING_LENGTH)
                        {
                            throw new CmisInvalidArgumentException("String limit exceeded!");
                        }
                        sb.Append(s);
                    }
                }
                else if (nodeType == XmlNodeType.Element)
                {
                    if (level + 1 > XMLConstraints.MAX_EXTENSIONS_DEPTH)
                    {
                        throw new CmisInvalidArgumentException("Extensions tree too deep!");
                    }

                    if (children == null)
                    {
                        children = new List<ICmisExtensionElement>();
                    }

                    if (children.Count + 1 > XMLConstraints.MAX_EXTENSIONS_WIDTH)
                    {
                        throw new CmisInvalidArgumentException("Extensions tree too wide!");
                    }

                    children.Add(handleExtensionLevel(parser, level + 1));

                    continue;
                }

                if (!XmlUtils.next(parser))
                {
                    break;
                }
            }

            XmlUtils.next(parser);

            CmisExtensionElement element = new CmisExtensionElement()
            {
                Name = localname,
                Namespace = ns,
                Attributes = attributes
            };

            if (children != null)
            {
                element.Children = children;
            }
            else
            {
                element.Value = sb.ToString();
            }

            return element;
        }

        protected IList<S> addToList<S>(IList<S> list, S value)
        {
            if (list == null)
            {
                list = new List<S>();
            }
            list.Add(value);

            return list;
        }

        protected string readText(XmlReader parser)
        {
            return XmlUtils.readText(parser, XMLConstraints.MAX_STRING_LENGTH);
        }

        protected bool? readBoolean(XmlReader parser)
        {
            parser.MoveToContent();

            try
            {
                return parser.ReadContentAsBoolean();
            }
            catch (Exception e)
            {
                throw new CmisInvalidArgumentException("Invalid boolean value!", e);
            }
        }

        protected BigInteger readInteger(XmlReader parser)
        {
            string value = readText(parser);

            try
            {
                return BigInteger.Parse(value, NumberStyles.Integer, CultureInfo.InvariantCulture);
            }
            catch (Exception e)
            {
                throw new CmisInvalidArgumentException("Invalid integer value!", e);
            }
        }

        protected decimal readDecimal(XmlReader parser)
        {
            parser.MoveToContent();

            try
            {
                return parser.ReadContentAsDecimal();
            }
            catch (Exception e)
            {
                throw new CmisInvalidArgumentException("Invalid decimal value!", e);
            }
        }

        protected DateTime readDateTime(XmlReader parser)
        {
            string value = readText(parser);

            DateTime result = DateTimeHelper.ParseISO8601(value);
            if (result == null)
            {
                throw new CmisInvalidArgumentException("Invalid datetime value!");
            }

            return result;
        }

        protected E readEnum<E>(XmlReader parser)
        {
            return readText(parser).GetCmisEnum<E>();
        }

        protected abstract T prepareTarget(XmlReader parser, string localname, string ns);

        protected abstract bool read(XmlReader parser, string localname, string ns, T target);

    }

    internal class XMLConstraints
    {
        public const int MAX_STRING_LENGTH = 100 * 1024;

        public const int MAX_EXTENSIONS_WIDTH = 500;
        public const int MAX_EXTENSIONS_DEPTH = 20;
    }
}