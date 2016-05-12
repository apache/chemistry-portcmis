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

namespace PortCMIS.Client
{
    /// <summary>
    /// Default values for some session parameters.
    /// </summary>
    public static class SessionParameterDefaults
    {
        /// <summary>Default repository cache size</summary>
        public const int CacheSizeRepositories = 10;

        /// <summary>Default type cache size</summary>
        public const int CacheSizeTypes = 100;

        /// <summary>Default link cache size</summary>
        public const int CacheSizeLinks = 400;

        /// <summary>Default object cache size</summary>
        public const int CacheSizeObjects = 100;

        /// <summary>Default time-to-live for objects in the object cache</summary>
        public const int CacheTTLObjects = 2 * 60 * 60 * 1000;

        /// <summary>Default path cache size</summary>
        public const int CacheSizePathToId = 100;

        /// <summary>Default time-to-live for paths in the object cache</summary>
        public const int CacheTTLPathToId = 30 * 60 * 1000;
    }
}
