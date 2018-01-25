using LinqToDB;
using LinqToDB.Data;

namespace SBM_CommitFix
{
    public class VisionSuiteDB : DataConnection
    {
        public VisionSuiteDB(Options options) : base("SqlServer",
            $"Server={options.Server};Database={options.Database};Trusted_Connection=True;Enlist=False;")
        {
        }

        public ITable<Commit> Commits
        {
            get { return GetTable<Commit>(); }
        }
    }
}