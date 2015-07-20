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

using PortCMIS.Binding.Http;
using PortCMIS.Binding.Services;
using PortCMIS.Client;
using PortCMIS.Data;
using PortCMIS.Data.Extensions;
using PortCMIS.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace PortCMIS.Binding.Impl
{
    /// <summary>
    /// Binding layer implementation.
    /// </summary>
    internal class CmisBinding : ICmisBinding
    {
        private BindingSession session;
        private BindingRepositoryService repositoryServiceWrapper;

        private object bindingLock = new object();

        public CmisBinding(IDictionary<string, string> sessionParameters)
            : this(sessionParameters, null)
        {
        }

        public CmisBinding(IDictionary<string, string> sessionParameters, IAuthenticationProvider authenticationProvider)
        {
            if (sessionParameters == null)
            {
                throw new ArgumentNullException("sessionParameters");
            }

            if (!sessionParameters.ContainsKey(SessionParameter.BindingSpiClass))
            {
                throw new ArgumentException("Session parameters do not contain a SPI class name!");
            }

            // initialize session
            session = new BindingSession();
            foreach (KeyValuePair<string, string> kv in sessionParameters)
            {
                session.PutValue(kv.Key, kv.Value);
            }

            // set up authentication provider
            if (authenticationProvider == null)
            {
                string authenticationProviderClass;
                if (sessionParameters.TryGetValue(SessionParameter.AuthenticationProviderClass, out authenticationProviderClass))
                {
                    try
                    {
                        Type authProvType = Type.GetType(authenticationProviderClass);
                        authenticationProvider = (IAuthenticationProvider)Activator.CreateInstance(authProvType);
                        authenticationProvider.Session = session;
                        session.PutValue(BindingSession.AuthenticationProvider, authenticationProvider);
                    }
                    catch (Exception e)
                    {
                        throw new CmisRuntimeException("Could not load authentictaion provider: " + e.Message, e);
                    }
                }
            }
            else
            {
                authenticationProvider.Session = session;
                session.PutValue(BindingSession.AuthenticationProvider, authenticationProvider);
            }

            // initialize the SPI
            GetSpi();

            // set up caches
            ClearAllCaches();

            // set up repository service
            repositoryServiceWrapper = new BindingRepositoryService(session);
        }

        public string BindingType
        {
            get
            {
                CheckSession();

                string bindingType = session.GetValue(SessionParameter.BindingType) as string;

                if (bindingType == null)
                {
                    bindingType = PortCMIS.BindingType.Custom;
                }

                return bindingType;
            }
        }

        public IRepositoryService GetRepositoryService()
        {
            CheckSession();
            return repositoryServiceWrapper;
        }

        public INavigationService GetNavigationService()
        {
            CheckSession();
            ICmisSpi spi = GetSpi();
            return spi.GetNavigationService();
        }

        public IObjectService GetObjectService()
        {
            CheckSession();
            ICmisSpi spi = GetSpi();
            return spi.GetObjectService();
        }

        public IVersioningService GetVersioningService()
        {
            CheckSession();
            ICmisSpi spi = GetSpi();
            return spi.GetVersioningService();
        }

        public IRelationshipService GetRelationshipService()
        {
            CheckSession();
            ICmisSpi spi = GetSpi();
            return spi.GetRelationshipService();
        }

        public IDiscoveryService GetDiscoveryService()
        {
            CheckSession();
            ICmisSpi spi = GetSpi();
            return spi.GetDiscoveryService();
        }

        public IMultiFilingService GetMultiFilingService()
        {
            CheckSession();
            ICmisSpi spi = GetSpi();
            return spi.GetMultiFilingService();
        }

        public IAclService GetAclService()
        {
            CheckSession();
            ICmisSpi spi = GetSpi();
            return spi.GetAclService();
        }

        public IPolicyService GetPolicyService()
        {
            CheckSession();
            ICmisSpi spi = GetSpi();
            return spi.GetPolicyService();
        }

        public IAuthenticationProvider GetAuthenticationProvider()
        {
            return session.GetAuthenticationProvider();
        }

        public void ClearAllCaches()
        {
            CheckSession();

            lock (bindingLock)
            {
                session.PutValue(BindingSession.RepositoryInfoCache, new RepositoryInfoCache(session));
                session.PutValue(BindingSession.TypeDefinitionCache, new TypeDefinitionCache(session));

                ICmisSpi spi = GetSpi();
                spi.ClearAllCaches();
            }
        }

        public void ClearRepositoryCache(string repositoryId)
        {
            CheckSession();

            if (repositoryId == null)
            {
                return;
            }

            lock (bindingLock)
            {
                RepositoryInfoCache repInfoCache = session.GetRepositoryInfoCache();
                repInfoCache.Remove(repositoryId);

                TypeDefinitionCache typeDefCache = session.GetTypeDefinitionCache();
                typeDefCache.Remove(repositoryId);

                ICmisSpi spi = GetSpi();
                spi.ClearRepositoryCache(repositoryId);
            }
        }

        public void Dispose()
        {
            CheckSession();

            lock (bindingLock)
            {
                GetSpi().Dispose();
                session = null;
            }
        }

        private void CheckSession()
        {
            if (session == null)
            {
                throw new InvalidOperationException("Already closed.");
            }
        }

        private ICmisSpi GetSpi()
        {
            return session.GetSpi();
        }
    }


    /// <summary>
    /// Session object implementation of the binding layer.
    /// </summary>
    internal class BindingSession : IBindingSession
    {
        public const string RepositoryInfoCache = "org.apache.chemistry.portcmis.bindings.repositoryInfoCache";
        public const string TypeDefinitionCache = "org.apache.chemistry.portcmis.bindings.typeDefintionCache";
        public const string AuthenticationProvider = "org.apache.chemistry.portcmis.bindings.authenticationProvider";
        public const string SpiObject = "org.apache.chemistry.portcmis.bindings.spi.object";
        public const string HttpInvokerObject = "org.apache.chemistry.portcmis.binding.httpinvoker.object";

        private Dictionary<string, object> data;
        private object sessionLock = new object();

        public BindingSession()
        {
            data = new Dictionary<string, object>();
        }

        public object GetValue(string key)
        {
            return GetValue(key, null);
        }

        public object GetValue(string key, object defValue)
        {
            object result = null;

            lock (sessionLock)
            {
                if (data.TryGetValue(key, out result))
                {
                    return result;
                }
                else
                {
                    return defValue;
                }
            }
        }

        public int GetValue(string key, int defValue)
        {
            object value = GetValue(key);

            try
            {
                if (value is string)
                {
                    return Int32.Parse((string)value);
                }
                else if (value is int)
                {
                    return (int)value;
                }
            }
            catch (Exception)
            {
            }

            return defValue;
        }

        public bool GetValue(string key, bool defValue)
        {
            object value = GetValue(key);

            try
            {
                if (value is string)
                {
                    return Convert.ToBoolean((string)value);
                }
                else if (value is bool)
                {
                    return (bool)value;
                }
            }
            catch (Exception)
            {
            }

            return defValue;
        }

        public void PutValue(string key, object value)
        {
            lock (sessionLock)
            {
                data[key] = value;
            }
        }

        public void RemoveValue(string key)
        {
            lock (sessionLock)
            {
                data.Remove(key);
            }
        }

        public ICmisSpi GetSpi()
        {
            lock (sessionLock)
            {
                ICmisSpi spi = GetValue(SpiObject) as ICmisSpi;
                if (spi != null)
                {
                    return spi;
                }

                // ok, we have to create it...
                try
                {
                    object spiObject;
                    if (data.TryGetValue(SessionParameter.BindingSpiClass, out spiObject))
                    {
                        string spiClassName = spiObject as string;
                        if (spiClassName != null)
                        {
                            Type spiClass = Type.GetType(spiClassName);
                            spi = (ICmisSpi)Activator.CreateInstance(spiClass);
                            spi.Initialize(this);
                        }
                        else
                        {
                            throw new CmisRuntimeException("SPI class is not set!");
                        }
                    }
                    else
                    {
                        throw new CmisRuntimeException("SPI class is not set!");
                    }
                }
                catch (CmisBaseException)
                {
                    throw;
                }
                catch (Exception e)
                {
                    throw new CmisRuntimeException("SPI cannot be initialized: " + e.Message, e);
                }

                // we have a SPI object -> put it into the session
                data[SpiObject] = spi;

                return spi;
            }
        }

        public RepositoryInfoCache GetRepositoryInfoCache()
        {
            return GetValue(RepositoryInfoCache) as RepositoryInfoCache;
        }

        public TypeDefinitionCache GetTypeDefinitionCache()
        {
            return GetValue(TypeDefinitionCache) as TypeDefinitionCache;
        }

        public IAuthenticationProvider GetAuthenticationProvider()
        {
            return GetValue(AuthenticationProvider) as IAuthenticationProvider;
        }

        public IHttpInvoker GetHttpInvoker()
        {
            lock (sessionLock)
            {
                IHttpInvoker invoker = GetValue(HttpInvokerObject) as IHttpInvoker;
                if (invoker != null)
                {
                    return invoker;
                }

                // ok, we have to create it...
                try
                {
                    object invokerObject;
                    if (data.TryGetValue(SessionParameter.HttpInvokerClass, out invokerObject))
                    {
                        string invokerClassName = invokerObject as string;
                        if (invokerClassName != null)
                        {
                            Type invokerClass = Type.GetType(invokerClassName);
                            invoker = (IHttpInvoker)Activator.CreateInstance(invokerClass);
                        }
                        else
                        {
                            throw new CmisRuntimeException("HTTP invoker class is not set!");
                        }
                    }
                    else
                    {
                        throw new CmisRuntimeException("HTTP invoker class is not set!");
                    }
                }
                catch (CmisBaseException)
                {
                    throw;
                }
                catch (Exception e)
                {
                    throw new CmisRuntimeException("HTTP invoker cannot be initialized: " + e.Message, e);
                }

                // we have an invoker object -> put it into the session
                data[HttpInvokerObject] = invoker;

                return invoker;
            }
        }

        public override string ToString()
        {
            return data.ToString();
        }
    }

    /// <summary>
    /// Repository service proxy that caches repository infos and type defintions.
    /// </summary>
    internal class BindingRepositoryService : IRepositoryService
    {
        private BindingSession session;

        public BindingRepositoryService(BindingSession session)
        {
            this.session = session;
        }

        public IList<IRepositoryInfo> GetRepositoryInfos(IExtensionsData extension)
        {
            IList<IRepositoryInfo> result = null;
            bool hasExtension = (extension != null) && (extension.Extensions != null) && (extension.Extensions.Count > 0);

            // get the SPI and fetch the repository infos
            ICmisSpi spi = session.GetSpi();
            result = spi.GetRepositoryService().GetRepositoryInfos(extension);

            // put it into the cache
            if (!hasExtension && (result != null))
            {
                RepositoryInfoCache cache = session.GetRepositoryInfoCache();
                foreach (IRepositoryInfo rid in result)
                {
                    cache.Put(rid);
                }
            }

            return result;
        }

        public IRepositoryInfo GetRepositoryInfo(string repositoryId, IExtensionsData extension)
        {
            IRepositoryInfo result = null;
            bool hasExtension = (extension != null) && (extension.Extensions != null) && (extension.Extensions.Count > 0);

            RepositoryInfoCache cache = session.GetRepositoryInfoCache();

            // if extension is not set, check the cache first
            if (!hasExtension)
            {
                result = cache.Get(repositoryId);
                if (result != null)
                {
                    return result;
                }
            }

            // it was not in the cache -> get the SPI and fetch the repository info
            ICmisSpi spi = session.GetSpi();
            result = spi.GetRepositoryService().GetRepositoryInfo(repositoryId, extension);

            // put it into the cache
            if (!hasExtension)
            {
                cache.Put(result);
            }

            return result;
        }

        public ITypeDefinitionList GetTypeChildren(string repositoryId, string typeId, bool? includePropertyDefinitions,
                BigInteger? maxItems, BigInteger? skipCount, IExtensionsData extension)
        {
            ITypeDefinitionList result = null;
            bool hasExtension = (extension != null) && (extension.Extensions != null) && (extension.Extensions.Count > 0);

            // get the SPI and fetch the type definitions
            ICmisSpi spi = session.GetSpi();
            result = spi.GetRepositoryService().GetTypeChildren(repositoryId, typeId, includePropertyDefinitions, maxItems,
                    skipCount, extension);

            // put it into the cache
            if (!hasExtension && (includePropertyDefinitions ?? false) && (result != null) && (result.List != null))
            {
                TypeDefinitionCache cache = session.GetTypeDefinitionCache();
                foreach (ITypeDefinition tdd in result.List)
                {
                    cache.Put(repositoryId, tdd);
                }
            }

            return result;
        }

        public IList<ITypeDefinitionContainer> GetTypeDescendants(string repositoryId, string typeId, BigInteger? depth,
                bool? includePropertyDefinitions, IExtensionsData extension)
        {
            IList<ITypeDefinitionContainer> result = null;
            bool hasExtension = (extension != null) && (extension.Extensions != null) && (extension.Extensions.Count > 0);

            // get the SPI and fetch the type definitions
            ICmisSpi spi = session.GetSpi();
            result = spi.GetRepositoryService().GetTypeDescendants(repositoryId, typeId, depth, includePropertyDefinitions,
                    extension);

            // put it into the cache
            if (!hasExtension && (includePropertyDefinitions ?? false) && (result != null))
            {
                TypeDefinitionCache cache = session.GetTypeDefinitionCache();
                AddToTypeCache(cache, repositoryId, result);
            }

            return result;
        }

        private void AddToTypeCache(TypeDefinitionCache cache, string repositoryId, IList<ITypeDefinitionContainer> containers)
        {
            if (containers == null)
            {
                return;
            }

            foreach (ITypeDefinitionContainer container in containers)
            {
                cache.Put(repositoryId, container.TypeDefinition);
                AddToTypeCache(cache, repositoryId, container.Children);
            }
        }

        public ITypeDefinition GetTypeDefinition(string repositoryId, string typeId, IExtensionsData extension)
        {
            ITypeDefinition result = null;
            bool hasExtension = (extension != null) && (extension.Extensions != null) && (extension.Extensions.Count > 0);

            TypeDefinitionCache cache = session.GetTypeDefinitionCache();

            // if extension is not set, check the cache first
            if (!hasExtension)
            {
                result = cache.Get(repositoryId, typeId);
                if (result != null)
                {
                    return result;
                }
            }

            // it was not in the cache -> get the SPI and fetch the type definition
            ICmisSpi spi = session.GetSpi();
            result = spi.GetRepositoryService().GetTypeDefinition(repositoryId, typeId, extension);

            // put it into the cache
            if (!hasExtension && (result != null))
            {
                cache.Put(repositoryId, result);
            }

            return result;
        }

        public ITypeDefinition CreateType(string repositoryId, ITypeDefinition type, IExtensionsData extension)
        {
            ICmisSpi spi = session.GetSpi();
            return spi.GetRepositoryService().CreateType(repositoryId, type, extension);
        }

        public ITypeDefinition UpdateType(string repositoryId, ITypeDefinition type, IExtensionsData extension)
        {
            ICmisSpi spi = session.GetSpi();
            return spi.GetRepositoryService().UpdateType(repositoryId, type, extension);
        }

        public void DeleteType(string repositoryId, string typeId, IExtensionsData extension)
        {
            ICmisSpi spi = session.GetSpi();
            spi.GetRepositoryService().DeleteType(repositoryId, typeId, extension);
        }
    }
}
