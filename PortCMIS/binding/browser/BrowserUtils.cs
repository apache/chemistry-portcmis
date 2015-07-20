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

using PortCMIS.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PortCMIS.Binding.Browser
{
    internal class RepositoryUrlCache
    {
        private const String ObjectId = "objectId";
        private const String Selector = "cmisselector";

        private IDictionary<string, string> repositoryUrls;
        private IDictionary<string, string> rootUrls;

        private object repLock = new object();

        public RepositoryUrlCache()
        {
            repositoryUrls = new Dictionary<string, string>();
            rootUrls = new Dictionary<string, string>();
        }

        /// <summary>
        /// Adds the URLs of a repository to the cache.
        /// </summary>
        public void AddRepository(string repositoryId, string repositoryUrl, string rootUrl)
        {
            if (repositoryId == null || repositoryUrl == null || rootUrl == null)
            {
                throw new ArgumentException("Repository Id or Repository URL or Root URL is not set!");
            }

            lock (repLock)
            {
                repositoryUrls.Add(repositoryId, repositoryUrl);
                rootUrls.Add(repositoryId, rootUrl);
            }
        }

        /// <summary>
        /// Removes the URLs of a repository from the cache.
        /// </summary>
        public void RemoveRepository(string repositoryId)
        {
            lock (repLock)
            {
                repositoryUrls.Remove(repositoryId);
                rootUrls.Remove(repositoryId);
            }
        }

        /// <summary>
        /// Returns the base repository URL of a repository.
        /// </summary>
        public string GetRepositoryBaseUrl(string repositoryId)
        {
            lock (repLock)
            {
                string url;
                if (!repositoryUrls.TryGetValue(repositoryId, out url))
                {
                    url = null;
                }

                return url;
            }
        }

        /// <summary>
        /// Returns the root URL of a repository.
        /// </summary>
        public string GetRootUrl(string repositoryId)
        {
            lock (repLock)
            {
                string url;
                if (!rootUrls.TryGetValue(repositoryId, out url))
                {
                    url = null;
                }

                return url;
            }
        }

        /// <summary>
        /// Returns the repository URL.
        /// </summary>
        public UrlBuilder GetRepositoryUrl(string repositoryId)
        {
            string url = GetRepositoryBaseUrl(repositoryId);
            if (url == null)
            {
                return null;
            }

            return new UrlBuilder(url);
        }

        /// <summary>
        /// Returns the repository URL with the given selector.
        /// </summary>
        public UrlBuilder GetRepositoryUrl(string repositoryId, string selector)
        {
            UrlBuilder result = GetRepositoryUrl(repositoryId);
            if (result == null)
            {
                return null;
            }

            result.AddParameter(Selector, selector);

            return result;
        }

        /// <summary>
        /// Returns an object URL with the given selector.
        /// </summary>
        public UrlBuilder GetObjectUrl(string repositoryId, string objectId)
        {
            String url = GetRootUrl(repositoryId);
            if (url == null)
            {
                return null;
            }

            UrlBuilder result = new UrlBuilder(url);
            result.AddParameter(ObjectId, objectId);

            return result;
        }

        /// <summary>
        /// Returns an object URL with the given selector.
        /// </summary>
        public UrlBuilder GetObjectUrl(string repositoryId, string objectId, string selector)
        {
            UrlBuilder result = GetObjectUrl(repositoryId, objectId);
            if (result == null)
            {
                return null;
            }

            result.AddParameter(Selector, selector);

            return result;
        }

        /// <summary>
        /// Returns an object URL with the given selector.
        /// </summary>
        public UrlBuilder GetPathUrl(string repositoryId, string path)
        {
            String url = GetRootUrl(repositoryId);
            if (url == null)
            {
                return null;
            }

            UrlBuilder result = new UrlBuilder(url);
            result.AddPath(path);

            return result;
        }

        /// <summary>
        /// Returns an object URL with the given selector.
        /// </summary>
        public UrlBuilder GetPathUrl(string repositoryId, string path, string selector)
        {
            UrlBuilder result = GetPathUrl(repositoryId, path);
            if (result == null)
            {
                return null;
            }

            result.AddParameter(Selector, selector);

            return result;
        }
    }

    internal interface ITypeCache
    {
        ITypeDefinition GetTypeDefinition(string typeId);
        ITypeDefinition ReloadTypeDefinition(string typeId);
        ITypeDefinition GetTypeDefinitionForObject(string objectId);
        IPropertyDefinition GetPropertyDefinition(string propId);
    }
}
