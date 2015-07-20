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

using PortCMIS.Binding;
using System.Collections.Generic;

namespace PortCMIS.Client.Impl
{
    /// <summary>
    /// Session factory implementation for Store Apps.
    /// </summary>
    public class WindowsSessionFactory : ISessionFactory
    {
        protected SessionFactory DefaultSessionFactory { get; private set; }

        private WindowsSessionFactory()
        {
            DefaultSessionFactory = SessionFactory.NewInstance();
        }

        public static WindowsSessionFactory NewInstance()
        {
            return new WindowsSessionFactory();
        }

        public ISession CreateSession(IDictionary<string, string> parameters)
        {
            return DefaultSessionFactory.CreateSession(AddWindowClasses(parameters));
        }

        public ISession CreateSession(IDictionary<string, string> parameters, IObjectFactory objectFactory, IAuthenticationProvider authenticationProvider, ICache cache)
        {
            return DefaultSessionFactory.CreateSession(AddWindowClasses(parameters), objectFactory, authenticationProvider, cache);
        }

        public IList<IRepository> GetRepositories(IDictionary<string, string> parameters)
        {
            return DefaultSessionFactory.GetRepositories(AddWindowClasses(parameters));
        }

        public IList<IRepository> GetRepositories(IDictionary<string, string> parameters, IObjectFactory objectFactory, IAuthenticationProvider authenticationProvider, ICache cache)
        {
            return DefaultSessionFactory.GetRepositories(AddWindowClasses(parameters), objectFactory, authenticationProvider, cache);
        }

        protected IDictionary<string, string> AddWindowClasses(IDictionary<string, string> parameters)
        {
            if (parameters == null)
            {
                return parameters;
            }

            if (parameters.ContainsKey(SessionParameter.HttpInvokerClass) && parameters.ContainsKey(SessionParameter.AuthenticationProviderClass))
            {
                return parameters;
            }

            IDictionary<string, string> newParameters = new Dictionary<string, string>(parameters);

            if (!newParameters.ContainsKey(SessionParameter.HttpInvokerClass))
            {
                newParameters.Add(SessionParameter.HttpInvokerClass, typeof(PortCMIS.Binding.Http.WindowsHttpInvoker).AssemblyQualifiedName);
            }

            if (!newParameters.ContainsKey(SessionParameter.AuthenticationProviderClass))
            {
                newParameters.Add(SessionParameter.AuthenticationProviderClass, typeof(PortCMIS.Binding.StandardWindowsAuthenticationProvider).AssemblyQualifiedName);
            }

            return newParameters;
        }
    }
}
