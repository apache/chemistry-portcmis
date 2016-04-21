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

using PortCMIS.Client;
using PortCMIS.Exceptions;
using PortCMIS.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace PortCMIS.Binding.Http
{
    public interface IHttpInvoker
    {
        IResponse InvokeGET(UrlBuilder url, IBindingSession session);
        IResponse InvokeGET(UrlBuilder url, IBindingSession session, long? offset, long? length);
        IResponse InvokePOST(UrlBuilder url, HttpContent content, IBindingSession session);
        IResponse InvokePUT(UrlBuilder url, IDictionary<string, string> headers, HttpContent content, IBindingSession session);
        IResponse InvokeDELETE(UrlBuilder url, IBindingSession session);
    }

    public interface IResponse
    {
        int StatusCode { get; }
        string Message { get; }
        Stream Stream { get; }
        string ErrorContent { get; }
        string ContentType { get; }
        string Charset { get; }
        long? ContentLength { get; }
        string Filename { get; }
    }
}
