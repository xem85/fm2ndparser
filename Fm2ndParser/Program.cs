using CommandLine;
using CommandLine.Text;
using Fm2ndParser.Common;
using Fm2ndParser.Parsers;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Resources;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace Fm2ndParser
{
    class Program
    {
        public class ParseOptions
        {
            [Usage(ApplicationAlias = "Fm2ndParser")]
            public static IEnumerable<Example> Examples
            {
                get
                {
                    yield return new Example("Single file", new ParseOptions { InputFiles = new[] { "character1.player" } });
                    yield return new Example("Parse and Clean up", new ParseOptions { InputFiles = new[] { "character1.player" }, CleanUp = true });
                    yield return new Example("Multiple files", new ParseOptions { InputFiles = new[] { "character1.player", "character2.player" } });
                }
            }

            [Value(0, Required = true, Hidden = true, HelpText = "Kgt input file to be processed.")]
            public IEnumerable<string> InputFiles { get; set; }

            [Option('n', "new-files",
              Default = false,
              HelpText = "Instead of replacing the existing json, it creates another one.")]
            public bool NewFiles { get; set; }

            [Option('c', "clean-up",
              Default = false,
              HelpText = "Merges [I] blocks and does other cleanups for comparison purposes.")]
            public bool CleanUp { get; set; }

            [Option('x', "export-resources",
              Default = false,
              HelpText = "Export attached resources.")]
            public bool ExportResources { get; set; }
        }


        [Verb("generate", HelpText = "Generate FM2k binary files from JSON files.")]
        public class GenerateOptions
        {
            [Usage(ApplicationAlias = "Fm2ndParser")]
            public static IEnumerable<Example> Examples
            {
                get
                {
                    yield return new Example("Single file", new GenerateOptions { InputFiles = new[] { "character1.json" } });
                    yield return new Example("Multiple files", new GenerateOptions { InputFiles = new[] { "character1.json", "character2.json" } });
                }
            }

            [Value(0, Required = true, Hidden = true, HelpText = "JSON input file to be processed.")]
            public IEnumerable<string> InputFiles { get; set; }
        }

        static async Task Main(string[] args)
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

            var parsed = Parser.Default.ParseArguments<ParseOptions, GenerateOptions>(args)
                .MapResult(
                (ParseOptions opts) => new ParseCommand(
                   opts.InputFiles.Single(),
                   opts.CleanUp,
                   opts.NewFiles,
                   opts.ExportResources
                ).Execute(),
                (GenerateOptions opts) => new GenerateCommand(opts.InputFiles.Single()).Execute(),
                errs => HandleParseError(errs)
            );
            await parsed;
        }

        private static Task HandleParseError(IEnumerable<Error> obj)
        {
            //Console.WriteLine("You must specify a .player path as argument");
            return Task.CompletedTask;
        }
    }
}
