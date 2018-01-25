using System;
using System.Linq;
using CommandLine;
using LinqToDB;
using LinqToDB.Data;
using LinqToDB.Mapping;

namespace SBM_CommitFix
{
    class Program
    {
        static void Main(string[] args)
        {
            Parser.Default.ParseArguments<Options>(args).WithParsed(options =>
            {
                using (var db = new VisionSuiteDB(options))
                {
                    var count = db.Commits.Count();
                    Console.WriteLine($"There are {count:N0} commits in the {options.Database} database.");
                }

            });
        }
    }


    public class Options
    {
        [Option('d', "database", HelpText = "Use database name", Required = true)]
        public string Database { get; set; }

        [Option('S', "server", HelpText = "Use server", Default = ".")]
        public string Server { get; set; }

        [Option("verbose", HelpText = "Verbose output", Default = false)]
        public bool Verbose { get; set; }

        [Option("what-if", HelpText = "Report what would happen but don't actually do it", Default = false)]
        public bool WhatIf { get; set; }

        internal Guid[] SitesToKeep;
    }

    public class VisionSuiteDB : DataConnection
    {
        public VisionSuiteDB(Options options) : base("SqlServer", $"Server={options.Server};Database={options.Database};Trusted_Connection=True;Enlist=False;") { }

        public ITable<Commit> Commits { get { return GetTable<Commit>(); } }
    }

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
