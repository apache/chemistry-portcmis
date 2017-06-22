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
using System.Globalization;
using System.IO;
using System.Text;

namespace PortCMIS.Utils
{
    /// <summary>
    /// Simplistic logging.
    /// </summary>
    public static class Logger
    {

        /// <summary>
        /// The log levels
        /// </summary>
        public enum LogLevel
        {
            /// <summary>
            /// Log level Error
            /// </summary>
            Error,
            /// <summary>
            /// Log level Warn
            /// </summary>
            Warn,
            /// <summary>
            /// Log level Info
            /// </summary>
            Info,
            /// <summary>
            /// Log level Debug
            /// </summary>
            Debug,
            /// <summary>
            /// Log level Trace
            /// </summary>
            Trace
        }

        /// <value>Gets or sets the log level</value>
        public static LogLevel Level { get; set; }
        /// <value>Gets or sets whether the log messages should go to System.Diagnostics.Debug</value>
        public static bool ToDebug { get; set; }
        /// <value>Gets or sets whether the log messages should go to a writer</value>
        public static bool ToWriter { get; set; }
        /// <value>Sets the log writer</value>
        public static TextWriter Writer { private get; set; }

        /// <value>Is the error log level enabled?</value>
        public static bool IsErrorEnabled { get { return true; } }
        /// <value>Is the warn log level enabled?</value>
        public static bool IsWarnEnabled { get { return Level == LogLevel.Warn || Level == LogLevel.Info || Level == LogLevel.Debug || Level == LogLevel.Trace; } }
        /// <value>Is the info log level enabled?</value>
        public static bool IsInfoEnabled { get { return Level == LogLevel.Info || Level == LogLevel.Debug || Level == LogLevel.Trace; } }
        /// <value>Is the debug log level enabled?</value>
        public static bool IsDebugEnabled { get { return Level == LogLevel.Debug || Level == LogLevel.Trace; } }
        /// <value>Is the trace log level enabled?</value>
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


        /// <summary>
        /// Logs an error message.
        /// </summary>
        /// <param name="message">the message</param>
        public static void Error(string message)
        {
            Error(message, null);
        }

        /// <summary>
        /// Logs an error message.
        /// </summary>
        /// <param name="message">the message</param>
        /// <param name="e">the exception</param>
        public static void Error(string message, Exception e)
        {
            lock (loggerLock)
            {
                Write("ERROR", message, e);
            }
        }

        /// <summary>
        /// Logs a warn message.
        /// </summary>
        /// <param name="message">the message</param>
        public static void Warn(string message)
        {
            Warn(message, null);
        }

        /// <summary>
        /// Logs a warn message.
        /// </summary>
        /// <param name="message">the message</param>
        /// <param name="e">the exception</param>
        public static void Warn(string message, Exception e)
        {
            lock (loggerLock)
            {
                if (IsWarnEnabled)
                {
                    Write("WARN", message, e);
                }
            }
        }

        /// <summary>
        /// Logs an info message.
        /// </summary>
        /// <param name="message">the message</param>
        public static void Info(string message)
        {
            Info(message, null);
        }

        /// <summary>
        /// Logs an info message.
        /// </summary>
        /// <param name="message">the message</param>
        /// <param name="e">the exception</param>
        public static void Info(string message, Exception e)
        {
            lock (loggerLock)
            {
                if (IsInfoEnabled)
                {
                    Write("INFO", message, e);
                }
            }
        }

        /// <summary>
        /// Logs a debug message.
        /// </summary>
        /// <param name="message">the message</param>
        public static void Debug(string message)
        {
            Debug(message, null);
        }

        /// <summary>
        /// Logs a debug message.
        /// </summary>
        /// <param name="message">the message</param>
        /// <param name="e">the exception</param>
        public static void Debug(string message, Exception e)
        {
            lock (loggerLock)
            {
                if (IsDebugEnabled)
                {
                    Write("DEBUG", message, e);
                }
            }
        }

        /// <summary>
        /// Logs a trace message.
        /// </summary>
        /// <param name="message">the message</param>
        public static void Trace(string message)
        {
            Trace(message, null);
        }

        /// <summary>
        /// Logs a trace message.
        /// </summary>
        /// <param name="message">the message</param>
        /// <param name="e">the exception</param>
        public static void Trace(string message, Exception e)
        {
            lock (loggerLock)
            {
                if (IsTraceEnabled)
                {
                    Write("TRACE", message, e);
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
            sb.Append(now.ToString("o", CultureInfo.InvariantCulture));
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
