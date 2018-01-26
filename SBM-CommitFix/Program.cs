using CommandLine;

namespace SBM_CommitFix
{
    class Program
    {
        static void Main(string[] args)
        {
            Parser.Default.ParseArguments<Options>(args).WithParsed(options =>
            {
                var logger = new Logger(options);
                new FixCommits(options, logger).Execute();
            });
        }
    }
}