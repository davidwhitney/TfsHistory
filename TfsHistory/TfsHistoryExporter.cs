using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Microsoft.TeamFoundation.Client;
using Microsoft.TeamFoundation.VersionControl.Client;

namespace TfsHistory
{
    public class TfsHistoryExporter
    {
        public string TfsServer { get; set; }

        public VersionSpec VersionStart { get; set; }
        public VersionSpec VersionTo { get; set; }

        public TfsHistoryExporter(string tfsServer)
        {
            TfsServer = tfsServer;

            VersionStart = VersionSpec.ParseSingleSpec("1", "");
            VersionTo = VersionSpec.Latest;
        }

        public IEnumerable<Changeset> RetrieveAllHistory(string tfsPath)
        {
            return RetrieveHistoryBetweenChangesets(tfsPath, VersionStart, VersionTo);
        }

        public IEnumerable<Changeset> RetrieveHistoryStartingAtChangeset(string tfsPath, string versionStart)
        {
            if (string.IsNullOrEmpty(versionStart))
            {
                throw new ArgumentNullException("versionStart");
            }

            return RetrieveHistoryBetweenChangesets(tfsPath, ParseVersionNumber(versionStart), VersionTo);
        }

        public IEnumerable<Changeset> RetrieveHistoryBetweenChangesets(string tfsPath, string versionStart, string versionTo)
        {
            if(string.IsNullOrEmpty(versionStart))
            {
                throw new ArgumentNullException("versionStart");
            }
            if (string.IsNullOrEmpty(versionTo))
            {
                throw new ArgumentNullException("versionTo");
            }

            return RetrieveHistoryBetweenChangesets(tfsPath, ParseVersionNumber(versionStart), ParseVersionNumber(versionTo));
        }

        public IEnumerable<Changeset> RetrieveHistoryBetweenChangesets(string tfsPath, VersionSpec versionStart, VersionSpec versionTo)
        {
            var changesets = GetTfsChangesets(tfsPath, versionStart, versionTo);
            changesets.OrderBy(change => change.FirstWorkItem()).Reverse().ToList();
            return changesets;
        }

        public Dictionary<string, ChangeType> RetrieveModifiedFilesStartingAtChangeset(string tfsPath, string versionStart)
        {
            if (string.IsNullOrEmpty(versionStart))
            {
                throw new ArgumentNullException("versionStart");
            }

            return RetrieveModifiedFilesBetweenVersions(tfsPath, ParseVersionNumber(versionStart), VersionTo);
        }

        public Dictionary<string, ChangeType> RetrieveModifiedFilesBetweenVersions(string tfsPath, string versionStart, string versionTo)
        {
            if (string.IsNullOrEmpty(versionStart))
            {
                throw new ArgumentNullException("versionStart");
            }
            if (string.IsNullOrEmpty(versionTo))
            {
                throw new ArgumentNullException("versionTo");
            }

            return RetrieveModifiedFilesBetweenVersions(tfsPath, ParseVersionNumber(versionStart), ParseVersionNumber(versionTo));
        }


        public Dictionary<string, ChangeType> RetrieveModifiedFilesBetweenVersions(string tfsPath, VersionSpec versionStart, VersionSpec versionTo)
        {
            var changesets = GetTfsChangesets(tfsPath, versionStart, versionTo);
            
            changesets.OrderBy(changeset => changeset.CreationDate);
            
            var changedFiles = new Dictionary<string, ChangeType>();

            foreach (var change in changesets.SelectMany(changeset => changeset.Changes))
            {
                if (changedFiles.ContainsKey(change.Item.ServerItem))
                {
                    if (change.ChangeType == ChangeType.Delete)
                    {
                        changedFiles.Remove(change.Item.ServerItem);
                    }
                    else
                    {
                        changedFiles.Remove(change.Item.ServerItem);
                        
                        if (!change.ChangeType.ToString().Contains("Branch")
                            && !change.ChangeType.ToString().Contains("Merge"))
                        {
                            changedFiles.Add(change.Item.ServerItem, change.ChangeType);
                        }
                    }
                }
                else
                {
                    if (!change.ChangeType.ToString().Contains("Branch")
                        && !change.ChangeType.ToString().Contains("Merge"))
                    {
                        changedFiles.Add(change.Item.ServerItem, change.ChangeType);
                    }
                }
            }

            changedFiles = changedFiles.OrderBy(kvp => kvp.Key).ToDictionary(k => k.Key, v => v.Value);
            
            return changedFiles;
        }

        public Dictionary<string, ChangeType> RetrieveAllModifiedFilesForPath(string tfsPath)
        {
            return RetrieveModifiedFilesBetweenVersions(tfsPath, VersionStart, VersionTo);
        }

        private IEnumerable<Changeset> GetTfsChangesets(string tfsPath, VersionSpec versionStart, VersionSpec versionTo)
        {
            IEnumerable history = GetTfsHistory(tfsPath, versionStart, versionTo);

            return history.OfType<Changeset>().ToList();
        }

        private IEnumerable GetTfsHistory(string tfsPath, VersionSpec versionStart, VersionSpec versionTo)
        {
            var tfs = TeamFoundationServerFactory.GetServer(TfsServer);
            var vcs = (VersionControlServer)tfs.GetService(typeof(VersionControlServer));

            return vcs.QueryHistory(tfsPath, VersionSpec.Latest, 0, RecursionType.Full, "", versionStart,
                                    versionTo, Int32.MaxValue, true, false);
        }

        private static VersionSpec ParseVersionNumber(string version)
        {
            return VersionSpec.ParseSingleSpec(version, "");   
        }
    }
}
