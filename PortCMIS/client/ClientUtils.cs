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

using PortCMIS.Enums;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace PortCMIS.Client.Impl
{
    /// <summary>
    /// Operation context implementation.
    /// </summary>
    public class OperationContext : IOperationContext
    {
        /// <summary>
        /// Property selector for all properties ('*').
        /// </summary>
        public const string PropertiesStar = "*";

        /// <summary>
        /// Rendition constant for 'no rendition' ('cmis:none').
        /// </summary>
        public const string RenditionNone = "cmis:none";

        private ISet<string> filter;
        private bool includeAllowableActions;
        private bool includeAcls;
        private IncludeRelationships? includeRelationships;
        private bool includePolicies;
        private ISet<string> renditionFilter;
        private bool includePathSegments;
        private string orderBy;
        private bool cacheEnabled;
        private string cacheKey;
        private int maxItemsPerPage;

        /// <summary>
        /// Constructor with default values.
        /// </summary>
        public OperationContext()
        {
            filter = null;
            includeAcls = false;
            includeAllowableActions = true;
            includePolicies = false;
            includeRelationships = PortCMIS.Enums.IncludeRelationships.None;
            renditionFilter = null;
            includePathSegments = true;
            orderBy = null;
            cacheEnabled = false;
            maxItemsPerPage = 100;

            GenerateCacheKey();
        }

        /// <summary>
        /// Copy constructor.
        /// </summary>
        public OperationContext(IOperationContext source)
        {
            filter = (source.Filter == null ? null : new HashSet<string>(source.Filter));
            includeAcls = source.IncludeAcls;
            includeAllowableActions = source.IncludeAllowableActions;
            includePolicies = source.IncludePolicies;
            includeRelationships = source.IncludeRelationships;
            renditionFilter = (source.RenditionFilter == null ? null : new HashSet<string>(source.RenditionFilter));
            includePathSegments = source.IncludePathSegments;
            orderBy = source.OrderBy;
            cacheEnabled = source.CacheEnabled;
            maxItemsPerPage = source.MaxItemsPerPage;

            GenerateCacheKey();
        }

        /// <summary>
        /// Constructor with given values.
        /// </summary>
        public OperationContext(ISet<string> filter, bool includeAcls, bool includeAllowableActions,
            bool includePolicies, IncludeRelationships includeRelationships, ISet<string> renditionFilter,
            bool includePathSegments, string orderBy, bool cacheEnabled, int maxItemsPerPage)
        {
            this.filter = filter;
            this.includeAcls = includeAcls;
            this.includeAllowableActions = includeAllowableActions;
            this.includePolicies = includePolicies;
            this.includeRelationships = includeRelationships;
            this.renditionFilter = renditionFilter;
            this.includePathSegments = includePathSegments;
            this.orderBy = orderBy;
            this.cacheEnabled = cacheEnabled;
            this.maxItemsPerPage = maxItemsPerPage;

            GenerateCacheKey();
        }

        /// <inheritdoc/>
        public virtual ISet<string> Filter
        {
            get { return filter == null ? null : new HashSet<string>(filter); }
            set
            {
                if (value != null)
                {
                    HashSet<string> tempSet = new HashSet<string>();
                    foreach (string oid in value)
                    {
                        if (oid == null) { continue; }

                        string toid = oid.Trim();
                        if (toid.Length == 0) { continue; }
                        if (toid == PropertiesStar)
                        {
                            tempSet = new HashSet<string>();
                            tempSet.Add(PropertiesStar);
                            break;
                        }
                        if (toid.IndexOf(',') > -1)
                        {
                            throw new ArgumentException("Query ID must not contain a comma!");
                        }

                        tempSet.Add(toid);
                    }

                    if (tempSet.Count == 0) { filter = null; }
                    else { filter = tempSet; }
                }
                else
                {
                    filter = null;
                }

                GenerateCacheKey();
            }
        }

        /// <inheritdoc/>
        public virtual string FilterString
        {
            get
            {
                if (filter == null) { return null; }

                if (filter.Contains(PropertiesStar))
                {
                    return PropertiesStar;
                }

                this.filter.Add(PropertyIds.ObjectId);
                this.filter.Add(PropertyIds.BaseTypeId);
                this.filter.Add(PropertyIds.ObjectTypeId);

                StringBuilder sb = new StringBuilder();

                foreach (String oid in filter)
                {
                    if (sb.Length > 0) { sb.Append(','); }
                    sb.Append(oid);
                }

                return sb.ToString();
            }

            set
            {
                if (value == null || value.Trim().Length == 0)
                {
                    Filter = null;
                    return;
                }

                string[] ids = value.Split(',');
                HashSet<string> tempSet = new HashSet<string>();
                foreach (string qid in ids)
                {
                    tempSet.Add(qid);
                }

                Filter = tempSet;
            }
        }

        /// <inheritdoc/>
        public virtual bool IncludeAllowableActions
        {
            get { return includeAllowableActions; }
            set { includeAllowableActions = value; GenerateCacheKey(); }
        }

        /// <inheritdoc/>
        public virtual bool IncludeAcls
        {
            get { return includeAcls; }
            set { includeAcls = value; GenerateCacheKey(); }
        }

        /// <inheritdoc/>
        public virtual IncludeRelationships? IncludeRelationships
        {
            get { return includeRelationships; }
            set { includeRelationships = value; GenerateCacheKey(); }
        }

        /// <inheritdoc/>
        public virtual bool IncludePolicies
        {
            get { return includePolicies; }
            set { includePolicies = value; GenerateCacheKey(); }
        }

        /// <inheritdoc/>
        public virtual ISet<string> RenditionFilter
        {
            get { return renditionFilter == null ? null : new HashSet<string>(renditionFilter); }
            set
            {
                HashSet<string> tempSet = new HashSet<string>();
                if (value != null)
                {
                    foreach (String rf in value)
                    {
                        if (rf == null) { continue; }

                        String trf = rf.Trim();
                        if (trf.Length == 0) { continue; }
                        if (trf.IndexOf(',') > -1)
                        {
                            throw new ArgumentException("Rendition must not contain a comma!");
                        }

                        tempSet.Add(trf);
                    }

                    if (tempSet.Count == 0)
                    {
                        tempSet.Add(RenditionNone);
                    }
                }
                else
                {
                    tempSet.Add(RenditionNone);
                }

                renditionFilter = tempSet;

                GenerateCacheKey();
            }
        }

        /// <inheritdoc/>
        public virtual string RenditionFilterString
        {
            get
            {
                if (renditionFilter == null) { return null; }

                StringBuilder sb = new StringBuilder();
                foreach (string rf in renditionFilter)
                {
                    if (sb.Length > 0) { sb.Append(','); }
                    sb.Append(rf);
                }

                return sb.ToString();
            }

            set
            {
                if (value == null || value.Trim().Length == 0)
                {
                    RenditionFilter = null;
                    return;
                }

                string[] renditions = value.Split(',');
                HashSet<string> tempSet = new HashSet<string>();
                foreach (string rend in renditions)
                {
                    tempSet.Add(rend);
                }

                RenditionFilter = tempSet;
            }
        }

        /// <inheritdoc/>
        public virtual bool IncludePathSegments
        {
            get { return includePathSegments; }
            set { includePathSegments = value; GenerateCacheKey(); }
        }

        /// <inheritdoc/>
        public virtual string OrderBy
        {
            get { return orderBy; }
            set { orderBy = value; GenerateCacheKey(); }
        }

        /// <inheritdoc/>
        public virtual bool CacheEnabled
        {
            get { return cacheEnabled; }
            set { cacheEnabled = value; GenerateCacheKey(); }
        }

        /// <inheritdoc/>
        public virtual string CacheKey
        {
            get { return cacheKey; }
        }

        /// <inheritdoc/>
        public virtual int MaxItemsPerPage
        {
            get { return maxItemsPerPage; }
            set { maxItemsPerPage = value; }
        }

        /// <summary>
        /// Generates a cache key from the current state of the operation context.
        /// </summary>
        protected virtual void GenerateCacheKey()
        {
            if (!cacheEnabled)
            {
                cacheKey = null;
            }
            else
            {
                StringBuilder sb = new StringBuilder();

                sb.Append(includeAcls ? "1" : "0");
                sb.Append(includeAllowableActions ? "1" : "0");
                sb.Append(includePolicies ? "1" : "0");
                sb.Append("|");
                sb.Append(filter == null ? "" : FilterString);
                sb.Append("|");
                sb.Append(includeRelationships == null ? "" : includeRelationships.GetCmisValue());
                sb.Append("|");
                sb.Append(renditionFilter == null ? "" : RenditionFilterString);

                cacheKey = sb.ToString();
            }
        }
    }

    /// <summary>
    /// Operation Context helpers.
    /// </summary>
    public class OperationContextUtils
    {
        private OperationContextUtils()
        {
        }

        /// <summary>
        /// Creates a new operation context object.
        /// </summary>
        public static IOperationContext CreateOperationContext()
        {
            return new OperationContext();
        }

        /// <summary>
        /// Copies an operation context object.
        /// </summary>
        public static IOperationContext CopyOperationContext(OperationContext context)
        {
            return new OperationContext(context);
        }

        /// <summary>
        /// Creates a new operation context object with the given parameters.
        /// </summary>
        /// <remarks>
        /// Caching is enabled.
        /// </remarks>
        public static IOperationContext CreateOperationContext(HashSet<string> filter, bool includeAcls,
                bool includeAllowableActions, bool includePolicies, IncludeRelationships includeRelationships,
                HashSet<string> renditionFilter, bool includePathSegments, string orderBy, bool cacheEnabled,
                int maxItemsPerPage)
        {
            return new OperationContext(filter, includeAcls, includeAllowableActions, includePolicies,
                    includeRelationships, renditionFilter, includePathSegments, orderBy, cacheEnabled, maxItemsPerPage);
        }

        /// <summary>
        /// Creates a new operation context object that only selects the bare minimum.
        /// </summary>
        /// <remarks>
        /// Caching is enabled.
        /// </remarks>
        public static IOperationContext CreateMinimumOperationContext()
        {
            return CreateMinimumOperationContext((string[])null);
        }

        /// <summary>
        /// Creates a new operation context object that only selects the bare minimum plus the provided properties.
        /// </summary>
        /// <remarks>
        /// Caching is enabled.
        /// </remarks>
        public static IOperationContext CreateMinimumOperationContext(params string[] property)
        {
            ISet<string> filter = new HashSet<string>();
            filter.Add(PropertyIds.ObjectId);
            filter.Add(PropertyIds.ObjectTypeId);
            filter.Add(PropertyIds.BaseTypeId);

            if (property != null)
            {
                foreach (string prop in property)
                {
                    filter.Add(prop);
                }
            }

            return new OperationContext(filter, false, false, false, IncludeRelationships.None,
                new HashSet<string>() { OperationContext.RenditionNone }, false, null, true, 100);
        }

        /// <summary>
        /// Creates a new operation context object that selects everything.
        /// </summary>
        /// <remarks>
        /// Caching is enabled.
        /// </remarks>
        public static IOperationContext CreateMaximumOperationContext()
        {
            return new OperationContext(new HashSet<string>() { OperationContext.PropertiesStar }, true,
                true, true, IncludeRelationships.Both, new HashSet<string>() { "*" }, false, null, true, 100);
        }

        /// <summary>
        /// Returns an unmodifiable view of the specified operation context.
        /// </summary>
        public static IOperationContext CreateReadOnlyOperationContext(IOperationContext context)
        {
            return new ReadOnlyOperationContext(context);
        }

        internal class ReadOnlyOperationContext : OperationContext
        {
            public ReadOnlyOperationContext(IOperationContext originalContext) : base(originalContext) { }

            public override ISet<string> Filter
            {
                get { return base.Filter == null ? null : new HashSet<string>(base.Filter); }
                set { throw new Exception("Not supported!"); }
            }

            public override string FilterString
            {
                get { return base.FilterString; }
                set { throw new Exception("Not supported!"); }
            }

            public override bool IncludeAllowableActions
            {
                get { return base.IncludeAllowableActions; }
                set { throw new Exception("Not supported!"); }
            }

            public override bool IncludeAcls
            {
                get { return base.IncludeAcls; }
                set { throw new Exception("Not supported!"); }
            }

            public override IncludeRelationships? IncludeRelationships
            {
                get { return base.IncludeRelationships; }
                set { throw new Exception("Not supported!"); }
            }

            public override bool IncludePolicies
            {
                get { return base.IncludePolicies; }
                set { throw new Exception("Not supported!"); }
            }

            public override ISet<string> RenditionFilter
            {
                get { return base.RenditionFilter == null ? null : new HashSet<string>(base.RenditionFilter); }
                set { throw new Exception("Not supported!"); }
            }

            public override string RenditionFilterString
            {
                get { return base.RenditionFilterString; }
                set { throw new Exception("Not supported!"); }
            }

            public override bool IncludePathSegments
            {
                get { return base.IncludePathSegments; }
                set { throw new Exception("Not supported!"); }
            }

            public override string OrderBy
            {
                get { return base.OrderBy; }
                set { throw new Exception("Not supported!"); }
            }

            public override bool CacheEnabled
            {
                get { return base.CacheEnabled; }
                set { throw new Exception("Not supported!"); }
            }

            public override int MaxItemsPerPage
            {
                get { return base.MaxItemsPerPage; }
                set { throw new Exception("Not supported!"); }
            }
        }
    }


    /// <summary>
    /// Object ID implementation.
    /// </summary>
    public class ObjectId : IObjectId
    {
        private string id;

        /// <inheritdoc/>
        public string Id
        {
            get { return id; }
            set
            {
                if (value == null || value.Length == 0)
                {
                    throw new ArgumentException("ID must be set!");
                }

                id = value;
            }
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="id">the object ID as a string</param>
        public ObjectId(string id)
        {
            Id = id;
        }
    }

    /// <summary>
    /// Tree implementation.
    /// </summary>
    public class Tree<T> : ITree<T>
    {
        /// <inheritdoc/>
        public T Item { get; set; }

        /// <inheritdoc/>
        public IList<ITree<T>> Children { get; set; }
    }

    /// <summary>
    /// Base class for IItemEnumerable's.
    /// </summary>
    public abstract class AbstractEnumerable<T> : IItemEnumerable<T>
    {
        private AbstractEnumerator<T> enumerator;
        protected AbstractEnumerator<T> Enumerator
        {
            get
            {
                if (enumerator == null) { enumerator = CreateEnumerator(); }
                return enumerator;
            }
        }

        protected PageFetcher<T> PageFetcher { get; set; }
        protected BigInteger SkipCount { get; private set; }

        public AbstractEnumerable(PageFetcher<T> pageFetcher) :
            this(0, pageFetcher) { }

        protected AbstractEnumerable(BigInteger position, PageFetcher<T> pageFetcher)
        {
            this.PageFetcher = pageFetcher;
            this.SkipCount = position;
        }

        protected abstract AbstractEnumerator<T> CreateEnumerator();

        IEnumerator<T> IEnumerable<T>.GetEnumerator()
        {
            return GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public IEnumerator<T> GetEnumerator()
        {
            return Enumerator;
        }

        public IItemEnumerable<T> SkipTo(BigInteger position)
        {
            return new CollectionEnumerable<T>(position, PageFetcher);
        }

        public IItemEnumerable<T> GetPage()
        {
            return new CollectionPageEnumerable<T>(SkipCount, PageFetcher);
        }

        public IItemEnumerable<T> GetPage(int maxNumItems)
        {
            PageFetcher.MaxNumItems = maxNumItems;
            return new CollectionPageEnumerable<T>(SkipCount, PageFetcher);
        }

        public BigInteger PageNumItems { get { return Enumerator.PageNumItems; } }

        public bool HasMoreItems { get { return Enumerator.HasMoreItems; } }

        public BigInteger TotalNumItems { get { return Enumerator.TotalNumItems; } }
    }

    /// <summary>
    /// Abstract Enumerator implementation.
    /// </summary>
    public abstract class AbstractEnumerator<T> : IEnumerator<T>
    {
        private PageFetcher<T> pageFetcher;
        private PageFetcher<T>.Page<T> page = null;
        private BigInteger? totalNumItems = null;
        private bool? hasMoreItems = null;

        protected T current;

        public AbstractEnumerator(BigInteger skipCount, PageFetcher<T> pageFetcher)
        {
            this.SkipCount = skipCount;
            this.pageFetcher = pageFetcher;
        }

        /// <inheritdoc/>
        T IEnumerator<T>.Current { get { return Current; } }

        /// <inheritdoc/>
        object IEnumerator.Current { get { return Current; } }

        /// <inheritdoc/>
        public T Current { get { return current; } }

        /// <summary>
        /// Reset is not supported.
        /// </summary>
        public void Reset()
        {
            throw new NotSupportedException();
        }

        /// <inheritdoc/>
        public abstract bool MoveNext();

        /// <inheritdoc/>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <inheritdoc/>
        protected virtual void Dispose(bool disposing)
        {
        }

        public BigInteger SkipCount { get; protected set; }

        public int SkipOffset { get; protected set; }

        public BigInteger Position { get { return SkipCount + SkipOffset; } }

        public BigInteger PageNumItems
        {
            get
            {
                PageFetcher<T>.Page<T> page = GetCurrentPage();
                if (page != null)
                {
                    IList<T> items = page.Items;
                    if (items != null)
                    {
                        return items.Count;
                    }
                }
                return 0;
            }
        }

        public BigInteger TotalNumItems
        {
            get
            {
                if (totalNumItems == null)
                {
                    totalNumItems = -1;
                    PageFetcher<T>.Page<T> page = GetCurrentPage();
                    if (page != null)
                    {
                        totalNumItems = page.TotalNumItems;
                    }
                }
                return (BigInteger)totalNumItems;
            }
        }

        public bool HasMoreItems
        {
            get
            {
                if (hasMoreItems == null)
                {
                    hasMoreItems = false;
                    PageFetcher<T>.Page<T> page = GetCurrentPage();
                    if (page != null)
                    {
                        if (page.HasMoreItems.HasValue)
                        {
                            hasMoreItems = page.HasMoreItems;
                        }
                    }
                }
                return (bool)hasMoreItems;
            }
        }

        protected int IncrementSkipOffset()
        {
            return SkipOffset++;
        }

        protected PageFetcher<T>.Page<T> GetCurrentPage()
        {
            if (page == null)
            {
                page = pageFetcher.FetchNextPage(SkipCount);
            }
            return page;
        }

        protected PageFetcher<T>.Page<T> IncrementPage()
        {
            SkipCount += SkipOffset;
            SkipOffset = 0;
            totalNumItems = null;
            hasMoreItems = null;
            page = pageFetcher.FetchNextPage(SkipCount);
            return page;
        }
    }

    /// <summary>
    /// Page fetcher.
    /// </summary>
    public class PageFetcher<T>
    {
        public delegate Page<T> FetchPage(BigInteger maxNumItems, BigInteger skipCount);

        private FetchPage fetchPageDelegate;

        public PageFetcher(BigInteger maxNumItems, FetchPage fetchPageDelegate)
        {
            MaxNumItems = maxNumItems;
            this.fetchPageDelegate = fetchPageDelegate;
        }

        public BigInteger MaxNumItems { get; set; }

        public Page<T> FetchNextPage(BigInteger skipCount)
        {
            return fetchPageDelegate(MaxNumItems, skipCount);
        }

        public class Page<P>
        {
            public Page(IList<P> items, BigInteger? totalNumItems, bool? hasMoreItems)
            {
                Items = items;
                TotalNumItems = totalNumItems;
                HasMoreItems = hasMoreItems;
            }

            public IList<P> Items { get; private set; }
            public BigInteger? TotalNumItems { get; private set; }
            public bool? HasMoreItems { get; private set; }
        }
    }

    /// <summary>
    /// CMIS Collection Enumerable.
    /// </summary>
    public class CollectionEnumerable<T> : AbstractEnumerable<T>
    {
        public CollectionEnumerable(PageFetcher<T> pageFetcher) :
            this(0, pageFetcher) { }

        public CollectionEnumerable(BigInteger position, PageFetcher<T> pageFetcher) :
            base(position, pageFetcher) { }

        protected override AbstractEnumerator<T> CreateEnumerator()
        {
            return new CollectionEnumerator<T>(SkipCount, PageFetcher);
        }
    }

    /// <summary>
    /// Enumerator for iterating over all items in a CMIS Collection.
    /// </summary>
    public class CollectionEnumerator<T> : AbstractEnumerator<T>
    {
        public CollectionEnumerator(BigInteger skipCount, PageFetcher<T> pageFetcher) :
            base(skipCount, pageFetcher) { }

        public override bool MoveNext()
        {
            PageFetcher<T>.Page<T> page = GetCurrentPage();
            if (page == null)
            {
                return false;
            }

            IList<T> items = page.Items;
            if (items == null || items.Count == 0)
            {
                return false;
            }

            if (SkipOffset == items.Count)
            {
                if (!HasMoreItems)
                {
                    return false;
                }

                page = IncrementPage();
                items = page == null ? null : page.Items;
            }

            if (items == null || items.Count == 0 || SkipOffset == items.Count)
            {
                return false;
            }

            current = items[IncrementSkipOffset()];

            return true;
        }
    }

    /// <summary>
    /// Enumerable for a CMIS Collection Page.
    /// </summary>
    public class CollectionPageEnumerable<T> : AbstractEnumerable<T>
    {
        public CollectionPageEnumerable(PageFetcher<T> pageFetcher) :
            this(0, pageFetcher) { }

        public CollectionPageEnumerable(BigInteger position, PageFetcher<T> pageFetcher) :
            base(position, pageFetcher) { }

        protected override AbstractEnumerator<T> CreateEnumerator()
        {
            return new CollectionPageEnumerator<T>(SkipCount, PageFetcher);
        }
    }

    /// <summary>
    /// Enumerator for iterating over a page of items in a CMIS Collection.
    /// </summary>
    public class CollectionPageEnumerator<T> : AbstractEnumerator<T>
    {
        public CollectionPageEnumerator(BigInteger skipCount, PageFetcher<T> pageFetcher) :
            base(skipCount, pageFetcher) { }

        public override bool MoveNext()
        {
            PageFetcher<T>.Page<T> page = GetCurrentPage();
            if (page == null)
            {
                return false;
            }

            IList<T> items = page.Items;
            if (items == null || items.Count == 0 || SkipOffset == items.Count)
            {
                return false;
            }

            current = items[IncrementSkipOffset()];

            return true;
        }
    }

    internal class StringListBuilder
    {
        private string seperator;
        private bool first;
        public StringBuilder StringBuilder { get; private set; }

        public StringListBuilder() : this(",", new StringBuilder()) { }

        public StringListBuilder(StringBuilder stringBuilder) : this(",", stringBuilder) { }

        public StringListBuilder(string seperator) : this(seperator, new StringBuilder()) { }

        public StringListBuilder(string seperator, StringBuilder stringBuilder)
        {
            this.seperator = seperator;
            StringBuilder = stringBuilder;
            first = true;
        }

        public void Add(string s)
        {
            if (!first)
            {
                StringBuilder.Append(seperator);
            }
            else
            {
                first = false;
            }

            StringBuilder.Append(s);
        }

        override public string ToString()
        {
            return StringBuilder.ToString();
        }
    }
}
