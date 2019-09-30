using CommandLine;

namespace GivenSpecs.CommandLine.Options
{
    [Verb("generate", HelpText = "Generate tests from feature files.")]
    public class GenerateOptions
    {
        [Option('f', "feature-path", Required = true, HelpText = "Path to the root of the feature files. A relative path is expected unless --absolute-path is set")]
        public string FeaturePath { get; set; }
        [Option("absolute-path", Required = false, HelpText = "Modify --feature-path to accept absolute path instead of the default relative path")]
        public bool AbsolutePath { get; set; }
        [Option('n', "namespace", Required = true, HelpText = "Namespace to use for the generated test files")]
        public string Namespace { get; set; }
        [Option("with-ast-json", Required = false, Default = false, HelpText = "Generate also AST json files")]
        public bool GenerateAstJson { get; set; }
    }
}
