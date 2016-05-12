using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PortCMIS
{
    /// <summary>
    /// PortCMIS client version.
    /// </summary>
    public class ClientVersion
    {
        /// <summary>PortCMIS user agent name</summary>
        public const string UserAgentName = "ApacheChemistryPortCMIS";

        /// <summary>PortCMIS version</summary>
        public const string Version = "0.1";

        /// <summary>PortCMIS user agent name</summary>
        public const string UserAgent = UserAgentName + "/" + Version;
    }
}