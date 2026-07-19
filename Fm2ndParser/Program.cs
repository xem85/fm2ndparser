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
