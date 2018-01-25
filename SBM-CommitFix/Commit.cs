using System;
using LinqToDB;
using LinqToDB.Mapping;

namespace SBM_CommitFix
{
    [Table(Schema = "Foundation", Name = "Commits")]
    public class Commit
    {
        [PrimaryKey, Identity]
        public int CheckpointNumber { get; set; }

        [Column, DataType(DataType.VarBinary)]
        public byte[] Payload { get; set; }

        [Column]
        public Guid StreamIdOriginal { get; set; }

        [Column]
        public Guid? Site { get; set; }

        [Column]
        public int CommitSequence { get; set; }
    }
}