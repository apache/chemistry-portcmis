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

using PortCMIS.Utils;
using System;
using System.Collections.Generic;

namespace PortCMIS.Client
{
    /// <summary>
    /// Client object cache interface.
    /// </summary>
    public interface ICache
    {
        /// <summary>
        /// Initializes the cache.
        /// </summary>
        /// <param name="session">the session</param>
        /// <param name="parameters">cache parameters</param>
        void Initialize(ISession session, IDictionary<string, string> parameters);

        /// <summary>
        /// Returns whether the cache contains an object with given object ID and cache key.
        /// </summary>
        /// <param name="objectId">the object ID</param>
        /// <param name="cacheKey">the cache key</param>
        /// <returns><c>true</c> if the object is in the cache, <c>false</c> otherwise</returns>
        bool ContainsId(string objectId, string cacheKey);

        /// <summary>
        /// Returns whether the cache contains an object with given path and cache key.
        /// </summary>
        /// <param name="path">the path</param>
        /// <param name="cacheKey">the cache key</param>
        /// <returns><c>true</c> if the object is in the cache, <c>false</c> otherwise</returns>
        bool ContainsPath(string path, string cacheKey);

        /// <summary>
        /// Puts an object into the cache.
        /// </summary>
        /// <param name="cmisObject">the object</param>
        /// <param name="cacheKey">the cache key</param>
        void Put(ICmisObject cmisObject, string cacheKey);

        /// <summary>
        /// Puts an object with a path into the cache.
        /// </summary>
        /// <param name="path">the path</param>
        /// <param name="cmisObject">the object</param>
        /// <param name="cacheKey">the cache key</param>
        void PutPath(string path, ICmisObject cmisObject, string cacheKey);

        /// <summary>
        /// Gets an object by ID.
        /// </summary>
        /// <param name="objectId">the object ID</param>
        /// <param name="cacheKey">the cache key</param>
        /// <returns>the object or <c>null</c> if the object is not in the cache</returns>
        ICmisObject GetById(string objectId, string cacheKey);

        /// <summary>
        /// Gets an object by path.
        /// </summary>
        /// <param name="path">the path</param>
        /// <param name="cacheKey">the cache key</param>
        /// <returns>the object or <c>null</c> if the object is not in the cache</returns>
        ICmisObject GetByPath(string path, string cacheKey);

        /// <summary>
        /// Removes an object from the cache.
        /// </summary>
        /// <param name="objectId">the object ID</param>
        void Remove(string objectId);

        /// <summary>
        /// Clears the cache.
        /// </summary>
        void Clear();

        /// <value>
        /// The number of objects in the cache.
        /// </value>
        int CacheSize { get; }
    }

    /// <summary>
    /// Cache implementation that doesn't cache anything.
    /// </summary>
    public class NoCache : ICache
    {
        /// <inheritdoc/> 
        public void Initialize(ISession session, IDictionary<string, string> parameters) { }

        /// <inheritdoc/> 
        public bool ContainsId(string objectId, string cacheKey) { return false; }

        /// <inheritdoc/> 
        public bool ContainsPath(string path, string cacheKey) { return false; }

        /// <inheritdoc/> 
        public void Put(ICmisObject cmisObject, string cacheKey) { }

        /// <inheritdoc/> 
        public void PutPath(string path, ICmisObject cmisObject, string cacheKey) { }

        /// <inheritdoc/> 
        public ICmisObject GetById(string objectId, string cacheKey) { return null; }

        /// <inheritdoc/> 
        public ICmisObject GetByPath(string path, string cacheKey) { return null; }

        /// <inheritdoc/> 
        public void Remove(string objectId) { }

        /// <inheritdoc/> 
        public void Clear() { }

        /// <inheritdoc/> 
        public int CacheSize { get { return 0; } }
    }

    public class CmisObjectCache : ICache
    {
        private int cacheSize;
        private int cacheTtl;
        private int pathToIdSize;
        private int pathToIdTtl;

        private LRUCache<string, IDictionary<string, ICmisObject>> objectCache;
        private LRUCache<string, string> pathToIdCache;

        private object cacheLock = new object();

        public CmisObjectCache() { }

