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

namespace PortCMIS.Binding
{
    internal static class DateTimeHelper
    {
        private static readonly DateTime Jan1st1970 = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        public static DateTime ConvertMillisToDateTime(long millis)
        {
            return new DateTime(Jan1st1970.Ticks + millis * TimeSpan.TicksPerMillisecond, DateTimeKind.Utc);
        }

        public static long ConvertDateTimeToMillis(DateTime datetime)
        {
            return (datetime.Ticks - Jan1st1970.Ticks) / TimeSpan.TicksPerMillisecond;
        }

        public static DateTime ParseISO8601(string s)
        {
            return DateTime.Parse(s, null, DateTimeStyles.RoundtripKind);
        }

        public static string FormatISO8601(DateTime dt)
        {
            return dt.ToString("o");
        }
    }
}
