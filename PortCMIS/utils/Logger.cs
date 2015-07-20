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
using System.IO;
using System.Text;

namespace PortCMIS.Utils
{
    /// <summary>
    /// Simplistic logginh.
    /// </summary>
    public class Logger
    {

        public enum LogLevel
        {
            Error, Warn, Info, Debug, Trace
        }

        public static LogLevel Level { get; set; }
        public static bool ToDebug { get; set; }
        public static bool ToWriter { get; set; }
        public static TextWriter Writer { private get; set; }

        public static bool IsErrorEnabled { get { return true; } }
        public static bool IsWarnEnabled { get { return Level == LogLevel.Warn || Level == LogLevel.Info || Level == LogLevel.Debug || Level == LogLevel.Trace; } }
        public static bool IsInfoEnabled { get { return Level == LogLevel.Info || Level == LogLevel.Debug || Level == LogLevel.Trace; } }
        public static bool IsDebugEnabled { get { return Level == LogLevel.Debug || Level == LogLevel.Trace; } }
        public static bool IsTraceEnabled { get { return Level == LogLevel.Trace; } }

        static Logger()
        {
            Level = LogLevel.Info;
#if DEBUG
            ToDebug = true;
#else
            ToDebug = false;
#endif
            ToWriter = false;
        }

        private static object loggerLock = new object();

        public static void Error(string message, Exception e = null)
        {
            lock (loggerLock)
            {
                Write("ERROR", message, e);
            }
        }

        public static void Warn(string message, Exception e = null)
        {
            lock (loggerLock)
            {
                if (IsWarnEnabled)
                {
                    Write("WARN", message, e);
                }
            }
        }

        public static void Info(string message, Exception e = null)
        {
            lock (loggerLock)
            {
                if (IsInfoEnabled)
                {
                    Write("INFO", message, e);
                }
            }
        }

        public static void Debug(string message, Exception e = null)
        {
            lock (loggerLock)
            {
                if (IsDebugEnabled)
                {
                    Write("DEBUG", message, e);
                }
            }
        }

        public static void Trace(string message, Exception e = null)
        {
            lock (loggerLock)
            {
                if (IsTraceEnabled)
                {
                    Write("Trace", message, e);
                }
            }
        }

        private static void Write(string levelname, string message, Exception e)
        {
            if (!ToDebug && !ToWriter)
            {
                return;
            }

            DateTime now = DateTime.Now;

            StringBuilder sb = new StringBuilder();
            sb.Append(now.ToString("o"));
            sb.Append(' ');
            sb.Append(levelname);
            sb.Append(": ");
            sb.Append(message);
            if (e != null)
            {
                sb.AppendLine();
                sb.AppendLine("   ");
                sb.AppendLine(e.ToString());
                sb.AppendLine("   ");
                sb.AppendLine(e.StackTrace);
            }

            if (ToDebug)
            {
                System.Diagnostics.Debug.WriteLine(sb.ToString());
            }

            if (ToWriter && Writer != null)
            {
                Writer.WriteLine(sb.ToString());
                Writer.Flush();
            }
        }

    }
}
