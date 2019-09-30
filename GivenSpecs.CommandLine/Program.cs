using CommandLine;
using GivenSpecs.Application;
using GivenSpecs.Application.Configuration;
using GivenSpecs.Application.Interfaces;
using GivenSpecs.CommandLine.Options;
using GlobExpressions;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using System;
using System.IO;

namespace GivenSpecs.CommandLine
{
    class Program
    {
        static int Main(string[] args)
        {
            return Parser.Default
                .ParseArguments<GenerateOptions>(args)
                .MapResult(
                    (GenerateOptions opts) => RunGenerateAndReturnExitCode(opts),
                    errs => 1
                );
        }

        static int RunGenerateAndReturnExitCode(GenerateOptions opts)
        {
            // Dependency injection
            var services = new ServiceCollection();
            var config = new GivenSpecsAppConfiguration()
            {
                FeatureNamespace = opts.Namespace
            };
            services.AddGivenSpecsAppServices(config);
            var serviceProvider = services.BuildServiceProvider();

            DirectoryInfo root = opts.AbsolutePath ?
                new DirectoryInfo(opts.FeaturePath) :
                new DirectoryInfo(Path.Combine(Environment.CurrentDirectory, opts.FeaturePath));
            Console.WriteLine($"Searching feature files in : {root.FullName}");
            var files = root.GlobFiles("**/*.feature");
            var gen = serviceProvider.GetService<IXunitGeneratorService>();
            var first = true;

            foreach (var f in files)
            {
                try
                {
                    Console.WriteLine($"Processing file: {f.FullName}");

                    var parser = new Gherkin.Parser();
                    var content = parser.Parse(f.FullName);

                    var test = gen.Generate(content, f.FullName, first, gen).Result;

                    var outputPath = f.FullName + ".cs";
                    File.WriteAllText(outputPath, test);

                    if(opts.GenerateAstJson)
                    {
                        var contentStr = JsonConvert.SerializeObject(content);
                        outputPath = f.FullName + ".ast.json";
                        File.WriteAllText(outputPath, contentStr);
                    }
                }
                catch (Exception ex)
                {
                    throw;
                }
                first = false;
            }
            return 0;
        }
    }
}
