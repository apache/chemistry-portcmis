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

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using PortCMIS.Enums;
using System.Globalization;
using PortCMIS.Exceptions;

namespace PortCMIS.Binding.AtomPub
{
    internal static class XmlUtils
    {

        // --------------
        // --- writer ---
        // --------------

        /// <summary>
        /// Creates a new XML writer.
        /// </summary>
        public static XmlWriter CreateWriter(Stream stream)
        {
            XmlWriterSettings settings = new XmlWriterSettings()
            {
                Encoding = Encoding.UTF8
            };

            return XmlWriter.Create(stream, settings);
        }

        /// <summary>
        /// Starts a XML document.
        /// </summary>
        public static void StartXmlDocument(XmlWriter writer)
        {
            // writer.setPrefix(XmlConstants.PREFIX_ATOM, XmlConstants.NAMESPACE_ATOM);
            // writer.setPrefix(XmlConstants.PREFIX_CMIS, XmlConstants.NAMESPACE_CMIS);
            // writer.setPrefix(XmlConstants.PREFIX_RESTATOM, XmlConstants.NAMESPACE_RESTATOM);
            // writer.setPrefix(XmlConstants.PREFIX_APACHE_CHEMISTY, XmlConstants.NAMESPACE_APACHE_CHEMISTRY);

            writer.WriteStartDocument();
        }

        /// <summary>
        /// Ends a XML document.
        /// </summary>
        public static void EndXmlDocument(XmlWriter writer)
        {
            writer.WriteEndDocument();
        }

        /// <summary>
        /// Writes a String tag.
        /// </summary>
        public static void Write(XmlWriter writer, string prefix, string ns, string tag, string value)
        {
            if (value == null)
            {
                return;
            }

            if (ns == null)
            {
                writer.WriteStartElement(tag);
            }
            else
            {
                writer.WriteStartElement(prefix, tag, ns);
            }
            writer.WriteString(value);
            writer.WriteEndElement();
        }

        /// <summary>
        /// Writes an Integer tag.
        /// </summary>
        public static void Write(XmlWriter writer, string prefix, string ns, string tag, BigInteger? value)
        {
            if (value == null)
            {
                return;
            }

            Write(writer, prefix, ns, tag, ((BigInteger)value).ToString("0", CultureInfo.InvariantCulture));
        }

        /// <summary>
        /// Writes a Decimal tag.
        /// </summary>
        public static void Write(XmlWriter writer, String prefix, String ns, String tag, decimal? value)
        {
            if (value == null)
            {
                return;
            }

            if (ns == null)
            {
                writer.WriteStartElement(tag);
            }
            else
            {
                writer.WriteStartElement(prefix, tag, ns);
            }
            writer.WriteValue(value);
            writer.WriteEndElement();
        }

        /// <summary>
        /// Writes a DateTime tag.
        /// </summary>
        public static void Write(XmlWriter writer, String prefix, String ns, String tag, DateTime? value)
        {
            if (value == null)
            {
                return;
            }

            if (ns == null)
            {
                writer.WriteStartElement(tag);
            }
            else
            {
                writer.WriteStartElement(prefix, tag, ns);
            }
            writer.WriteValue(value);
            writer.WriteEndElement();
        }

        /// <summary>
        /// Writes a Boolean tag.
        /// </summary>
        public static void Write(XmlWriter writer, string prefix, string ns, string tag, bool? value)
        {
            if (value == null)
            {
                return;
            }

            Write(writer, prefix, ns, tag, (bool)value ? "true" : "false");
        }

        /// <summary>
        /// Writes an Enum tag.
        /// </summary>
        public static void Write(XmlWriter writer, String prefix, String ns, String tag, Enum value)
        {
            if (value == null)
            {
                return;
            }

            Write(writer, prefix, ns, tag, value.GetCmisValue());
        }


        // ---------------
        // ---- parser ---
        // ---------------

        /// <summary>
        /// Creates a new XML parser with OpenCMIS default settings.
        /// </summary>
        public static XmlReader CreateParser(Stream stream)
        {
            XmlReaderSettings settings = new XmlReaderSettings()
            {
                MaxCharactersInDocument = 0,
                CloseInput = true
            };

            return XmlReader.Create(stream, settings);
        }

        /// <summary>
        /// Moves the parser to the next element.
        /// </summary>
        public static bool Next(XmlReader parser)
        {
            return parser.Read();
        }

        /// <summary>
        /// Skips a tag or subtree.
        /// </summary>
        public static void Skip(XmlReader parser)
        {
            parser.Skip();
        }

        /// <summary>
        /// Moves the parser to the next start element.
        /// <returns><c>true</c> if another start element has been found, <c>false</c> otherwise</returns>
        /// </summary>
        public static bool FindNextStartElemenet(XmlReader parser)
        {
            while (true)
            {
                XmlNodeType nodeType = parser.NodeType;

                if (nodeType == XmlNodeType.Element)
                {
                    return true;
                }

                if (!parser.Read())
                {
                    return false;
                }
            }
        }

        /// <summary>
        /// Parses a tag that contains text.
        /// </summary>
        public static String ReadText(XmlReader parser, int maxLength)
        {
            StringBuilder sb = new StringBuilder();

            if (!parser.IsEmptyElement)
            {
                Next(parser);

                while (true)
                {
                    XmlNodeType nodeType = parser.NodeType;
                    if (nodeType == XmlNodeType.EndElement)
                    {
                        break;
                    }
                    else if (nodeType == XmlNodeType.Text || nodeType == XmlNodeType.CDATA)
                    {
                        char[] buffer = new char[8 * 1024];

                        int len;
                        while ((len = parser.ReadValueChunk(buffer, 0, buffer.Length)) > 0)
                        {
                            if (sb.Length + len > maxLength)
                            {
                                throw new CmisInvalidArgumentException("String limit exceeded!");
                            }

                            sb.Append(buffer, 0, len);
                        }
                    }
                    else if (nodeType == XmlNodeType.Element)
                    {
                        throw new CmisInvalidArgumentException("Unexpected tag: " + parser.Name);
                    }

                    if (!Next(parser))
                    {
                        break;
                    }
                }
            }

            Next(parser);

            return sb.ToString();
        }
    }
}
