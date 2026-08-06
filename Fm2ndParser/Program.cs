using CommandLine;
using CommandLine.Text;
using Fm2ndParser.Common;
using Fm2ndParser.Parsers;
using Microsoft.Extensions.Logging;
using System;
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

        static async Task Main(string[] args)
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

            using var loggerFactory = LoggerFactory.Create(builder =>
            {
                builder
                    .SetMinimumLevel(LogLevel.Information)
                    .AddSimpleConsole(options =>
                    {
                        options.SingleLine = true;
                        options.TimestampFormat = "HH:mm:ss ";
                    });
            });

            var knownVerbs = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                "parse",
                "compile",
            };

            if (args.Length > 0 && !args[0].StartsWith("-") && !knownVerbs.Contains(args[0]))
            {
                // select the default verb "parse" if the first argument is not a known verb
                args = new[] { "parse" }.Concat(args).ToArray();
            }

            var parsed = Parser.Default.ParseArguments<ParseOptions, CompileOptions>(args)
                .MapResult(
                    (ParseOptions opts) => new ParseCommand(
                        opts.InputFiles.Single(),
                        opts.CleanUp,
                        opts.NewFiles,
                        opts.ExportResources,
                        loggerFactory.CreateLogger<ParseCommand>()
                    ).Execute(),

                    (CompileOptions opts) => new CompileCommand(
                        opts.InputFiles.Single(),
                        loggerFactory.CreateLogger<CompileCommand>()
                    ).Execute(),
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
