using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using FileHelpers;
using Microsoft.TeamFoundation.VersionControl.Client;
using NDesk.Options;

namespace TfsHistory
{
    public class Application
    {
        public static string OutputLocation;
        public static string TfsServer;
        public static string TfsPath;

        public static void Main(string[] args)
        {
            var parameters = new OptionSet {
   	            { "file=",      param => OutputLocation = param },
   	            { "server=",    param => TfsServer = param },
   	            { "path=",      param => TfsPath = param },
               };
            parameters.Parse(args);

            var historyExporter = new TfsHistoryExporter(TfsServer);
            
            //var changes = historyExporter.RetrieveAllHistory(TfsPath);
            var changes = historyExporter.RetrieveAllModifiedFilesForPath(TfsPath);
            var csvEntries = changes.Select(change => BuildHistoryCsvEntry(change)).ToList();

            //var csvEntries = changes.Select(change => BuildHistoryCsvEntry(change)).ToList();

            var csvEngine = new FileHelperEngine<ModifiedFilesCsvEntry>();
            csvEngine.WriteFile(OutputLocation, csvEntries);

        }

        private static ModifiedFilesCsvEntry BuildHistoryCsvEntry(KeyValuePair<string, ChangeType> modifiedFile)
        {
            var entry = new ModifiedFilesCsvEntry
                        {
                            Path = modifiedFile.Key,
                            ChangeType = modifiedFile.Value
                            
                        };
            return entry;
        }

        private static HistoryCsvEntry BuildHistoryCsvEntry(Changeset change)
        {
            var entry = new HistoryCsvEntry
                        {
                            Date = change.CreationDate,
                            ChangesetId = change.ChangesetId,
                            Committer = change.Committer,
                            Comment = change.Comment.Trim().TrimEnd(',').Replace('"', ' '),
                            WorkItems = change.WorkItems.Aggregate(string.Empty, (current, workItem) => current + string.Format("{0}: {1}, ", workItem.Id, workItem.Title)).Trim().TrimEnd(',').Replace('"', ' ')
                        };
            return entry;
        }
    }
}
