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
using PortCMIS.Enums;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using System.Text;

namespace PortCMIS.Client
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
            get
            {
                if (cacheKey == null)
                {
                    GenerateCacheKey();
                }
                return cacheKey;
            }
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

        /// <inheritdoc/>
        public override string ToString()
        {
            return Id;
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }

        /// <inheritdoc/>
        public override bool Equals(object obj)
        {
            return this.Equals(obj as ObjectId);
        }

        /// <inheritdoc/>
        public bool Equals(ObjectId oid)
        {
            if (Object.ReferenceEquals(oid, null))
            {
                return false;
            }

            if (Object.ReferenceEquals(this, oid))
            {
                return true;
            }

            if (this.GetType() != oid.GetType())
            {
                return false;
            }

            return Id == oid.Id;
        }

        /// <inheritdoc/>
        public static bool operator ==(ObjectId id1, ObjectId id2)
        {
            if (object.ReferenceEquals(id1, null))
            {
                return object.ReferenceEquals(id2, null);
            }

            return id1.Id == id2.Id;
        }

        /// <inheritdoc/>
        public static bool operator !=(ObjectId id1, ObjectId id2)
        {
            return !(id1 == id2);
        }
    }

    /// <summary>
    /// Tree implementation.
    /// </summary>
    internal class Tree<T> : ITree<T>
    {
        /// <inheritdoc/>
        public T Item { get; set; }

        /// <inheritdoc/>
        public IList<ITree<T>> Children { get; set; }
    }

    /// <summary>
    /// Base class for IItemEnumerable's.
    /// </summary>
    internal abstract class AbstractEnumerable<T> : IItemEnumerable<T>
    {
        private AbstractEnumerator<T> enumerator;

        /// <value>
        /// Gets the enumerator to creates one if it hasn't been set up, yet. 
        /// </value>
        protected AbstractEnumerator<T> Enumerator
        {
            get
            {
                if (enumerator == null) { enumerator = CreateEnumerator(); }
                return enumerator;
            }
        }

        /// <value>
        /// Gets the delegate that fetches a page.
        /// </value>
        protected PageFetcher<T> PageFetcher { get; set; }

        /// <value>
        /// Gets the skip count.
        /// </value>
        protected BigInteger SkipCount { get; private set; }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="pageFetcher">>the delegate that fetches a page</param>
        public AbstractEnumerable(PageFetcher<T> pageFetcher) :
            this(0, pageFetcher) { }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="position">the skip count</param>
        /// <param name="pageFetcher">>the delegate that fetches a page</param>
        protected AbstractEnumerable(BigInteger position, PageFetcher<T> pageFetcher)
        {
            this.PageFetcher = pageFetcher;
            this.SkipCount = position;
        }

        /// <summary>
        /// Creates an enumerator.
        /// </summary>
        protected abstract AbstractEnumerator<T> CreateEnumerator();

        /// <inheritdoc/>
        IEnumerator<T> IEnumerable<T>.GetEnumerator()
        {
            return GetEnumerator();
        }

        /// <inheritdoc/>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        /// <inheritdoc/>
        public IEnumerator<T> GetEnumerator()
        {
            return Enumerator;
        }

        /// <inheritdoc/>
        public IItemEnumerable<T> SkipTo(BigInteger position)
        {
            return new CollectionEnumerable<T>(position, PageFetcher);
        }

        /// <inheritdoc/>
        public IItemEnumerable<T> GetPage()
        {
            return new CollectionPageEnumerable<T>(SkipCount, PageFetcher);
        }

        /// <inheritdoc/>
        public IItemEnumerable<T> GetPage(int maxNumItems)
        {
            PageFetcher.MaxNumItems = maxNumItems;
            return new CollectionPageEnumerable<T>(SkipCount, PageFetcher);
        }

        /// <inheritdoc/>
        public BigInteger PageNumItems { get { return Enumerator.PageNumItems; } }

        /// <inheritdoc/>
        public bool HasMoreItems { get { return Enumerator.HasMoreItems; } }

        /// <inheritdoc/>
        public BigInteger TotalNumItems { get { return Enumerator.TotalNumItems; } }
    }

    /// <summary>
    /// Abstract Enumerator implementation.
    /// </summary>
    internal abstract class AbstractEnumerator<T> : IEnumerator<T>
    {
        private PageFetcher<T> pageFetcher;
        private PageFetcher<T>.Page<T> page = null;
        private BigInteger? totalNumItems = null;
        private bool? hasMoreItems = null;

        /// <summary>
        /// The current element.
        /// </summary>
        protected T current;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="skipCount">the skip count</param>
        /// <param name="pageFetcher">the delegate that fetches a page</param>
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

        /// <values>
        /// Gets the skip count.
        /// </values>
        public BigInteger SkipCount { get; protected set; }

        /// <values>
        /// Gets the skip offset.
        /// </values>
        public int SkipOffset { get; protected set; }

        /// <values>
        /// Gets the current position.
        /// </values>
        public BigInteger Position { get { return SkipCount + SkipOffset; } }

        /// <values>
        /// Gets the number of items of the current page.
        /// </values>
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

        /// <values>
        /// Gets the total number of items.
        /// </values>
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

        /// <values>
        /// Gets whether there are more items or not.
        /// </values>
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

        /// <summary>
        /// Increments the skip offset.
        /// </summary>
        /// <returns>the new offset</returns>
        protected int IncrementSkipOffset()
        {
            return SkipOffset++;
        }

        /// <summary>
        /// Returns the current page.
        /// </summary>
        /// <returns></returns>
        protected PageFetcher<T>.Page<T> GetCurrentPage()
        {
            if (page == null)
            {
                page = pageFetcher.FetchNextPage(SkipCount);
            }
            return page;
        }

        /// <summary>
        /// Fetches the next page.
        /// </summary>
        /// <returns>the next page</returns>
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
    internal class PageFetcher<T>
    {
        /// <summary>
        /// A delegate that fetches a page.
        /// </summary>
        /// <param name="maxNumItems">max number of items</param>
        /// <param name="skipCount">the skip count</param>
        /// <returns>a page</returns>
        public delegate Page<T> FetchPage(BigInteger maxNumItems, BigInteger skipCount);

        private FetchPage fetchPageDelegate;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="maxNumItems">max number of items</param>
        /// <param name="fetchPageDelegate">the delegate that fetches a page</param>
        public PageFetcher(BigInteger maxNumItems, FetchPage fetchPageDelegate)
        {
            MaxNumItems = maxNumItems;
            this.fetchPageDelegate = fetchPageDelegate;
        }

        /// <value>
        /// Gets the max number of items.
        /// </value>
        public BigInteger MaxNumItems { get; set; }

        /// <summary>
        /// Fetches the next page.
        /// </summary>
        /// <param name="skipCount">the skip count</param>
        /// <returns>the next page</returns>
        public Page<T> FetchNextPage(BigInteger skipCount)
        {
            return fetchPageDelegate(MaxNumItems, skipCount);
        }

        /// <summary>
        /// A page.
        /// </summary>
        public class Page<P>
        {
            /// <summary>
            /// Constructor.
            /// </summary>
            /// <param name="items">list of items</param>
            /// <param name="totalNumItems">total number of items, if known</param>
            /// <param name="hasMoreItems">a flag whether there are more items, if known</param>
            public Page(IList<P> items, BigInteger? totalNumItems, bool? hasMoreItems)
            {
                Items = items;
                TotalNumItems = totalNumItems;
                HasMoreItems = hasMoreItems;
            }

            /// <values>
            /// Gets the items of the page.
            /// </values>
            public IList<P> Items { get; private set; }

            /// <values>
            /// Gets the total number of items, if known.
            /// </values>
            public BigInteger? TotalNumItems { get; private set; }

            /// <values>
            /// Gets whether there are more items or not, if known.
            /// </values>
            public bool? HasMoreItems { get; private set; }
        }
    }

    /// <summary>
    /// CMIS Collection Enumerable.
    /// </summary>
    internal class CollectionEnumerable<T> : AbstractEnumerable<T>
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="pageFetcher">the delegate that fetches a page</param>
        public CollectionEnumerable(PageFetcher<T> pageFetcher) :
            this(0, pageFetcher) { }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="position">the position</param>
        /// <param name="pageFetcher">the delegate that fetches a page</param>
        public CollectionEnumerable(BigInteger position, PageFetcher<T> pageFetcher) :
            base(position, pageFetcher) { }

        /// <inheritdoc/>
        protected override AbstractEnumerator<T> CreateEnumerator()
        {
            return new CollectionEnumerator<T>(SkipCount, PageFetcher);
        }
    }

    /// <summary>
    /// Enumerator for iterating over all items in a CMIS Collection.
    /// </summary>
    internal class CollectionEnumerator<T> : AbstractEnumerator<T>
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="skipCount">the skip count</param>
        /// <param name="pageFetcher">the delegate that fetches a page</param>
        public CollectionEnumerator(BigInteger skipCount, PageFetcher<T> pageFetcher) :
            base(skipCount, pageFetcher) { }

        /// <summary>
        /// Move to the next items.
        /// </summary>
        /// <returns><c>true</c> if there is a next item, <c>false</c> otherwise</returns>
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
    internal class CollectionPageEnumerable<T> : AbstractEnumerable<T>
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="pageFetcher">the delegate that fetches a page</param>
        public CollectionPageEnumerable(PageFetcher<T> pageFetcher) :
            this(0, pageFetcher) { }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="position">the position</param>
        /// <param name="pageFetcher">the delegate that fetches a page</param>
        public CollectionPageEnumerable(BigInteger position, PageFetcher<T> pageFetcher) :
            base(position, pageFetcher) { }

        /// <inheritdoc/>
        protected override AbstractEnumerator<T> CreateEnumerator()
        {
            return new CollectionPageEnumerator<T>(SkipCount, PageFetcher);
        }
    }

    /// <summary>
    /// Enumerator for iterating over a page of items in a CMIS Collection.
    /// </summary>
    internal class CollectionPageEnumerator<T> : AbstractEnumerator<T>
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="skipCount">the skip count</param>
        /// <param name="pageFetcher">the delegate that fetches a page</param>
        public CollectionPageEnumerator(BigInteger skipCount, PageFetcher<T> pageFetcher) :
            base(skipCount, pageFetcher) { }

        /// <summary>
        /// Move to the next items.
        /// </summary>
        /// <returns><c>true</c> if there is a next item, <c>false</c> otherwise</returns>
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

    /// <summary>
    /// Content Stream helpers.
    /// </summary>
    public class ContentStreamUtils
    {
        /// <summary>Octet Stream MIME type.</summary>
        private const string OctetStream = "application/octet-stream";

        private ContentStreamUtils()
        {
        }

        /// <summary>
        /// Creates a content stream object.
        /// </summary>
        /// <param name="filename">the filename</param>
        /// <param name="length">the length</param>
        /// <param name="mimetype">the MIME type</param>
        /// <param name="stream">the stream</param>
        /// <returns>the content stream</returns>
        public static IContentStream CreateContentStream(string filename, BigInteger? length, string mimetype, Stream stream)
        {
            return new ContentStream()
            {
                FileName = CheckFilename(filename),
                Length = length,
                MimeType = CheckMimeType(mimetype),
                Stream = stream
            };
        }

        // --- byte arrays ---

        /// <summary>
        /// Creates a content stream object from a byte array.
        /// </summary>
        /// <param name="filename">the filename</param>
        /// <param name="contentBytes">the byte array</param>
        /// <param name="mimetype">the MIME type</param>
        /// <returns>the content stream</returns>
        public static IContentStream CreateByteArrayContentStream(string filename, byte[] contentBytes, string mimetype)
        {
            if (contentBytes == null)
            {
                return CreateContentStream(filename, null, mimetype, null);
            }

            return CreateByteArrayContentStream(filename, contentBytes, 0, contentBytes.Length, mimetype);
        }

        /// <summary>
        /// Creates a content stream object from a byte array.
        /// </summary>
        /// <param name="filename">the filename</param>
        /// <param name="contentBytes">the byte array</param>
        /// <param name="index">the begin of the stream in the byte array</param>
        /// <param name="count">the length of the stream</param>
        /// <param name="mimetype">the MIME type</param>
        /// <returns>the content stream</returns>
        public static IContentStream CreateByteArrayContentStream(string filename, byte[] contentBytes, int index, int count, string mimetype)
        {
            if (contentBytes == null)
            {
                return CreateContentStream(filename, null, mimetype, null);
            }

            if (index < 0 || index > contentBytes.Length)
            {
                throw new ArgumentOutOfRangeException("index");
            }
            else if (count < 0 || (index + count) > contentBytes.Length || (index + count) < 0)
            {
                throw new ArgumentOutOfRangeException("count");
            }

            return CreateContentStream(filename, count, mimetype, new MemoryStream(contentBytes, index, count));
        }

        // --- strings ---

        /// <summary>
        /// Creates a content stream object from a string.
        /// </summary>
        /// <param name="filename">the filename</param>
        /// <param name="content">the content</param>
        /// <returns>the content stream</returns>
        public static IContentStream CreateTextContentStream(string filename, string content)
        {
            return CreateTextContentStream(filename, content, "text/plain; charset=UTF-8");
        }

        /// <summary>
        /// Creates a content stream object from a string.
        /// </summary>
        /// <param name="filename">the filename</param>
        /// <param name="content">the content</param>
        /// <param name="mimetype">the MIME type</param>
        /// <returns>the content stream</returns>
        public static IContentStream CreateTextContentStream(string filename, string content, string mimetype)
        {
            byte[] contentBytes = Encoding.UTF8.GetBytes(content);
            return CreateByteArrayContentStream(filename, contentBytes, CheckMimeType(mimetype));
        }

        // ---

        private static string CheckFilename(string filename)
        {
            if (filename == null || filename.Length == 0)
            {
                return "content";
            }

            return filename;
        }

        private static string CheckMimeType(string mimetype)
        {
            if (mimetype == null)
            {
                return OctetStream;
            }

            string result = mimetype.Trim();
            if (result.Length < 3)
            {
                return OctetStream;
            }

            return result;
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
