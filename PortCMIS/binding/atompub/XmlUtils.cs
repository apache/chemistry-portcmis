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
    class XmlUtils
    {

        // --------------
        // --- writer ---
        // --------------

        /**
         * Creates a new XML writer.
         */
        public static XmlWriter createWriter(Stream stream)
        {
            XmlWriterSettings settings = new XmlWriterSettings();
            settings.Encoding = Encoding.UTF8;
            return XmlWriter.Create(stream, settings);
        }

        /**
         * Starts a XML document.
         */
        public static void startXmlDocument(XmlWriter writer, string rootLocalName, string rootNamespace)
        {
            // writer.setPrefix(XmlConstants.PREFIX_ATOM, XmlConstants.NAMESPACE_ATOM);
            // writer.setPrefix(XmlConstants.PREFIX_CMIS, XmlConstants.NAMESPACE_CMIS);
            // writer.setPrefix(XmlConstants.PREFIX_RESTATOM, XmlConstants.NAMESPACE_RESTATOM);
            // writer.setPrefix(XmlConstants.PREFIX_APACHE_CHEMISTY, XmlConstants.NAMESPACE_APACHE_CHEMISTRY);

            writer.WriteStartDocument();
        }

        /**
         * Ends a XML document.
         */
        public static void endXmlDocument(XmlWriter writer)
        {
            writer.WriteEndDocument();
            writer.Dispose();
        }

        /**
         * Writes a String tag.
         */
        public static void write(XmlWriter writer, string prefix, string ns, string tag, string value)
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

        /**
         * Writes an Integer tag.
         */
        public static void write(XmlWriter writer, string prefix, string ns, string tag, BigInteger? value)
        {
            if (value == null)
            {
                return;
            }

            write(writer, prefix, ns, tag, ((BigInteger)value).ToString("#", CultureInfo.InvariantCulture));
        }

        /**
         * Writes a Decimal tag.
         */
        public static void write(XmlWriter writer, String prefix, String ns, String tag, decimal? value)
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

        /**
         * Writes a DateTime tag.
         */
        public static void write(XmlWriter writer, String prefix, String ns, String tag, DateTime? value)
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

        /**
         * Writes a Boolean tag.
         */
        public static void write(XmlWriter writer, string prefix, string ns, string tag, bool? value)
        {
            if (value == null)
            {
                return;
            }

            write(writer, prefix, ns, tag, (bool)value ? "true" : "false");
        }

        /**
         * Writes an Enum tag.
         */
        public static void write(XmlWriter writer, String prefix, String ns, String tag, Enum value)
        {
            if (value == null)
            {
                return;
            }

            write(writer, prefix, ns, tag, value.GetCmisValue());
        }


        // ---------------
        // ---- parser ---
        // ---------------

        /**
         * Creates a new XML parser with OpenCMIS default settings.
         */
        public static XmlReader createParser(Stream stream)
        {
            XmlReaderSettings settings = new XmlReaderSettings();
            settings.MaxCharactersInDocument = 0;
            settings.CloseInput = true;
            return XmlReader.Create(stream, settings);
        }

        /**
         * Moves the parser to the next element.
         */
        public static bool next(XmlReader parser)
        {
            return parser.Read();
        }

        /**
         * Skips a tag or subtree.
         */
        public static void skip(XmlReader parser)
        {
            parser.Skip();
        }

        /**
         * Moves the parser to the next start element.
         * 
         * @return <code>true</code> if another start element has been found,
         *         <code>false</code> otherwise
         */
        public static bool findNextStartElemenet(XmlReader parser)
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

        /**
         * Parses a tag that contains text.
         */
        public static String readText(XmlReader parser, int maxLength)
        {
            StringBuilder sb = new StringBuilder();

            next(parser);

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

                if (!next(parser))
                {
                    break;
                }
            }

            next(parser);

            return sb.ToString();
        }
    }
}