        /// <inheritdoc/> 
        public void Initialize(ISession session, IDictionary<string, string> parameters)
        {
            lock (cacheLock)
            {
                // cache size
                cacheSize = 1000;
                try
                {
                    string cacheSizeStr;
                    if (parameters.TryGetValue(SessionParameter.CacheSizeObjects, out cacheSizeStr))
                    {
                        cacheSize = Int32.Parse(cacheSizeStr);
                        if (cacheSize < 0)
                        {
                            cacheSize = 0;
                        }
                    }
                }
                catch (Exception) { }

                // cache time-to-live
                cacheTtl = 2 * 60 * 60 * 1000;
                try
                {
                    string cacheTtlStr;
                    if (parameters.TryGetValue(SessionParameter.CacheTTLObjects, out cacheTtlStr))
                    {
                        cacheTtl = Int32.Parse(cacheTtlStr);
                        if (cacheTtl < 0)
                        {
                            cacheTtl = 2 * 60 * 60 * 1000;
                        }
                    }
                }
                catch (Exception) { }

                // path-to-id size
                pathToIdSize = 1000;
                try
                {
                    string pathToIdSizeStr;
                    if (parameters.TryGetValue(SessionParameter.CacheSizePathToId, out pathToIdSizeStr))
                    {
                        pathToIdSize = Int32.Parse(pathToIdSizeStr);
                        if (pathToIdSize < 0)
                        {
                            pathToIdSize = 0;
                        }
                    }
                }
                catch (Exception) { }

                // path-to-id time-to-live
                pathToIdTtl = 30 * 60 * 1000;
                try
                {
                    string pathToIdTtlStr;
                    if (parameters.TryGetValue(SessionParameter.CacheTTLPathToId, out pathToIdTtlStr))
                    {
                        pathToIdTtl = Int32.Parse(pathToIdTtlStr);
                        if (pathToIdTtl < 0)
                        {
                            pathToIdTtl = 30 * 60 * 1000;
                        }
                    }
                }
                catch (Exception) { }

                InitializeInternals();
            }
        }

        private void InitializeInternals()
        {
            lock (cacheLock)
            {
                objectCache = new LRUCache<string, IDictionary<string, ICmisObject>>(cacheSize, TimeSpan.FromMilliseconds(cacheTtl));
                pathToIdCache = new LRUCache<string, string>(pathToIdSize, TimeSpan.FromMilliseconds(pathToIdTtl));
            }
        }

        /// <inheritdoc/> 
        public void Clear()
        {
            InitializeInternals();
        }

        /// <inheritdoc/> 
        public bool ContainsId(string objectId, string cacheKey)
        {
            lock (cacheLock)
            {
                return objectCache.Get(objectId) != null;
            }
        }

        /// <inheritdoc/> 
        public bool ContainsPath(string path, string cacheKey)
        {
            lock (cacheLock)
            {
                return pathToIdCache.Get(path) != null;
            }
        }

        /// <inheritdoc/> 
        public ICmisObject GetById(string objectId, string cacheKey)
        {
            lock (cacheLock)
            {
                IDictionary<string, ICmisObject> cacheKeyDict = objectCache.Get(objectId);
                if (cacheKeyDict == null)
                {
                    return null;
                }

                ICmisObject cmisObject;
                if (cacheKeyDict.TryGetValue(cacheKey, out cmisObject))
                {
                    return cmisObject;
                }

                return null;
            }
        }

        /// <inheritdoc/> 
        public ICmisObject GetByPath(string path, string cacheKey)
        {
            lock (cacheLock)
            {
                string id = pathToIdCache.Get(path);
                if (id == null)
                {
                    return null;
                }

                return GetById(id, cacheKey);
            }
        }

        /// <inheritdoc/> 
        public void Put(ICmisObject cmisObject, string cacheKey)
        {
            // no object, no id, no cache key - no cache
            if (cmisObject == null || cmisObject.Id == null || cacheKey == null)
            {
                return;
            }

            lock (cacheLock)
            {
                IDictionary<string, ICmisObject> cacheKeyDict = objectCache.Get(cmisObject.Id);
                if (cacheKeyDict == null)
                {
                    cacheKeyDict = new Dictionary<string, ICmisObject>();
                    objectCache.Add(cmisObject.Id, cacheKeyDict);
                }

                cacheKeyDict[cacheKey] = cmisObject;

                // folders may have a path, use it!
                string path = cmisObject.GetPropertyValue(PropertyIds.Path) as string;
                if (path != null)
                {
                    pathToIdCache.Add(path, cmisObject.Id);
                }
            }
        }

        /// <inheritdoc/> 
        public void PutPath(string path, ICmisObject cmisObject, string cacheKey)
        {
            // no path, no object, no id, no cache key - no cache
            if (path == null || cmisObject == null || cmisObject.Id == null || cacheKey == null)
            {
                return;
            }

            lock (cacheLock)
            {
                Put(cmisObject, cacheKey);
                pathToIdCache.Add(path, cmisObject.Id);
            }
        }

        /// <inheritdoc/> 
        public void Remove(string objectId)
        {
            if (objectId == null)
            {
                return;
            }

            lock (cacheLock)
            {
                objectCache.Remove(objectId);
            }
        }

        /// <inheritdoc/> 
        public int CacheSize
        {
            get { return cacheSize; }
        }
    }
}
