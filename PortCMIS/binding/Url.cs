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

using PortCMIS.Enums;
using System;
using System.Globalization;
using System.Numerics;
using System.Text;

namespace PortCMIS.Binding
{
    public class UrlBuilder
    {
        private UriBuilder uri;

        public Uri Url
        {
            get { return uri.Uri; }
        }

        public UrlBuilder(string url)
        {
            if (url == null)
            {
                throw new ArgumentNullException("url");
            }

            uri = new UriBuilder(url);
        }

        public UrlBuilder AddParameter(string name, object value)
        {
            if (name == null || value == null)
            {
                return this;
            }

            string valueStr = Uri.EscapeDataString(UrlBuilder.NormalizeParameter(value));

            if (uri.Query != null && uri.Query.Length > 1)
            {
                uri.Query = uri.Query.Substring(1) + "&" + name + "=" + valueStr;
            }
            else
            {
                uri.Query = name + "=" + valueStr;
            }

            return this;
        }

        public UrlBuilder AddPath(string path)
        {
            return AddPathPart(path, false);
        }

        protected UrlBuilder AddPathPart(string part, bool quoteSlash)
        {
            if (String.IsNullOrEmpty(part))
            {
                return this;
            }
            if (uri.Path.Length == 0 || uri.Path[uri.Path.Length - 1] != '/')
            {
                uri.Path += "/";
            }
            if (part[0] == '/')
            {
                part = part.Substring(1);
            }
            uri.Path += QuoteURIPathComponent(part, quoteSlash);

            return this;
        }


        private static readonly char[] RFC7232Reserved = ";?:@&=+$,[]".ToCharArray();

        public string QuoteURIPathComponent(string s, bool quoteSlash)
        {
            if (String.IsNullOrEmpty(s))
            {
                return s;
            }

            StringBuilder result = new StringBuilder(s);

            foreach (char c in RFC7232Reserved)
            {
                result.Replace(c.ToString(), "%" + Convert.ToByte(c).ToString("X"));
            }

            if (quoteSlash)
            {
                result.Replace("/", "%2F");
            }

            return result.ToString();
        }

        public static string NormalizeParameter(object value)
        {
            if (value == null)
            {
                return null;
            }
            else if (value is Enum)
            {
                return ((Enum)value).GetCmisValue();
            }
            else if (value is bool)
            {
                return (bool)value ? "true" : "false";
            }
            else if (value is BigInteger)
            {
                return ((BigInteger)value).ToString("#", CultureInfo.InvariantCulture);
            }

            return value.ToString();
        }

        public override string ToString()
        {
            return Url.ToString();
        }
    }

    internal class MimeHelper
    {
        public const string ContentDisposition = "Content-Disposition";
        public const string DispositionAttachment = "attachment";
        public const string DispositionFilename = "filename";

        private const string MIMESpecials = "()<>@,;:\\\"/[]?=" + "\t ";
        private const string RFC2231Specials = "*'%" + MIMESpecials;
        private static char[] HexDigits = new char[] { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', 'A', 'B', 'C', 'D', 'E', 'F' };

        public static string EncodeContentDisposition(string disposition, string filename)
        {
            if (disposition == null)
            {
                disposition = DispositionAttachment;
            }
            return disposition + EncodeRFC2231(DispositionFilename, filename);
        }

        protected static string EncodeRFC2231(string key, string value)
        {
            StringBuilder buf = new StringBuilder();
            bool encoded = EncodeRFC2231value(value, buf);
            if (encoded)
            {
                return "; " + key + "*=" + buf.ToString();
            }
            else
            {
                return "; " + key + "=" + value;
            }
        }

        protected static bool EncodeRFC2231value(string value, StringBuilder buf)
        {
            buf.Append("UTF-8");
            buf.Append("''"); // no language
            byte[] bytes;
            try
            {
                bytes = UTF8Encoding.UTF8.GetBytes(value);
            }
            catch (Exception)
            {
                return true;
            }

            bool encoded = false;
            for (int i = 0; i < bytes.Length; i++)
            {
                int ch = bytes[i] & 0xff;
                if (ch <= 32 || ch >= 127 || RFC2231Specials.IndexOf((char)ch) != -1)
                {
                    buf.Append('%');
                    buf.Append(HexDigits[ch >> 4]);
                    buf.Append(HexDigits[ch & 0xf]);
                    encoded = true;
                }
                else
                {
                    buf.Append((char)ch);
                }
            }
            return encoded;
        }
    }
}
