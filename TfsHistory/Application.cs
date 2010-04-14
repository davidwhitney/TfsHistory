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

            var historyExporter = new TfsHistoryExporter();
            historyExporter.VersionStart = VersionSpec.ParseSingleSpec("40800", "");
            historyExporter.TfsPath = TfsPath;
            historyExporter.TfsServer = TfsServer;
            historyExporter.OutputLocation = OutputLocation;

            var changes = historyExporter.RetrieveHistory();

            var csvEntries = changes.Select(change => BuildCsvEntry(change)).ToList();
            var csvEngine = new FileHelperEngine<CsvEntry>();
            csvEngine.WriteFile(OutputLocation, csvEntries);

        }

        private static CsvEntry BuildCsvEntry(Changeset change)
        {
            var entry = new CsvEntry();
            entry.Date = change.CreationDate;
            entry.ChangesetId = change.ChangesetId;
            entry.WorkItems = change.WorkItems.Aggregate(string.Empty, (current, workItem) => current + string.Format("{0}: {1}, ", workItem.Id, workItem.Title)).Trim().TrimEnd(',').Replace('"',' ');
            entry.Committer = change.Committer;
            entry.Comment = change.Comment.Trim().TrimEnd(',').Replace('"', ' ');
            return entry;
        }
    }
}
