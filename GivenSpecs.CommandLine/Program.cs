using CommandLine;
using GivenSpecs.CommandLine.Generate;
using GlobExpressions;
using System;
using System.IO;
using System.Threading.Tasks;

namespace GivenSpecs.CommandLine
{
    [Verb("generate", HelpText = "Generate tests from feature files.")]
    public class GenerateOptions
    {
        [Option('f', "feature-path", Required = true, HelpText = "Path to the root of the feature files")]
        public string FeaturePath { get; set; }
        [Option('n', "namespace", Required = true, HelpText = "Namespace to use for the test files")]
        public string Namespace { get; set; }
    }

    [Verb("other", HelpText = "Other.")]
    public class OtherOptions
    {
    }

    class Program
    {
        static int Main(string[] args)
        {
            //RunGenerateAndReturnExitCode(new GenerateOptions()
            //{
            //    FeaturePath = @"D:\DEV\personal\givenspecs\TestProject\Features\00BasicGherkin",
            //    Namespace = "TestProject.Features"
            //});
            //return 0;
            return Parser.Default
                .ParseArguments<GenerateOptions, OtherOptions>(args)
                .MapResult(
                    (GenerateOptions opts) => RunGenerateAndReturnExitCode(opts),
                    (OtherOptions opts) => 1,
                    errs => 1
                );
        }

        static int RunGenerateAndReturnExitCode(GenerateOptions opts)
        {
            var root = new DirectoryInfo(Path.Combine(Environment.CurrentDirectory, opts.FeaturePath));
            var files = root.GlobFiles("**/*.feature");
            var gen = new XunitGenerator(opts);
            foreach(var f in files)
            {
                try
                {
                    var parser = new Gherkin.Parser();
                    var content = parser.Parse(f.FullName);
                    var test = gen.Generate(content);
                    var outputPath = f.FullName + ".cs";
                    File.WriteAllText(outputPath, test);
                }
                catch (Exception ex )
                {
                    throw;
                }
            }
            return 0;
        }
    }
}
