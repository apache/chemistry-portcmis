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

using PortCMIS.Binding.Impl;
using PortCMIS.Client;
using PortCMIS.Data;
using PortCMIS.Enums;
using PortCMIS.Exceptions;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace PortCMIS.Binding.AtomPub
{

    internal class AtomPubParser
    {
        public const string LinkRelContent = "@@content@@";

        private Stream stream;
        private AtomBase parseResult;

        public AtomPubParser(Stream stream)
        {
            if (stream == null)
            {
                throw new ArgumentNullException(nameof(stream));
            }

            this.stream = stream;
        }

        /// <summary>
        /// Parses the stream.
        /// </summary>
        public void Parse()
        {
            XmlReader parser = XmlUtils.CreateParser(stream);

            try
            {
                while (true)
                {
                    XmlNodeType nodeType = parser.NodeType;
                    if (nodeType == XmlNodeType.Element)
                    {
                        string uri = parser.NamespaceURI;
                        string local = parser.LocalName;

                        if (uri == XmlConstants.NamespaceAtom)
                        {
                            if (local == XmlConstants.TAG_FEED)
                            {
                                parseResult = ParseFeed(parser);
                                break;
                            }
                            else if (local == XmlConstants.TAG_ENTRY)
                            {
                                parseResult = ParseEntry(parser);
                                break;
                            }
                        }
                        else if (uri == XmlConstants.NamespaceCmis)
                        {
                            if (local == XmlConstants.TAG_ALLOWABLEACTIONS)
                            {
                                parseResult = ParseAllowableActions(parser);
                                break;
                            }
                            else if (local == XmlConstants.TAG_ACL)
                            {
                                parseResult = ParseACL(parser);
                                break;
                            }
                        }
                        else if (uri == XmlConstants.NamespaceApp)
                        {
                            if (local == XmlConstants.TAG_SERVICE)
                            {
                                parseResult = ParseServiceDoc(parser);
                                break;
                            }
                        }
                        else if (string.Equals(XmlConstants.TAG_HTML, local, StringComparison.OrdinalIgnoreCase))
                        {
                            parseResult = new HtmlDoc();
                            break;
                        }
                    }

                    if (!XmlUtils.Next(parser))
                    {
                        break;
                    }
                }
            }
            finally
            {
                parser.Dispose();

                // make sure the stream is read and closed in all cases
                IOUtils.ConsumeAndClose(stream);
            }
        }

        /// <summary>
        /// Return the parse results.
        /// </summary>
        public AtomBase GetResults()
        {
            return parseResult;
        }

        /// <summary>
        /// Parses a service document.
        /// </summary>
        private static ServiceDoc ParseServiceDoc(XmlReader parser)
        {
            ServiceDoc result = new ServiceDoc();

            XmlUtils.Next(parser);

            while (true)
            {
                XmlNodeType nodeType = parser.NodeType;
                if (nodeType == XmlNodeType.Element)
                {
                    string uri = parser.NamespaceURI;
                    string local = parser.LocalName;

                    if (uri == XmlConstants.NamespaceApp)
                    {
                        if (local == XmlConstants.TAG_WORKSPACE)
                        {
                            result.AddWorkspace(ParseWorkspace(parser));
                        }
                        else
                        {
                            XmlUtils.Skip(parser);
                        }
                    }
                    else
                    {
                        XmlUtils.Skip(parser);
                    }
                }
                else if (nodeType == XmlNodeType.EndElement)
                {
                    break;
                }
                else
                {
                    if (!XmlUtils.Next(parser))
                    {
                        break;
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Parses a workspace element in a service document.
        /// </summary>
        private static RepositoryWorkspace ParseWorkspace(XmlReader parser)
        {
            RepositoryWorkspace workspace = new RepositoryWorkspace();

            XmlUtils.Next(parser);

            while (true)
            {
                XmlNodeType nodeType = parser.NodeType;
                if (nodeType == XmlNodeType.Element)
                {
                    AtomElement element = ParseWorkspaceElement(parser);

                    // check if we can extract the workspace id
                    if (element != null && (element.Object is IRepositoryInfo))
                    {
                        workspace.Id = ((IRepositoryInfo)element.Object).Id;
                    }

                    // add to workspace
                    workspace.AddElement(element);
                }
                else if (nodeType == XmlNodeType.EndElement)
                {
                    break;
                }
                else
                {
                    if (!XmlUtils.Next(parser))
                    {
                        break;
                    }
                }
            }

            XmlUtils.Next(parser);

            return workspace;
        }

        /// <summary>
        /// Parses an Atom feed.
        /// </summary>
        private AtomFeed ParseFeed(XmlReader parser)
        {
            AtomFeed result = new AtomFeed();

            XmlUtils.Next(parser);

            while (true)
            {
                XmlNodeType nodeType = parser.NodeType;
                if (nodeType == XmlNodeType.Element)
                {
                    string uri = parser.NamespaceURI;
                    string local = parser.LocalName;

                    if (uri == XmlConstants.NamespaceAtom)
                    {
                        if (local == XmlConstants.TAG_LINK)
                        {
                            result.AddElement(ParseLink(parser));
                        }
                        else if (local == XmlConstants.TAG_ENTRY)
                        {
                            result.AddEntry(ParseEntry(parser));
                        }
                        else
                        {
                            XmlUtils.Skip(parser);
                        }
                    }
                    else if (uri == XmlConstants.NamespaceRestAtom)
                    {
                        if (local == XmlConstants.TAG_NUM_ITEMS)
                        {
                            result.AddElement(ParseBigInteger(parser));
                        }
                        else
                        {
                            XmlUtils.Skip(parser);
                        }
                    }
                    else if (uri == XmlConstants.NamespaceApacheChemistry)
                    {
                        result.AddElement(ParseText(parser));
                    }
                    else
                    {
                        XmlUtils.Skip(parser);
                    }
                }
                else if (nodeType == XmlNodeType.EndElement)
                {
                    break;
                }
                else
                {
                    if (!XmlUtils.Next(parser))
                    {
                        break;
                    }
                }
            }

            XmlUtils.Next(parser);

            return result;
        }

        /// <summary>
        /// Parses an Atom entry.
        /// </summary>
        private AtomEntry ParseEntry(XmlReader parser)
        {
            AtomEntry result = new AtomEntry();

            XmlUtils.Next(parser);

            // walk through all tags in entry
            while (true)
            {
                XmlNodeType nodeType = parser.NodeType;
                if (nodeType == XmlNodeType.Element)
                {
                    AtomElement element = ParseElement(parser);
                    if (element != null)
                    {
                        // add to entry
                        result.AddElement(element);

                        // find and set object id
                        if (element.Object is IObjectData)
                        {
                            result.Id = ((IObjectData)element.Object).Id;
                        }
                        else if (element.Object is ITypeDefinition)
                        {
                            result.Id = ((ITypeDefinition)element.Object).Id;
                        }
                    }
                }
                else if (nodeType == XmlNodeType.EndElement)
                {
                    break;
                }
                else
                {
                    if (!XmlUtils.Next(parser))
                    {
                        break;
                    }
                }
            }

            XmlUtils.Next(parser);

            return result;
        }

        /// <summary>
        /// Parses an Allowable Actions document.
        /// </summary>
        private static AtomAllowableActions ParseAllowableActions(XmlReader parser)
        {
            return new AtomAllowableActions() { AllowableActions = XmlConverter.ConvertAllowableActions(parser) };
        }

        /// <summary>
        /// Parses an ACL document.
        /// </summary>
        private static AtomAcl ParseACL(XmlReader parser)
        {
            return new AtomAcl() { Acl = XmlConverter.ConvertAcl(parser) };
        }

        /// <summary>
        /// Parses an element.
        /// </summary>
        private AtomElement ParseElement(XmlReader parser)
        {
            string uri = parser.NamespaceURI;
            string local = parser.LocalName;

            if (uri == XmlConstants.NamespaceRestAtom)
            {
                if (local == XmlConstants.TAG_OBJECT)
                {
                    return new AtomElement(uri, local, XmlConverter.ConvertObject(parser));
                }
                else if (local == XmlConstants.TAG_PATH_SEGMENT
                        || local == XmlConstants.TAG_RELATIVE_PATH_SEGMENT)
                {
                    return ParseText(parser);
                }
                else if (local == XmlConstants.TAG_TYPE)
                {
                    return new AtomElement(uri, local, XmlConverter.ConvertTypeDefinition(parser));
                }
                else if (local == XmlConstants.TAG_CHILDREN)
                {
                    return ParseChildren(parser);
                }
            }
            else if (uri == XmlConstants.NamespaceAtom)
            {
                if (local == XmlConstants.TAG_LINK)
                {
                    return ParseLink(parser);
                }
                else if (local == XmlConstants.TAG_CONTENT)
                {
                    return ParseAtomContentSrc(parser);
                }
            }

            // we don't know it - skip it
            XmlUtils.Skip(parser);

            return null;
        }

        /// <summary>
        /// Parses a children element.
        /// </summary>
        private AtomElement ParseChildren(XmlReader parser)
        {
            AtomElement result = null;
            string uri = parser.NamespaceURI;
            string local = parser.LocalName;

            XmlUtils.Next(parser);

            // walk through the children tag
            while (true)
            {
                XmlNodeType nodeType = parser.NodeType;
                if (nodeType == XmlNodeType.Element)
                {
                    string tagUri = parser.NamespaceURI;
                    string tagLocal = parser.LocalName;

                    if (tagUri == XmlConstants.NamespaceAtom)
                    {
                        if (tagLocal == XmlConstants.TAG_FEED)
                        {
                            result = new AtomElement(uri, local, ParseFeed(parser));
                        }
                        else
                        {
                            XmlUtils.Skip(parser);
                        }
                    }
                    else
                    {
                        XmlUtils.Skip(parser);
                    }
                }
                else if (nodeType == XmlNodeType.EndElement)
                {
                    break;
                }
                else
                {
                    if (!XmlUtils.Next(parser))
                    {
                        break;
                    }
                }
            }

            XmlUtils.Next(parser);

            return result;
        }

        /// <summary>
        /// Parses a workspace element.
        /// </summary>
        private static AtomElement ParseWorkspaceElement(XmlReader parser)
        {
            string uri = parser.NamespaceURI;
            string local = parser.LocalName;

            if (uri == XmlConstants.NamespaceRestAtom)
            {
                if (local == XmlConstants.TAG_REPOSITORY_INFO)
                {
                    return new AtomElement(uri, local, XmlConverter.ConvertRepositoryInfo(parser));
                }
                else if (local == XmlConstants.TAG_URI_TEMPLATE)
                {
                    return ParseTemplate(parser);
                }
            }
            else if (uri == XmlConstants.NamespaceAtom)
            {
                if (local == XmlConstants.TAG_LINK)
                {
                    return ParseLink(parser);
                }
            }
            else if (uri == XmlConstants.NamespaceApp)
            {
                if (local == XmlConstants.TAG_COLLECTION)
                {
                    return ParseCollection(parser);
                }
            }

            // we don't know it - skip it
            XmlUtils.Skip(parser);

            return null;
        }

        /// <summary>
        /// Parses a collection tag.
        /// </summary>
        private static AtomElement ParseCollection(XmlReader parser)
        {
            string uri = parser.NamespaceURI;
            string local = parser.LocalName;
            Dictionary<string, string> result = new Dictionary<string, string>()
            {
                ["href"] = parser.GetAttribute("href")
            };

            XmlUtils.Next(parser);

            while (true)
            {
                XmlNodeType nodeType = parser.NodeType;
                if (nodeType == XmlNodeType.Element)
                {
                    string tagUri = parser.NamespaceURI;
                    string tagLocal = parser.LocalName;

                    if (tagUri == XmlConstants.NamespaceRestAtom
                            && tagLocal == XmlConstants.TAG_COLLECTION_TYPE)
                    {
                        result["collectionType"] = XmlUtils.ReadText(parser, XmlConstraints.MaxStringLength);
                    }
                    else
                    {
                        XmlUtils.Skip(parser);
                    }
                }
                else if (nodeType == XmlNodeType.EndElement)
                {
                    break;
                }
                else
                {
                    if (!XmlUtils.Next(parser))
                    {
                        break;
                    }
                }
            }

            XmlUtils.Next(parser);

            return new AtomElement(uri, local, result);
        }

        /// <summary>
        /// Parses a template tag.
        /// </summary>
        private static AtomElement ParseTemplate(XmlReader parser)
        {
            string uri = parser.NamespaceURI;
            string local = parser.LocalName;
            Dictionary<string, string> result = new Dictionary<string, string>();

            XmlUtils.Next(parser);

            while (true)
            {
                XmlNodeType nodeType = parser.NodeType;
                if (nodeType == XmlNodeType.Element)
                {
                    string tagUri = parser.NamespaceURI;
                    string tagLocal = parser.LocalName;

                    if (tagUri == XmlConstants.NamespaceRestAtom)
                    {
                        if (tagLocal == XmlConstants.TAG_TEMPLATE_TEMPLATE)
                        {
                            result["template"] = XmlUtils.ReadText(parser, XmlConstraints.MaxStringLength);
                        }
                        else if (tagLocal == XmlConstants.TAG_TEMPLATE_TYPE)
                        {
                            result["type"] = XmlUtils.ReadText(parser, XmlConstraints.MaxStringLength);
                        }
                        else
                        {
                            XmlUtils.Skip(parser);
                        }
                    }
                    else
                    {
                        XmlUtils.Skip(parser);
                    }
                }
                else if (nodeType == XmlNodeType.EndElement)
                {
                    break;
                }
                else
                {
                    if (!XmlUtils.Next(parser))
                    {
                        break;
                    }
                }
            }

            XmlUtils.Next(parser);

            return new AtomElement(uri, local, result);
        }

        /// <summary>
        /// Parses a link tag.
        /// </summary>
        private static AtomElement ParseLink(XmlReader parser)
        {
            string uri = parser.NamespaceURI;
            string local = parser.LocalName;

            AtomLink result = new AtomLink();

            // save attributes
            if (parser.HasAttributes)
            {
                for (int i = 0; i < parser.AttributeCount; i++)
                {
                    parser.MoveToAttribute(i);

                    if (parser.Name == XmlConstants.LINK_REL)
                    {
                        result.Rel = parser.Value;
                    }
                    else if (parser.Name == XmlConstants.LINK_HREF)
                    {
                        result.Href = parser.Value;
                    }
                    else if (parser.Name == XmlConstants.LINK_TYPE)
                    {
                        result.Type = parser.Value;
                    }
                }
            }

            // skip enclosed tags, if any
            XmlUtils.Skip(parser);

            return new AtomElement(uri, local, result);
        }

        /// <summary>
        /// Parses a link tag.
        /// </summary>
        private static AtomElement ParseAtomContentSrc(XmlReader parser)
        {
            string uri = parser.NamespaceURI;
            string local = parser.LocalName;

            AtomLink result = new AtomLink()
            {
                Rel = LinkRelContent
            };

            // save attributes
            if (parser.HasAttributes)
            {
                for (int i = 0; i < parser.AttributeCount; i++)
                {
                    parser.MoveToAttribute(i);

                    if (parser.Name == XmlConstants.CONTENT_SRC)
                    {
                        result.Href = parser.Value;
                    }
                }
            }

            // skip enclosed tags, if any
            XmlUtils.Skip(parser);

            return new AtomElement(uri, local, result);
        }

        /// <summary>
        /// Parses a text tag.
        /// </summary>
        private static AtomElement ParseText(XmlReader parser)
        {
            return new AtomElement(parser.NamespaceURI, parser.LocalName, XmlUtils.ReadText(parser, XmlConstraints.MaxStringLength));
        }

        /// <summary>
        /// Parses a text tag and convert it into an integer.
        /// </summary>
        private static AtomElement ParseBigInteger(XmlReader parser)
        {
            return new AtomElement(parser.NamespaceURI, parser.LocalName,
                BigInteger.Parse(XmlUtils.ReadText(parser, XmlConstraints.MaxStringLength), NumberStyles.Integer, CultureInfo.InvariantCulture));
        }
    }

    internal class AtomElement
    {
        public string Namespace { get; private set; }
        public string LocalName { get; private set; }
        public object Object { get; private set; }

        public AtomElement(string elementNamespace, string elementLocalName, object elementObject)
        {
            Namespace = elementNamespace;
            LocalName = elementLocalName;
            Object = elementObject;
        }

        public override string ToString()
        {
            return "{" + Namespace + "}" + LocalName + ": " + Object;
        }
    }

    internal abstract class AtomBase
    {
        private readonly IList<AtomElement> elements = new List<AtomElement>();

        public abstract string Type { get; }

        public IList<AtomElement> Elements { get { return elements; } }

        public void AddElement(AtomElement element)
        {
            if (element != null)
            {
                elements.Add(element);
            }
        }
    }

    internal class RepositoryWorkspace : AtomBase
    {
        public string Id { get; set; }
        public override string Type { get { return "Repository Workspace"; } }

        public override string ToString()
        {
            return "Workspace \"" + Id + "\": " + Elements;
        }
    }

    internal class ServiceDoc : AtomBase
    {
        private readonly IList<RepositoryWorkspace> workspaces = new List<RepositoryWorkspace>();

        public override string Type { get { return "Service Document"; } }
        public IList<RepositoryWorkspace> Workspaces { get { return workspaces; } }

        public void AddWorkspace(RepositoryWorkspace ws)
        {
            if (ws != null)
            {
                workspaces.Add(ws);
            }
        }

        public override string ToString()
        {
            return "Service Doc: " + workspaces;
        }
    }

    internal class AtomEntry : AtomBase
    {
        public string Id { get; set; }
        public override string Type { get { return "Atom Entry"; } }

        public override string ToString()
        {
            return "Entry \"" + Id + "\": " + Elements;
        }
    }

    internal class AtomFeed : AtomBase
    {
        private IList<AtomEntry> entries = new List<AtomEntry>();

        public override string Type { get { return "Atom Feed"; } }
        public IList<AtomEntry> Entries { get { return entries; } }

        public void AddEntry(AtomEntry entry)
        {
            if (entry != null)
            {
                entries.Add(entry);
            }
        }

        public override string ToString()
        {
            return "Feed: " + Elements;
        }
    }

    internal class AtomAllowableActions : AtomBase
    {
        public IAllowableActions AllowableActions { get; set; }
        public override string Type { get { return "Allowable Actions"; } }
    }

    internal class AtomAcl : AtomBase
    {
        public IAcl Acl { get; set; }
        public override string Type { get { return "ACL"; } }
    }

    internal class HtmlDoc : AtomBase
    {
        public override string Type { get { return "HTML document"; } }
    }

    internal class AtomLink
    {
        public string Rel { get; set; }
        public string Type { get; set; }
        public string Href { get; set; }
    }

    internal class LinkCache
    {
        private static readonly HashSet<string> KnownLinks = new HashSet<string>()
        {
            BindingConstants.RelAcl,
            BindingConstants.RelDown,
            BindingConstants.RelUp,
            BindingConstants.RelFolderTree,
            BindingConstants.RelRelationships,
            BindingConstants.RelSelf,
            BindingConstants.RelAllowableActions,
            BindingConstants.RelEditMedia,
            BindingConstants.RelPolicies,
            BindingConstants.RelVersionHistory,
            BindingConstants.RelWorkingCopy,
            AtomPubParser.LinkRelContent
        };

        static LinkCache()
        {
        }

        private readonly IBindingCache linkCache;
        private readonly IBindingCache typeLinkCache;
        private readonly IBindingCache collectionLinkCache;
        private readonly IBindingCache templateCache;
        private readonly IBindingCache repositoryLinkCache;

        /// <summary>
        /// Constructor.
        /// </summary>
        public LinkCache(BindingSession session)
        {
            int repCount = session.GetValue(SessionParameter.CacheSizeRepositories,
                    SessionParameterDefaults.CacheSizeRepositories);
            if (repCount < 1)
            {
                repCount = SessionParameterDefaults.CacheSizeRepositories;
            }

            int typeCount = session.GetValue(SessionParameter.CacheSizeTypes, SessionParameterDefaults.CacheSizeTypes);
            if (typeCount < 1)
            {
                typeCount = SessionParameterDefaults.CacheSizeTypes;
            }

            int objCount = session.GetValue(SessionParameter.CacheSizeLinks, SessionParameterDefaults.CacheSizeLinks);
            if (objCount < 1)
            {
                objCount = SessionParameterDefaults.CacheSizeLinks;
            }

            string dictionaryLevelName = typeof(DictionaryCacheLevel).FullName;
            string lruLevelName = typeof(LruCacheLevel).FullName;
            string contentTypeLevelName = typeof(DictionaryCacheLevel).FullName;

            linkCache = new Cache("Link Cache");
            linkCache.Initialize(new string[] {
                dictionaryLevelName + " " + DictionaryCacheLevel.Capacity + "=" + repCount.ToString(), // repository
                lruLevelName + " " + LruCacheLevel.MaxEntries + "=" + objCount.ToString(), // id
                dictionaryLevelName + " " + DictionaryCacheLevel.Capacity + "=16", // rel
                contentTypeLevelName + " " + DictionaryCacheLevel.Capacity + "=3,"
                        + DictionaryCacheLevel.SingleValue + "=true" // type
        });

            typeLinkCache = new Cache("Type Link Cache");
            typeLinkCache.Initialize(new string[] {
                dictionaryLevelName + " " + DictionaryCacheLevel.Capacity + "=" + repCount.ToString(), // repository
                lruLevelName + " " + LruCacheLevel.MaxEntries + "=" + typeCount.ToString(), // id
                dictionaryLevelName + " " + DictionaryCacheLevel.Capacity + "=16", // rel
                contentTypeLevelName + " " + DictionaryCacheLevel.Capacity + "=3,"
                        + DictionaryCacheLevel.SingleValue + "=true"// type
        });

            collectionLinkCache = new Cache("Collection Link Cache");
            collectionLinkCache.Initialize(new string[] {
                dictionaryLevelName + " " + DictionaryCacheLevel.Capacity + "=" + repCount.ToString(), // repository
                dictionaryLevelName + " " + DictionaryCacheLevel.Capacity + "=8" // collection
        });

            templateCache = new Cache("URI Template Cache");
            templateCache.Initialize(new string[] {
                dictionaryLevelName + " " + DictionaryCacheLevel.Capacity + "=" + repCount.ToString(), // repository
                dictionaryLevelName + " " + DictionaryCacheLevel.Capacity + "=6" // type
        });

            repositoryLinkCache = new Cache("Repository Link Cache");
            repositoryLinkCache.Initialize(new string[] {
                dictionaryLevelName + " " + DictionaryCacheLevel.Capacity + "=" + repCount.ToString(), // repository
                dictionaryLevelName + " " + DictionaryCacheLevel.Capacity + "=6" // rel
        });
        }

        /// <summary>
        /// Adds a link.
        /// </summary>
        public void AddLink(string repositoryId, string id, string rel, string type, string link)
        {
            if (KnownLinks.Contains(rel))
            {
                linkCache.Put(new string[] { repositoryId, id, rel, type }, link);
            }
            else if (BindingConstants.RelAlternate == rel)
            {
                // use streamId instead of type as discriminating parameter
                string streamId = ExtractStreamId(link);
                if (streamId != null)
                {
                    linkCache.Put(new string[] { repositoryId, id, rel, streamId }, link);
                }
            }
        }

        /// <summary>
        /// Tries to extract a streamId from an alternate link.
        /// this is not strictly in the spec
        /// </summary>
        protected string ExtractStreamId(string link)
        {
            int i = link.LastIndexOf('?');
            if (i > 0)
            {
                string[] parameters = link.Substring(i + 1).Split('&');
                foreach (string param in parameters)
                {
                    string[] parts = param.Split(new char[] { '=' }, 2);
                    if (parts[0] == BindingConstants.ParamStreamId && parts.Length == 2)
                    {
                        return parts[1];
                    }
                }
            }
            return null;
        }

        /// <summary>
        /// Removes all links of an object.
        /// </summary>
        public void RemoveLinks(string repositoryId, string id)
        {
            linkCache.Remove(new string[] { repositoryId, id });
        }

        /// <summary>
        /// Gets a link.
        /// </summary>
        public string GetLink(string repositoryId, string id, string rel, string type)
        {
            return (string)linkCache.Get(new string[] { repositoryId, id, rel, type });
        }

        /// <summary>
        /// Gets a link.
        /// </summary>
        public string GetLink(string repositoryId, string id, string rel)
        {
            return GetLink(repositoryId, id, rel, null);
        }

        /// <summary>
        /// Checks a link.
        /// </summary>
        public int CheckLink(string repositoryId, string id, string rel, string type)
        {
            return linkCache.Check(new string[] { repositoryId, id, rel, type });
        }

        /// <summary>
        /// Locks the link cache.
        /// </summary>
        public void LockLinks()
        {
            linkCache.Lock();
        }

        /// <summary>
        /// Unlocks the link cache.
        /// </summary>
        public void UnlockLinks()
        {
            linkCache.Unlock();
        }

        /// <summary>
        /// Adds a type link.
        /// </summary>
        public void AddTypeLink(string repositoryId, string id, string rel, string type, string link)
        {
            if (KnownLinks.Contains(rel))
            {
                typeLinkCache.Put(new string[] { repositoryId, id, rel, type }, link);
            }
        }

        /// <summary>
        /// Removes all links of a type.
        /// </summary>
        public void RemoveTypeLinks(string repositoryId, string id)
        {
            typeLinkCache.Remove(new string[] { repositoryId, id });
        }

        /// <summary>
        /// Gets a type link.
        /// </summary>
        public string GetTypeLink(string repositoryId, string id, string rel, string type)
        {
            return (string)typeLinkCache.Get(new string[] { repositoryId, id, rel, type });
        }

        /// <summary>
        /// Locks the type link cache.
        /// </summary>
        public void LockTypeLinks()
        {
            typeLinkCache.Lock();
        }

        /// <summary>
        /// Unlocks the type link cache.
        /// </summary>
        public void UnlockTypeLinks()
        {
            typeLinkCache.Unlock();
        }

        /// <summary>
        /// Adds a collection.
        /// </summary>
        public void AddCollection(string repositoryId, string collection, string link)
        {
            collectionLinkCache.Put(new string[] { repositoryId, collection }, link);
        }

        /// <summary>
        /// Gets a collection.
        /// </summary>
        public string GetCollection(string repositoryId, string collection)
        {
            return (string)collectionLinkCache.Get(new string[] { repositoryId, collection });
        }

        /// <summary>
        /// Adds an URI template.
        /// </summary>
        public void AddTemplate(string repositoryId, string type, string link)
        {
            templateCache.Put(new string[] { repositoryId, type }, link);
        }

        /// <summary>
        /// Gets an URI template and replaces place holders with the given
        /// parameters.
        /// </summary>
        public string GetTemplateLink(string repositoryId, string type, IDictionary<string, object> parameters)
        {
            string template = (string)templateCache.Get(new string[] { repositoryId, type });
            if (template == null)
            {
                return null;
            }

            StringBuilder result = new StringBuilder();
            StringBuilder param = new StringBuilder();

            bool paramMode = false;
            for (int i = 0; i < template.Length; i++)
            {
                char c = template[i];

                if (paramMode)
                {
                    if (c == '}')
                    {
                        paramMode = false;

                        object paramValue;
                        if (parameters.TryGetValue(param.ToString(), out paramValue))
                        {
                            result.Append(paramValue == null ? "" : Uri.EscapeDataString(UrlBuilder.NormalizeParameter(paramValue)));
                        }

                        param.Length = 0;
                    }
                    else
                    {
                        param.Append(c);
                    }
                }
                else
                {
                    if (c == '{')
                    {
                        paramMode = true;
                    }
                    else
                    {
                        result.Append(c);
                    }
                }
            }

            return result.ToString();
        }

        // ---- repository links ----

        /// <summary>
        /// Adds a collection.
        /// </summary>
        public void AddRepositoryLink(string repositoryId, string rel, string link)
        {
            repositoryLinkCache.Put(new string[] { repositoryId, rel }, link);
        }

        /// <summary>
        /// Gets a collection.
        /// </summary>
        public string GetRepositoryLink(string repositoryId, string rel)
        {
            return (string)repositoryLinkCache.Get(new string[] { repositoryId, rel });
        }

        // ---- clear ----

        /// <summary>
        /// Removes all entries of the given repository from the caches.
        /// </summary>
        public void ClearRepository(string repositoryId)
        {
            linkCache.Remove(new string[] { repositoryId });
            typeLinkCache.Remove(new string[] { repositoryId });
            collectionLinkCache.Remove(new string[] { repositoryId });
            templateCache.Remove(new string[] { repositoryId });
            repositoryLinkCache.Remove(new string[] { repositoryId });
        }

        public override string ToString()
        {
            return "Link Cache [link cache=" + linkCache + ", type link cache=" + typeLinkCache
                    + ", collection link cache=" + collectionLinkCache + ", repository link cache=" + repositoryLinkCache
                    + ",  template cache=" + templateCache + "]";
        }
    }

    /// <summary>
    /// Writes a CMIS Atom entry to an output stream.
    /// </summary>
    internal class AtomEntryWriter
    {
        private const int BufferSize = 64 * 1024;

        private CmisVersion cmisVersion;
        private IObjectData objectData;
        private IContentStream contentStream;
        private readonly Stream stream;
        private ITypeDefinition typeDef;
        private BulkUpdate bulkUpdate;

        /// <summary>
        /// Constructor for objects.
        /// </summary>
        public AtomEntryWriter(IObjectData objectData, CmisVersion cmisVersion)
            : this(objectData, cmisVersion, null)
        {
        }

        /// <summary>
        /// Constructor for objects.
        /// </summary>
        public AtomEntryWriter(IObjectData objectData, CmisVersion cmisVersion, IContentStream contentStream)
        {
            if (objectData == null || objectData.Properties == null)
            {
                throw new CmisInvalidArgumentException("Object and properties must not be null!");
            }

            if (contentStream != null && contentStream.MimeType == null)
            {
                throw new CmisInvalidArgumentException("Media type must be set if a stream is present!");
            }

            this.objectData = objectData;
            this.cmisVersion = cmisVersion;
            this.contentStream = contentStream;
            if (contentStream != null && contentStream.Stream != null)
            {
                // do we need buffering here?
                stream = contentStream.Stream;
            }
            else
            {
                stream = null;
            }
            this.typeDef = null;
            this.bulkUpdate = null;
        }

        /// <summary>
        /// Constructor for types.
        /// </summary>
        public AtomEntryWriter(ITypeDefinition type, CmisVersion cmisVersion)
        {
            if (type == null)
            {
                throw new CmisInvalidArgumentException("Type must not be null!");
            }

            this.typeDef = type;
            this.cmisVersion = cmisVersion;
            this.objectData = null;
            this.contentStream = null;
            this.stream = null;
            this.bulkUpdate = null;
        }

        /// <summary>
        /// Constructor for bulk updates.
        /// </summary>
        public AtomEntryWriter(BulkUpdate bulkUpdate)
        {
            if (bulkUpdate == null)
            {
                throw new CmisInvalidArgumentException("Bulk update data must not be null!");
            }

            this.bulkUpdate = bulkUpdate;
            this.typeDef = null;
            this.cmisVersion = CmisVersion.Cmis_1_1;
            this.objectData = null;
            this.contentStream = null;
            this.stream = null;
        }

        /// <summary>
        /// Writes the entry to an output stream.
        /// </summary>
        public void Write(Stream outStream)
        {
            using (XmlWriter writer = XmlUtils.CreateWriter(outStream))
            {
                XmlUtils.StartXmlDocument(writer);

                writer.WriteStartElement(XmlConstants.PrefixAtom, "entry", XmlConstants.NamespaceAtom);
                writer.WriteAttributeString("xmlns", XmlConstants.PrefixAtom, null, XmlConstants.NamespaceAtom);
                writer.WriteAttributeString("xmlns", XmlConstants.PrefixCmis, null, XmlConstants.NamespaceCmis);
                writer.WriteAttributeString("xmlns", XmlConstants.PrefixRestAtom, null, XmlConstants.NamespaceRestAtom);

                if (contentStream != null && contentStream.FileName != null)
                {
                    writer.WriteAttributeString("xmlns", XmlConstants.PrefixApacheChemistry, null, XmlConstants.NamespaceApacheChemistry);
                }

                // atom:id
                XmlUtils.Write(writer, XmlConstants.PrefixAtom, XmlConstants.NamespaceAtom, XmlConstants.TAG_ATOM_ID,
                        "urn:uuid:00000000-0000-0000-0000-00000000000");

                // atom:title
                XmlUtils.Write(writer, XmlConstants.PrefixAtom, XmlConstants.NamespaceAtom, XmlConstants.TAG_ATOM_TITLE, GetTitle());

                // atom:updated
                XmlUtils.Write(writer, XmlConstants.PrefixAtom, XmlConstants.NamespaceAtom, XmlConstants.TAG_ATOM_UPDATED, DateTime.UtcNow);

                // content
                if (stream != null)
                {
                    writer.WriteStartElement(XmlConstants.PrefixRestAtom, XmlConstants.TAG_CONTENT, XmlConstants.NamespaceRestAtom);

                    XmlUtils.Write(writer, XmlConstants.PrefixRestAtom, XmlConstants.NamespaceRestAtom,
                            XmlConstants.TAG_CONTENT_MEDIATYPE, contentStream.MimeType);

                    if (contentStream.FileName != null)
                    {
                        XmlUtils.Write(writer, XmlConstants.PrefixApacheChemistry, XmlConstants.NamespaceApacheChemistry,
                                XmlConstants.TAG_CONTENT_FILENAME, contentStream.FileName);
                    }

                    writer.WriteStartElement(XmlConstants.PrefixRestAtom, XmlConstants.TAG_CONTENT_BASE64, XmlConstants.NamespaceRestAtom);
                    WriteContent(writer);
                    writer.WriteEndElement();

                    writer.WriteEndElement();
                }

                // object
                if (objectData != null)
                {
                    XmlConverter.WriteObject(writer, cmisVersion, XmlConstants.NamespaceRestAtom, objectData);
                }

                // type
                if (typeDef != null)
                {
                    XmlConverter.WriteTypeDefinition(writer, cmisVersion, XmlConstants.NamespaceRestAtom, typeDef);
                }

                // bulk update
                if (bulkUpdate != null)
                {
                    XmlConverter.WriteBulkUpdate(writer, XmlConstants.NamespaceRestAtom, bulkUpdate);
                }

                // end entry
                writer.WriteEndElement();

                // end document
                XmlUtils.EndXmlDocument(writer);
            }
        }

        // ---- internal ----

        private string GetTitle()
        {
            string result = "";

            if (objectData != null)
            {
                IPropertyData nameProperty = objectData.Properties[PropertyIds.Name];
                if (nameProperty != null)
                {
                    result = nameProperty.FirstValue as string;
                }
            }

            if (typeDef != null)
            {
                if (typeDef.DisplayName != null)
                {
                    result = typeDef.DisplayName;
                }
            }

            if (bulkUpdate != null)
            {
                result = "Bulk Update Properties";
            }

            return result;
        }

        private void WriteContent(XmlWriter writer)
        {
            byte[] buffer = new byte[BufferSize];

            int rb;
            while ((rb = stream.Read(buffer, 0, buffer.Length)) > 0)
            {
                writer.WriteBase64(buffer, 0, rb);
            }
        }
    }




    /// <summary>
    /// HttpContent class for streaming AtomPub content.
    /// </summary>
    internal class AtomPubHttpContent : HttpContent
    {
        private readonly Action<Stream> writeAction;

        public AtomPubHttpContent(string contentType, Action<Stream> writeAction)
            : base()
        {
            MediaTypeHeaderValue contentTypeHeader = MediaTypeHeaderValue.Parse(contentType);
            contentTypeHeader.CharSet = Encoding.UTF8.WebName;

            this.Headers.ContentType = contentTypeHeader;
            this.writeAction = writeAction;
        }

        protected override async Task SerializeToStreamAsync(Stream stream, TransportContext context)
        {
            await Task.Run(() => writeAction(stream));
        }

        protected override bool TryComputeLength(out long length)
        {
            length = -1;
            return false;
        }
    }
}
