using System;
using FileHelpers;

namespace TfsHistory
{
    [DelimitedRecord(",")]
    public class CsvEntry
    {
        [FieldQuoted]
        [FieldConverter(ConverterKind.Date, "dd-MM-yyyy")]
        public DateTime Date;
        
        [FieldQuoted]
        public int ChangesetId;
        
        [FieldQuoted]
        public string WorkItems;
        
        [FieldQuoted]
        public string Committer;
        
        [FieldQuoted]
        public string Comment;
    }
}
