using System.Text;
using System.Text.RegularExpressions;
using GetRelease.Sources;

namespace GetRelease;

internal class Program
{
    static int Main(string[] args)
    {
        if (args.Length == 2)
        {
            return ParseSource(args[0], args[1], false);
        }
        else if (args.Length == 3)
        {
            if (args[2] == "-v")
            {
                return ParseSource(args[0], args[1], true);
            }
            else
            {
                return ParseSource(args[0], args[1], false);
            }
        }
        else
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("Usage: GetRelease {repo} {pattern}\n");
            sb.Append("e.g.,\n");
            sb.Append("GetRelease \"https://github.com/go-acme/lego\" \"linux_amd64.tar.gz$\"");
            Console.WriteLine(sb.ToString());
        }
        return 0;
    }

    private static int ParseSource(string repo, string pattern, bool verbose)
    {
        string githubPattern = @"github.com/";
        if (Regex.Match(repo, githubPattern).Success)
        {
            string repoPattern = @"(?<=github.com/).+?/.+?(?=(/|$)+)";
            Match match = Regex.Match(repo, repoPattern);
            if (match.Success)
            {
                if (verbose)
                {
                    Console.WriteLine($"Parsing with github");
                }
                return GitHubParser.ParseRelease(match.Value, pattern, verbose);
            }
        }
        return 1;
    }
}
