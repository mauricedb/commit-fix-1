using System;
using CommandLine;

namespace SBM_CommitFix
{
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
}