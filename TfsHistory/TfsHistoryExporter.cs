using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.TeamFoundation.Client;
using Microsoft.TeamFoundation.VersionControl.Client;

namespace TfsHistory
{
    public class TfsHistoryExporter
    {
        public string OutputLocation { get; set; }
        public string TfsServer { get; set; }
        public string TfsPath { get; set; }
        public VersionSpec VersionStart { get; set; }

        public List<Changeset> RetrieveHistory()
        {
            var tfs = TeamFoundationServerFactory.GetServer(TfsServer);
            var vcs = (VersionControlServer)tfs.GetService(typeof(VersionControlServer));

            var history = vcs.QueryHistory(TfsPath, VersionSpec.Latest, 0, RecursionType.Full, "",
                                           VersionStart, VersionSpec.Latest, Int32.MaxValue, true, false);

            var changes = history.OfType<Changeset>().ToList();
            return changes.OrderBy(change => change.FirstWorkItem()).Reverse().ToList();
        }
    }
}
