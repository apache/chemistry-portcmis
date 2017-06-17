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

using System.Collections.Generic;
using System.IO;
using System.Net.Http;

namespace PortCMIS.Binding.Http
{
    /// <summary>
    /// Implementations of this interface are responsible to handle HTTP requests.
    /// </summary>
    public interface IHttpInvoker
    {
        /// <summary>
        /// Invokes HTTP GET.
        /// </summary>
        /// <param name="url">the URL</param>
        /// <param name="session">the binding session</param>
        /// <returns>the HTTP response</returns>
        IResponse InvokeGET(UrlBuilder url, IBindingSession session);

        /// <summary>
        /// Invokes HTTP GET to get document content.
        /// </summary>
        /// <param name="url">the URL</param>
        /// <param name="session">the binding session</param>
        /// <param name="offset">content offset (<c>null</c> = start from the beginning)</param>
        /// <param name="length">max content length (<c>null</c> = read to the end)</param>
        /// <returns>the HTTP response</returns>
        IResponse InvokeGET(UrlBuilder url, IBindingSession session, long? offset, long? length);

        /// <summary>
        /// Invokes HTTP POST.
        /// </summary>
        /// <param name="url">the URL</param>
        /// <param name="content">the content to send</param>
        /// <param name="session">the binding session</param>
        /// <returns>the HTTP response</returns>
        IResponse InvokePOST(UrlBuilder url, HttpContent content, IBindingSession session);

        /// <summary>
        /// Invokes HTTP PUT.
        /// </summary>
        /// <param name="url">the URL</param>
        /// <param name="headers">additional headers, may be <c>null</c></param>
        /// <param name="content">the content to send</param>
        /// <param name="session">the binding session</param>
        /// <returns>the HTTP response</returns>
        IResponse InvokePUT(UrlBuilder url, IDictionary<string, string> headers, HttpContent content, IBindingSession session);

        /// <summary>
        /// Invokes HTTP DELETE.
        /// </summary>
        /// <param name="url">the URL</param>
        /// <param name="session">the binding session</param>
        /// <returns>the HTTP response</returns>
        IResponse InvokeDELETE(UrlBuilder url, IBindingSession session);
    }

    /// <summary>
    /// Response of a HTTP request.
    /// </summary>
    public interface IResponse
    {
        /// <value>HTTP status code</value>
        int StatusCode { get; }

        /// <value>HTTP status message</value>
        string Message { get; }

        /// <value>response stream</value>
        Stream Stream { get; }

        /// <value>the content in case of an error</value>
        string ErrorContent { get; }

        /// <value>the content type</value>
        string ContentType { get; }

        /// <value>the content charset</value>
        string Charset { get; }

        /// <value>the content length, if known</value>
        long? ContentLength { get; }

        /// <value>the content filename, if known</value>
        string Filename { get; }
    }
}
