using CommandLine;
using CommandLine.Text;
using Fm2ndParser.Kgt;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Fm2ndParser
{
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

    internal class GenerateCommand
    {
        private string kgtFile;

        public GenerateCommand(string kgtFile)
        {
            this.kgtFile = kgtFile;
        }
        public async Task Execute()
        {
            var baseDir = Path.GetDirectoryName(kgtFile);
            
            var contractResolver = new DynamicContractResolver()
            {
                NamingStrategy = new CamelCaseNamingStrategy()
            };
            var data = await File.ReadAllTextAsync(kgtFile);
            var json = JsonConvert.DeserializeObject<KGTFile>(data, new JsonSerializerSettings
            {
                ContractResolver = contractResolver,
                Formatting = Formatting.Indented
            });
        }
    }
}