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

/**
 * This JSON parser implemenation is based on
 * JSON.simple <http://code.google.com/p/json-simple/>.
 */

using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace PortCMIS.Binding.Browser.Json
{
    /// <summary>
    /// JSON stream interface.
    /// </summary>
    public interface IJsonStreamAware
    {
        void WriteJsonString(TextWriter writer);
    }

    /// <summary>
    /// JSON Value helpers.
    /// </summary>
    public class JsonValue
    {
        public static void WriteJsonString(object value, TextWriter writer)
        {
            if (value == null)
            {
                writer.Write("null");
                return;
            }

            if (value is string)
            {
                writer.Write('\"');
                writer.Write(Escape((string)value));
                writer.Write('\"');
                return;
            }

            if (value is Double)
            {
                if (Double.IsInfinity((Double)value) || Double.IsNaN((Double)value))
                {
                    writer.Write("null");
                }
                else
                {
                    writer.Write(((Double)value).ToString("#", CultureInfo.InvariantCulture));
                }
                return;
            }

            if (value is Single)
            {
                if (Single.IsInfinity((Single)value) || Single.IsNaN((Single)value))
                {
                    writer.Write("null");
                }
                else
                {
                    writer.Write(((Single)value).ToString("#", CultureInfo.InvariantCulture));
                }
                return;
            }

            if (value is decimal)
            {
                writer.Write(((decimal)value).ToString("#", CultureInfo.InvariantCulture));
                return;
            }

            if (value is BigInteger)
            {
                writer.Write(((BigInteger)value).ToString("#", CultureInfo.InvariantCulture));
                return;
            }

            if (value is Boolean)
            {
                writer.Write(value.ToString());
                return;
            }

            if (value is IJsonStreamAware)
            {
                ((IJsonStreamAware)value).WriteJsonString(writer);
                return;
            }

            try
            {
                long longValue = Convert.ToInt64(value);
                writer.Write(longValue.ToString("#", CultureInfo.InvariantCulture));
            }
            catch (Exception)
            {
                writer.Write(value.ToString());
            }
        }

        public static string Escape(string s)
        {
            if (s == null)
            {
                return null;
            }

            StringBuilder sb = new StringBuilder();
            Escape(s, sb);
            return sb.ToString();
        }

        private static void Escape(string s, StringBuilder sb)
        {
            for (int i = 0; i < s.Length; i++)
            {
                char ch = s[i];
                switch (ch)
                {
                    case '"':
                        sb.Append("\\\"");
                        break;
                    case '\\':
                        sb.Append("\\\\");
                        break;
                    case '\b':
                        sb.Append("\\b");
                        break;
                    case '\f':
                        sb.Append("\\f");
                        break;
                    case '\n':
                        sb.Append("\\n");
                        break;
                    case '\r':
                        sb.Append("\\r");
                        break;
                    case '\t':
                        sb.Append("\\t");
                        break;
                    case '/':
                        sb.Append("\\/");
                        break;
                    default:
                        if ((ch >= '\u0000' && ch <= '\u001F') || (ch >= '\u007F' && ch <= '\u009F') || (ch >= '\u2000' && ch <= '\u20FF'))
                        {
                            sb.Append("\\u");
                            sb.Append(Convert.ToUInt16(ch).ToString("X4", CultureInfo.InvariantCulture));
                        }
                        else
                        {
                            sb.Append(ch);
                        }
                        break;
                }
            }
        }
    }

    /// <summary>
    /// JSON object.
    /// </summary>
    public class JsonObject : IJsonStreamAware, IEnumerable<KeyValuePair<string, object>>
    {
        private Dictionary<string, KeyValuePair<string, object>> dict = new Dictionary<string, KeyValuePair<string, object>>();
        private List<KeyValuePair<string, object>> list = new List<KeyValuePair<string, object>>();

        public JsonObject()
        {
        }

        public object this[string key]
        {
            get
            {
                if (key == null)
                {
                    throw new ArgumentNullException("key");
                }

                return dict[key].Value;
            }

            set
            {
                if (key == null)
                {
                    throw new ArgumentNullException("key");
                }

                KeyValuePair<string, object> kv = new KeyValuePair<string, object>(key, value);

                if (dict.ContainsKey(key))
                {
                    list[list.FindIndex(x => x.Key == key)] = kv;
                    dict[key] = kv;
                }
                else
                {
                    list.Add(kv);
                    dict.Add(key, kv);
                }
            }
        }

        public int Count
        {
            get
            {
                return list.Count;
            }
        }

        public IList<string> Keys
        {
            get
            {
                List<String> result = new List<string>();
                foreach (KeyValuePair<string, object> kv in list)
                {
                    result.Add(kv.Key);
                }

                return result;
            }
        }

        public IList<object> Values
        {
            get
            {
                List<object> result = new List<object>();
                foreach (KeyValuePair<string, object> kv in list)
                {
                    result.Add(kv.Value);
                }

                return result;
            }
        }

        public void Add(string key, object value)
        {
            if (key == null)
            {
                throw new ArgumentNullException("key");
            }

            if (dict.ContainsKey(key))
            {
                throw new ArgumentException("Key already exists!", key);
            }

            this[key] = value;
        }

        public void Remove(string key)
        {
            if (key == null)
            {
                throw new ArgumentNullException("key");
            }

            if (dict.Remove(key))
            {
                RemoveFromList(key);
            }
        }

        public bool ContainsKey(string key)
        {
            return dict.ContainsKey(key);
        }

        public bool TryGetValue(string key, out object value)
        {
            if (dict.ContainsKey(key))
            {
                value = dict[key].Value;
                return true;
            }
            else
            {
                value = null;
                return false;
            }
        }

        IEnumerator<KeyValuePair<string, object>> IEnumerable<KeyValuePair<string, object>>.GetEnumerator()
        {
            return GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public IEnumerator<KeyValuePair<string, object>> GetEnumerator()
        {
            return list.GetEnumerator();
        }

        private void RemoveFromList(string key)
        {
            int index = list.FindIndex(x => x.Key == key);
            if (index >= 0)
            {
                list.RemoveAt(index);
            }
        }

        public void WriteJsonString(TextWriter writer)
        {
            bool first = true;

            writer.Write('{');

            foreach (KeyValuePair<string, object> keyValue in list)
            {
                if (first)
                {
                    first = false;
                }
                else
                {
                    writer.Write(',');
                }

                writer.Write('\"');
                writer.Write(JsonValue.Escape(keyValue.Key));
                writer.Write("\":");
                JsonValue.WriteJsonString(keyValue.Value, writer);
            }

            writer.Write('}');
        }

        public override string ToString()
        {
            StringWriter sw = new StringWriter();
            WriteJsonString(sw);
            return sw.ToString();
        }
    }

    /// <summary>
    /// JSON array.
    /// </summary>
    public class JsonArray : List<object>, IJsonStreamAware
    {
        public JsonArray()
        {
        }

        public void WriteJsonString(TextWriter writer)
        {
            bool first = true;

            writer.Write('[');

            foreach (object o in this)
            {
                if (first)
                {
                    first = false;
                }
                else
                {
                    writer.Write(',');
                }

                JsonValue.WriteJsonString(o, writer);
            }

            writer.Write(']');
        }

        public override string ToString()
        {
            StringWriter sw = new StringWriter();
            WriteJsonString(sw);
            return sw.ToString();
        }
    }

    /// <summary>
    /// JSON parser.
    /// </summary>
    public class JsonParser
    {
        public const int StatusInit = 0;
        public const int StatusInFinishedValue = 1; // string, number, boolean, null, object, array
        public const int StatusInObject = 2;
        public const int StatusInArray = 3;
        public const int StatusPassedPairKey = 4;
        public const int StatusInPairValue = 5;
        public const int StatusEnd = 6;
        public const int StatusInError = -1;

        private Yylex lexer = new Yylex(null);
        private Yytoken token = null;
        private int status = StatusInit;

        public int Position { get { return lexer.Position; } }

        private int PeekStatus(Stack<int> statusStack)
        {
            if (statusStack.Count == 0)
            {
                return -1;
            }

            return statusStack.Peek();
        }

        public void Reset()
        {
            token = null;
            status = StatusInit;
        }

        public void Reset(TextReader reader)
        {
            lexer.YyReset(reader);
            Reset();
        }

        public object Parse(TextReader reader)
        {
            Reset(reader);
            Stack<int> statusStack = new Stack<int>();
            Stack<object> valueStack = new Stack<object>();

            try
            {
                do
                {
                    NextToken();
                    switch (status)
                    {
                        case StatusInit:
                            switch (token.Type)
                            {
                                case Yytoken.TypeValue:
                                    status = StatusInFinishedValue;
                                    statusStack.Push(status);
                                    valueStack.Push(token.Value);
                                    break;
                                case Yytoken.TypeLeftBrace:
                                    status = StatusInObject;
                                    statusStack.Push(status);
                                    valueStack.Push(new JsonObject());
                                    break;
                                case Yytoken.TypeLeftSquare:
                                    status = StatusInArray;
                                    statusStack.Push(status);
                                    valueStack.Push(new JsonArray());
                                    break;
                                default:
                                    status = StatusInError;
                                    break;
                            }
                            break;

                        case StatusInFinishedValue:
                            if (token.Type == Yytoken.TypeEof)
                            {
                                return valueStack.Pop();
                            }
                            else
                            {
                                throw new JsonParseException(Position, JsonParseException.ErrorUnexpectedToken, token);
                            }

                        case StatusInObject:
                            switch (token.Type)
                            {
                                case Yytoken.TypeComma:
                                    break;
                                case Yytoken.TypeValue:
                                    if (token.Value is String)
                                    {
                                        String key = (String)token.Value;
                                        valueStack.Push(key);
                                        status = StatusPassedPairKey;
                                        statusStack.Push(status);
                                    }
                                    else
                                    {
                                        status = StatusInError;
                                    }
                                    break;
                                case Yytoken.TypeRightBrace:
                                    if (valueStack.Count > 1)
                                    {
                                        statusStack.Pop();
                                        valueStack.Pop();
                                        status = PeekStatus(statusStack);
                                    }
                                    else
                                    {
                                        status = StatusInFinishedValue;
                                    }
                                    break;
                                default:
                                    status = StatusInError;
                                    break;
                            }
                            break;

                        case StatusPassedPairKey:
                            switch (token.Type)
                            {
                                case Yytoken.TypeColon:
                                    break;
                                case Yytoken.TypeValue:
                                    statusStack.Pop();
                                    String key = (String)valueStack.Pop();
                                    JsonObject parent = (JsonObject)valueStack.Peek();
                                    parent.Add(key, token.Value);
                                    status = PeekStatus(statusStack);
                                    break;
                                case Yytoken.TypeLeftSquare:
                                    statusStack.Pop();
                                    key = (String)valueStack.Pop();
                                    parent = (JsonObject)valueStack.Peek();
                                    JsonArray newArray = new JsonArray();
                                    parent.Add(key, newArray);
                                    status = StatusInArray;
                                    statusStack.Push(status);
                                    valueStack.Push(newArray);
                                    break;
                                case Yytoken.TypeLeftBrace:
                                    statusStack.Pop();
                                    key = (String)valueStack.Pop();
                                    parent = (JsonObject)valueStack.Peek();
                                    JsonObject newObject = new JsonObject();
                                    parent.Add(key, newObject);
                                    status = StatusInObject;
                                    statusStack.Push(status);
                                    valueStack.Push(newObject);
                                    break;
                                default:
                                    status = StatusInError;
                                    break;
                            }
                            break;

                        case StatusInArray:
                            switch (token.Type)
                            {
                                case Yytoken.TypeComma:
                                    break;
                                case Yytoken.TypeValue:
                                    JsonArray val = (JsonArray)valueStack.Peek();
                                    val.Add(token.Value);
                                    break;
                                case Yytoken.TypeRightSquare:
                                    if (valueStack.Count > 1)
                                    {
                                        statusStack.Pop();
                                        valueStack.Pop();
                                        status = PeekStatus(statusStack);
                                    }
                                    else
                                    {
                                        status = StatusInFinishedValue;
                                    }
                                    break;
                                case Yytoken.TypeLeftBrace:
                                    val = (JsonArray)valueStack.Peek();
                                    JsonObject newObject = new JsonObject();
                                    val.Add(newObject);
                                    status = StatusInObject;
                                    statusStack.Push(status);
                                    valueStack.Push(newObject);
                                    break;
                                case Yytoken.TypeLeftSquare:
                                    val = (JsonArray)valueStack.Peek();
                                    JsonArray newArray = new JsonArray();
                                    val.Add(newArray);
                                    status = StatusInArray;
                                    statusStack.Push(status);
                                    valueStack.Push(newArray);
                                    break;
                                default:
                                    status = StatusInError;
                                    break;
                            }
                            break;
                        case StatusInError:
                            throw new JsonParseException(Position, JsonParseException.ErrorUnexpectedToken, token);
                    }
                    if (status == StatusInError)
                    {
                        throw new JsonParseException(Position, JsonParseException.ErrorUnexpectedToken, token);
                    }
                } while (token.Type != Yytoken.TypeEof);
            }
            catch (IOException)
            {
                throw;
            }

            throw new JsonParseException(Position, JsonParseException.ErrorUnexpectedToken, token);
        }

        private void NextToken()
        {
            token = lexer.YyLex();
            if (token == null)
            {
                token = new Yytoken(Yytoken.TypeEof, null);
            }
        }
    }

    /// <summary>
    /// JSON parser exception.
    /// </summary>
    public class JsonParseException : Exception
    {
        public const int ErrorUnexpectedChar = 0;
        public const int ErrorUnexpectedToken = 1;
        public const int ErrorUnexpectedException = 2;

        private int ErrorType { get; set; }
        private object UnexpectedObject { get; set; }
        private int Position { get; set; }

        public JsonParseException(int errorType) : this(-1, errorType, null) { }
        public JsonParseException(int errorType, object unexpectedObject) : this(-1, errorType, unexpectedObject) { }
        public JsonParseException(int position, int errorType, object unexpectedObject)
            : base()
        {
            Position = position;
            ErrorType = errorType;
            UnexpectedObject = unexpectedObject;
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();

            switch (ErrorType)
            {
                case ErrorUnexpectedChar:
                    sb.Append("Unexpected character (").Append(UnexpectedObject).Append(") at position ").Append(Position).Append('.');
                    break;
                case ErrorUnexpectedToken:
                    sb.Append("Unexpected token ").Append(UnexpectedObject).Append(" at position ").Append(Position).Append('.');
                    break;
                case ErrorUnexpectedException:
                    sb.Append("Unexpected exception at position ").Append(Position).Append(": ").Append(UnexpectedObject);
                    break;
                default:
                    sb.Append("Unkown error at position ").Append(Position).Append('.');
                    break;
            }
            return sb.ToString();
        }
    }

    /// <summary>
    /// Lexer tokens.
    /// </summary>
    internal class Yytoken
    {
        public const int TypeValue = 0;
        public const int TypeLeftBrace = 1;
        public const int TypeRightBrace = 2;
        public const int TypeLeftSquare = 3;
        public const int TypeRightSquare = 4;
        public const int TypeComma = 5;
        public const int TypeColon = 6;
        public const int TypeEof = -1;

        public int Type { get; private set; }
        public object Value { get; private set; }

        public Yytoken(int type, object value)
        {
            Type = type;
            Value = value;
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            switch (Type)
            {
                case TypeValue:
                    sb.Append("Value(").Append(Value).Append(')');
                    break;
                case TypeLeftBrace:
                    sb.Append("LEFT BRACE({)");
                    break;
                case TypeRightBrace:
                    sb.Append("RIGHT BRACE(})");
                    break;
                case TypeLeftSquare:
                    sb.Append("LEFT SQUARE([)");
                    break;
                case TypeRightSquare:
                    sb.Append("RIGHT SQUARE(])");
                    break;
                case TypeComma:
                    sb.Append("COMMA(,)");
                    break;
                case TypeColon:
                    sb.Append("COLON(:)");
                    break;
                case TypeEof:
                    sb.Append("END OF FILE");
                    break;
                default:
                    break;
            }
            return sb.ToString();
        }
    }

    /// <summary>
    /// Lexer.
    /// </summary>
    internal class Yylex
    {
        public const int YyEof = -1;
        public const int YyInitial = 0;
        public const int StringBegin = 2;

        private const int ZzBufferSize = 16384;

        private static readonly int[] ZzLexstate = { 0, 0, 1, 1 };

        private const string ZZ_CMAP_PACKED = "\u0009\u0000\u0001\u0007\u0001\u0007\u0002\u0000\u0001\u0007\u0012\u0000\u0001" +
            "\u0007\u0001\u0000\u0001\u0009\u0008\u0000\u0001\u0006\u0001\u0019\u0001\u0002\u0001\u0004\u0001\u000a\u000a\u0003\u0001" +
            "\u001a\u0006\u0000\u0004\u0001\u0001\u0005\u0001\u0001\u0014\u0000\u0001\u0017\u0001\u0008\u0001\u0018\u0003\u0000\u0001" +
            "\u0012\u0001\u000b\u0002\u0001\u0001\u0011\u0001\u000c\u0005\u0000\u0001\u0013\u0001\u0000\u0001\u000d\u0003\u0000\u0001" +
            "\u000e\u0001\u0014\u0001\u000f\u0001\u0010\u0005\u0000\u0001\u0015\u0001\u0000\u0001\u0016\uff82\u0000";
        private static readonly char[] ZZ_CMAP = ZzUnpackCMap(ZZ_CMAP_PACKED);

        private const string ZZ_ACTION_PACKED_0 = "\u0002\u0000\u0002\u0001\u0001\u0002\u0001\u0003\u0001\u0004\u0003\u0001" +
            "\u0001\u0005\u0001\u0006\u0001\u0007\u0001\u0008\u0001\u0009\u0001\u000a\u0001\u000b\u0001\u000c\u0001\u000d\u0005\u0000" +
            "\u0001\u000c\u0001\u000e\u0001\u000f\u0001\u0010\u0001\u0011\u0001\u0012\u0001\u0013\u0001\u0014\u0001\u0000\u0001\u0015" +
            "\u0001\u0000\u0001\u0015\u0004\u0000\u0001\u0016\u0001\u0017\u0002\u0000\u0001\u0018";
        private static readonly int[] ZZ_ACTION = ZzUnpackAction();

        private static int[] ZzUnpackAction()
        {
            int[] result = new int[45];
            ZzUnpackAction(ZZ_ACTION_PACKED_0, 0, result);
            return result;
        }

        private static int ZzUnpackAction(string packed, int offset, int[] result)
        {
            int i = 0;
            int j = offset;
            int l = packed.Length;
            while (i < l)
            {
                int count = packed[i++];
                int value = packed[i++];
                do
                {
                    result[j++] = value;
                } while (--count > 0);
            }
            return j;
        }

        private const string ZZ_ROWMAP_PACKED_0 = "\u0000\u0000\u0000\u001b\u0000\u0036\u0000\u0051\u0000\u006c\u0000\u0087" +
            "\u0000\u0036\u0000\u00a2\u0000\u00bd\u0000\u00d8\u0000\u0036\u0000\u0036\u0000\u0036\u0000\u0036\u0000\u0036\u0000\u0036" +
            "\u0000\u00f3\u0000\u010e\u0000\u0036\u0000\u0129\u0000\u0144\u0000\u015f\u0000\u017a\u0000\u0195\u0000\u0036\u0000\u0036" +
            "\u0000\u0036\u0000\u0036\u0000\u0036\u0000\u0036\u0000\u0036\u0000\u0036\u0000\u01b0\u0000\u01cb\u0000\u01e6\u0000\u01e6" +
            "\u0000\u0201\u0000\u021c\u0000\u0237\u0000\u0252\u0000\u0036\u0000\u0036\u0000\u026d\u0000\u0288\u0000\u0036";
        private static readonly int[] ZZ_ROWMAP = ZzUnpackRowMap();

        private static int[] ZzUnpackRowMap()
        {
            int[] result = new int[45];
            ZzUnpackRowMap(ZZ_ROWMAP_PACKED_0, 0, result);
            return result;
        }

        private static int ZzUnpackRowMap(string packed, int offset, int[] result)
        {
            int i = 0;
            int j = offset;
            int l = packed.Length;
            while (i < l)
            {
                int high = packed[i++] << 16;
                result[j++] = high | packed[i++];
            }
            return j;
        }

        private static readonly int[] ZZ_TRANS = { 2, 2, 3, 4, 2, 2, 2, 5, 2, 6, 2, 2, 7, 8, 2, 9, 2, 2, 2, 2, 2, 10, 11, 12,
            13, 14, 15, 16, 16, 16, 16, 16, 16, 16, 16, 17, 18, 16, 16, 16, 16, 16, 16, 16, 16, 16, 16, 16, 16, 16, 16,
            16, 16, 16, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
            -1, -1, -1, -1, -1, -1, 4, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
            -1, -1, -1, -1, -1, -1, 4, 19, 20, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 20, -1, -1, -1, -1, -1, -1,
            -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 5, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
            -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 21, -1, -1, -1, -1, -1,
            -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 22, -1, -1, -1, -1, -1, -1, -1,
            -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 23, -1, -1, -1, -1, -1, -1, -1, -1, -1,
            -1, -1, -1, 16, 16, 16, 16, 16, 16, 16, 16, -1, -1, 16, 16, 16, 16, 16, 16, 16, 16, 16, 16, 16, 16, 16, 16,
            16, 16, 16, -1, -1, -1, -1, -1, -1, -1, -1, 24, 25, 26, 27, 28, 29, 30, 31, 32, -1, -1, -1, -1, -1, -1, -1,
            -1, -1, -1, -1, -1, -1, 33, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
            -1, -1, -1, -1, -1, 34, 35, -1, -1, 34, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
            -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 36, -1, -1, -1, -1,
            -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 37, -1, -1, -1, -1,
            -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 38, -1, -1, -1, -1, -1, -1, -1,
            -1, -1, -1, -1, 39, -1, 39, -1, 39, -1, -1, -1, -1, -1, 39, 39, -1, -1, -1, -1, 39, 39, -1, -1, -1, -1, -1,
            -1, -1, -1, -1, -1, -1, 33, -1, 20, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 20, -1, -1, -1, -1, -1, -1,
            -1, -1, -1, -1, -1, -1, 35, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
            -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 38, -1, -1, -1,
            -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 40, -1, -1, -1, -1,
            -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 41, -1, -1, -1, -1, -1, -1,
            -1, -1, -1, -1, 42, -1, 42, -1, 42, -1, -1, -1, -1, -1, 42, 42, -1, -1, -1, -1, 42, 42, -1, -1, -1, -1, -1,
            -1, -1, -1, -1, 43, -1, 43, -1, 43, -1, -1, -1, -1, -1, 43, 43, -1, -1, -1, -1, 43, 43, -1, -1, -1, -1, -1,
            -1, -1, -1, -1, 44, -1, 44, -1, 44, -1, -1, -1, -1, -1, 44, 44, -1, -1, -1, -1, 44, 44, -1, -1, -1, -1, -1,
            -1, -1, -1, };

        // error codes
        private const int ZzUnknownError = 0;
        private const int ZzNoMatch = 1;
        private const int ZzPushbackTooBig = 2;

        // error messages for the codes above
        private static readonly string[] ZZ_ERROR_MSG = { 
                                              "Unkown internal scanner error",
                                              "Error: could not match input",
                                              "Error: pushback value was too large"
                                          };

        private static readonly int[] ZZ_ATTRIBUTE = ZzUnpackAttribute();
        private const string ZZ_ATTRIBUTE_PACKED_0 = "\u0002\u0000\u0001\u0009\u0003\u0001\u0001\u0009\u0003\u0001\u0006" +
            "\u0009\u0002\u0001\u0001\u0009\u0005\u0000\u0008\u0009\u0001\u0000\u0001\u0001\u0001\u0000\u0001\u0001\u0004" +
            "\u0000\u0002\u0009\u0002\u0000\u0001\u0009";

        private static int[] ZzUnpackAttribute()
        {
            int[] result = new int[45];
            ZzUnpackAttribute(ZZ_ATTRIBUTE_PACKED_0, 0, result);
            return result;
        }

        private static int ZzUnpackAttribute(string packed, int offset, int[] result)
        {
            int i = 0; /* index in packed string */
            int j = offset; /* index in unpacked array */
            int l = packed.Length;
            while (i < l)
            {
                int count = packed[i++];
                int value = packed[i++];
                do
                {
                    result[j++] = value;
                } while (--count > 0);
            }
            return j;
        }

        /** the input device */
        private TextReader zzReader;

        /** the current state of the DFA */
        private int zzState;

        /** the current lexical state */
        private int zzLexicalState = YyInitial;

        /**
         * this buffer contains the current text to be matched and is the source of
         * the yytext() string
         */
        private char[] zzBuffer = new char[ZzBufferSize];

        /** the textposition at the last accepting state */
        private int zzMarkedPos;

        /** the current text position in the buffer */
        private int zzCurrentPos;

        /** startRead marks the beginning of the yytext() string in the buffer */
        private int zzStartRead;

        /**
         * endRead marks the last character in the buffer, that has been read from
         * input
         */
        private int zzEndRead;

        /** the number of characters up to the start of the matched text */
        private int yychar;

        /** zzAtEOF == true <=> the scanner is at the EOF */
        private bool zzAtEOF;

        /* user code: */
        private StringBuilder sb = new StringBuilder();

        public Yylex(TextReader input)
        {
            zzReader = input;
        }

        public int Position { get { return yychar; } }

        private static char[] ZzUnpackCMap(string packed)
        {
            char[] map = new char[0x10000];
            int i = 0; /* index in packed string */
            int j = 0; /* index in unpacked array */
            while (i < 90)
            {
                int count = packed[i++];
                char value = packed[i++];
                do
                {
                    map[j++] = value;
                } while (--count > 0);
            }
            return map;
        }

        private bool ZzRefill()
        {

            /* first: make room (if you can) */
            if (zzStartRead > 0)
            {
                Array.Copy(zzBuffer, zzStartRead, zzBuffer, 0, zzEndRead - zzStartRead);

                /* translate stored positions */
                zzEndRead -= zzStartRead;
                zzCurrentPos -= zzStartRead;
                zzMarkedPos -= zzStartRead;
                zzStartRead = 0;
            }

            // is the buffer big enough?
            if (zzCurrentPos >= zzBuffer.Length)
            {
                // if not: blow it up
                Array.Resize<char>(ref zzBuffer, zzCurrentPos * 2);
            }

            // finally: fill the buffer with new input
            int numRead = zzReader.Read(zzBuffer, zzEndRead, zzBuffer.Length - zzEndRead);

            if (numRead > 0)
            {
                zzEndRead += numRead;
                return false;
            }

            // numRead <= 0
            return true;
        }

        public void Yyclose()
        {
            zzAtEOF = true;
            zzEndRead = zzStartRead;

            if (zzReader != null)
            {
                zzReader.Dispose();
            }
        }

        public void YyReset(TextReader reader)
        {
            zzReader = reader;
            zzAtEOF = false;
            zzEndRead = 0;
            zzStartRead = 0;
            zzCurrentPos = 0;
            zzMarkedPos = 0;
            yychar = 0;
            zzLexicalState = YyInitial;
        }

        public int YyState()
        {
            return zzLexicalState;
        }

        public void YyBegin(int newState)
        {
            zzLexicalState = newState;
        }

        public string YyText()
        {
            return new string(zzBuffer, zzStartRead, zzMarkedPos - zzStartRead);
        }

        public char YyCharat(int pos)
        {
            return zzBuffer[zzStartRead + pos];
        }

        public int YyLength()
        {
            return zzMarkedPos - zzStartRead;
        }

        private void ZzScanError(int errorCode)
        {
            string message = null;
            try
            {
                message = ZZ_ERROR_MSG[errorCode];
            }
            catch (IndexOutOfRangeException)
            {
                message = ZZ_ERROR_MSG[ZzUnknownError];
            }

            throw new Exception(message);
        }

        public void YyPushback(int number)
        {
            if (number > YyLength())
            {
                ZzScanError(ZzPushbackTooBig);
            }

            zzMarkedPos -= number;
        }

        public Yytoken YyLex()
        {
            int zzInput;
            int zzAction;

            // cached fields:
            int zzCurrentPosL;
            int zzMarkedPosL;
            int zzEndReadL = zzEndRead;
            char[] zzBufferL = zzBuffer;
            char[] zzCMapL = ZZ_CMAP;

            int[] zzTransL = ZZ_TRANS;
            int[] zzRowMapL = ZZ_ROWMAP;
            int[] zzAttrL = ZZ_ATTRIBUTE;

            while (true)
            {
                zzMarkedPosL = zzMarkedPos;

                yychar += zzMarkedPosL - zzStartRead;

                zzAction = -1;

                zzCurrentPosL = zzMarkedPosL;
                zzCurrentPos = zzMarkedPosL;
                zzStartRead = zzMarkedPosL;

                zzState = ZzLexstate[zzLexicalState];

                while (true)
                {
                    if (zzCurrentPosL < zzEndReadL)
                    {
                        zzInput = zzBufferL[zzCurrentPosL++];
                    }
                    else if (zzAtEOF)
                    {
                        zzInput = YyEof;
                        break;
                    }
                    else
                    {
                        // store back cached positions
                        zzCurrentPos = zzCurrentPosL;
                        zzMarkedPos = zzMarkedPosL;
                        bool eof = ZzRefill();
                        // get translated positions and possibly new buffer
                        zzCurrentPosL = zzCurrentPos;
                        zzMarkedPosL = zzMarkedPos;
                        zzBufferL = zzBuffer;
                        zzEndReadL = zzEndRead;
                        if (eof)
                        {
                            zzInput = YyEof;
                            break;
                        }
                        else
                        {
                            zzInput = zzBufferL[zzCurrentPosL++];
                        }
                    }
                    int zzNext = zzTransL[zzRowMapL[zzState] + zzCMapL[zzInput]];
                    if (zzNext == -1)
                    {
                        break;
                    }
                    zzState = zzNext;

                    int zzAttributes = zzAttrL[zzState];
                    if ((zzAttributes & 1) == 1)
                    {
                        zzAction = zzState;
                        zzMarkedPosL = zzCurrentPosL;
                        if ((zzAttributes & 8) == 8)
                        {
                            break;
                        }
                    }
                }

                // store back cached position
                zzMarkedPos = zzMarkedPosL;

                switch (zzAction < 0 ? zzAction : ZZ_ACTION[zzAction])
                {
                    case 11:
                        {
                            sb.Append(YyText());
                            break;
                        }
                    case 25:
                        break;
                    case 4:
                        {
                            sb.Clear();
                            YyBegin(StringBegin);
                            break;
                        }
                    case 26:
                        break;
                    case 16:
                        {
                            sb.Append('\b');
                            break;
                        }
                    case 27:
                        break;
                    case 6:
                        {
                            return new Yytoken(Yytoken.TypeRightBrace, null);
                        }
                    case 28:
                        break;
                    case 23:
                        {
                            Boolean val;
                            if (!Boolean.TryParse(YyText(), out val))
                            {
                                throw new FormatException("Boolean value cannot be parsed!");
                            }
                            return new Yytoken(Yytoken.TypeValue, val);
                        }
                    case 29:
                        break;
                    case 22:
                        {
                            return new Yytoken(Yytoken.TypeValue, null);
                        }
                    case 30:
                        break;
                    case 13:
                        {
                            YyBegin(YyInitial);
                            return new Yytoken(Yytoken.TypeValue, sb.ToString());
                        }
                    case 31:
                        break;
                    case 12:
                        {
                            sb.Append('\\');
                            break;
                        }
                    case 32:
                        break;
                    case 21:
                        {
                            decimal val;
                            if (!Decimal.TryParse(YyText(), NumberStyles.Float, CultureInfo.InvariantCulture, out val))
                            {
                                throw new FormatException("Decimal value cannot be parsed!");
                            }
                            return new Yytoken(Yytoken.TypeValue, val);
                        }
                    case 33:
                        break;
                    case 1:
                        {
                            throw new JsonParseException(yychar, JsonParseException.ErrorUnexpectedChar, YyCharat(0));
                        }
                    case 34:
                        break;
                    case 8:
                        {
                            return new Yytoken(Yytoken.TypeRightSquare, null);
                        }
                    case 35:
                        break;
                    case 19:
                        {
                            sb.Append('\r');
                            break;
                        }
                    case 36:
                        break;
                    case 15:
                        {
                            sb.Append('/');
                            break;
                        }
                    case 37:
                        break;
                    case 10:
                        {
                            return new Yytoken(Yytoken.TypeColon, null);
                        }
                    case 38:
                        break;
                    case 14:
                        {
                            sb.Append('"');
                            break;
                        }
                    case 39:
                        break;
                    case 5:
                        {
                            return new Yytoken(Yytoken.TypeLeftBrace, null);
                        }
                    case 40:
                        break;
                    case 17:
                        {
                            sb.Append('\f');
                            break;
                        }
                    case 41:
                        break;
                    case 24:
                        {
                            try
                            {
                                ushort ch = Convert.ToUInt16(YyText().Substring(2), 16);
                                sb.Append((char)ch);
                            }
                            catch (Exception e)
                            {
                                throw new JsonParseException(yychar, JsonParseException.ErrorUnexpectedException, e);
                            }
                            break;
                        }
                    case 42:
                        break;
                    case 20:
                        {
                            sb.Append('\t');
                            break;
                        }
                    case 43:
                        break;
                    case 7:
                        return new Yytoken(Yytoken.TypeLeftSquare, null);
                    case 44:
                        break;
                    case 2:
                        {
                            BigInteger val = BigInteger.Parse(YyText(), NumberStyles.Integer, CultureInfo.InvariantCulture);
                            return new Yytoken(Yytoken.TypeValue, val);
                        }
                    case 45:
                        break;
                    case 18:
                        {
                            sb.Append('\n');
                            break;
                        }
                    case 46:
                        break;
                    case 9:
                        return new Yytoken(Yytoken.TypeComma, null);
                    case 47:
                        break;
                    case 3:
                        break;
                    case 48:
                        break;
                    default:
                        if (zzInput == YyEof && zzStartRead == zzCurrentPos)
                        {
                            zzAtEOF = true;
                            return null;
                        }
                        else
                        {
                            ZzScanError(ZzNoMatch);
                        }
                        break;
                }
            }
        }
    }
}
