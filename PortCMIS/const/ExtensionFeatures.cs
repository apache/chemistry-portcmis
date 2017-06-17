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

namespace PortCMIS.Const
{
    /// <summary>
    /// CMIS extensions defined by the CMIS TC.
    /// </summary>
    public class ExtensionFeatures
    {
        /// <summary>
        /// DateTime format extension for the Browers Bindings.
        /// </summary>
        public readonly static IExtensionFeature ExtendedDatetimeFormat = new ExtensionFeature()
        {
            Id = "http://docs.oasis-open.org/ns/cmis/extension/datetimeformat",
            Url = "https://www.oasis-open.org/committees/tc_home.php?wg_abbrev=cmis",
            CommonName = "Browser Binding DateTime Format",
            VersionLabel = "1.0",
            Description = "Adds an additional DateTime format for the Browser Binding."
        };

        /// <summary>
        /// Content Stream Hash property extension.
        /// </summary>
        public readonly static IExtensionFeature ContentStreamHash = new ExtensionFeature()
        {
            Id = "http://docs.oasis-open.org/ns/cmis/extension/contentstreamhash",
            Url = "https://www.oasis-open.org/committees/tc_home.php?wg_abbrev=cmis",
            CommonName = "Content Stream Hash",
            VersionLabel = "1.0",
            Description = "Adds the property cmis:contentStreamHash, which represents the hash of the document content."
        };

        /// <summary>
        /// Latest Accessible State extension.
        /// </summary>
        public readonly static IExtensionFeature LatestAccessibleState = new ExtensionFeature()
        {
            Id = "http://docs.oasis-open.org/ns/cmis/extension/latestAccessibleState/1.1",
            Url = "https://www.oasis-open.org/committees/tc_home.php?wg_abbrev=cmis",
            CommonName = "Latest Accessible State",
            VersionLabel = "1.1",
            Description = "This extension provides for an identifier of each cmis:document that retrieves "
                    + "the latest accessible state of the document whether the document is versioned or not."
        };
    }
}
