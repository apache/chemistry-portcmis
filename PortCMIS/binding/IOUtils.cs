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

namespace PortCMIS.Binding
{
    class IOUtils
    {
        public static void ConsumeAndClose(TextReader reader)
        {
            if (reader == null)
            {
                return;
            }

            try
            {
                char[] buffer = new char[64 * 1024];
                while (reader.Read(buffer, 0, buffer.Length) > 0)
                {
                    // just consume
                }
            }
            catch (IOException)
            {
                // ignore
            }
            catch (ObjectDisposedException)
            {
                // ignore
            }
            finally
            {
                reader.Dispose();
            }
        }

        public static void ConsumeAndClose(Stream stream)
        {
            if (stream == null)
            {
                return;
            }

            try
            {
                byte[] buffer = new byte[64 * 1024];
                while (stream.Read(buffer, 0, buffer.Length) > 0)
                {
                    // just consume
                }
            }
            catch (IOException)
            {
                // ignore
            }
            catch (ObjectDisposedException)
            {
                // ignore
            }
            finally
            {
                stream.Dispose();
            }
        }
    }
}
