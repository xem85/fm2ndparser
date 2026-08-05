using CommandLine;
using CommandLine.Text;
using Fm2ndParser.Character;
using Fm2ndParser.Common;
using Fm2ndParser.Compilers;
using Fm2ndParser.Demo;
using Fm2ndParser.Kgt;
using Fm2ndParser.Stage;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Fm2ndParser
{
    [Verb("compile", HelpText = "Compile FM2k binary files from JSON files.")]
    public class CompileOptions
    {
        [Usage(ApplicationAlias = "Fm2ndParser")]
        public static IEnumerable<Example> Examples
        {
            get
            {
                yield return new Example("Single file", new CompileOptions { InputFiles = new[] { "character1.json" } });
                yield return new Example("Multiple files", new CompileOptions { InputFiles = new[] { "character1.json", "character2.json" } });
            }
        }

        [Value(0, Required = true, Hidden = true, HelpText = "JSON input file to be processed.")]
        public IEnumerable<string> InputFiles { get; set; }
    }

    internal class CompileCommand : Command 
    {
        private string kgtFile;

        public CompileCommand(string kgtFile)
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

            var settings = new JsonSerializerSettings
            {
                ContractResolver = contractResolver,
                Formatting = Formatting.Indented,
                Converters = {
                    new BlockConverter() ,
                    new StoryEntryConverter(),
                }
            };

            var jo = JObject.Parse(data);
            var type = jo["type"]?.Value<string>();

            if (string.IsNullOrWhiteSpace(type))
                throw new JsonSerializationException("Property 'type' is required");

            switch (type)
            {
                case "player":
                    {
                        var player = jo.ToObject<PlayerFile>(JsonSerializer.Create(settings));
                        var compiler = new PlayerCompiler($"{player.Name}.player", player);
                        compiler.Compile();
                        break;
                    }

                case "demo":
                    {
                        var demo = jo.ToObject<DemoFile>(JsonSerializer.Create(settings));
                        var compiler = new DemoCompiler($"{demo.Name}.demo", demo);
                        compiler.Compile();
                        break;
                    }

                case "stage":
                    {
                        var stage = jo.ToObject<StageFile>(JsonSerializer.Create(settings));
                        var compiler = new StageCompiler($"{stage.Name}.stage", stage);
                        compiler.Compile();
                        break;
                    }

                case "kgt":
                    {
                        var kgt = jo.ToObject<KGTFile>(JsonSerializer.Create(settings));
                        var compiler = new KGTCompiler($"{kgt.Name}.kgt", kgt);
                        compiler.Compile();
                        break;
                    }

                default:
                    throw new InvalidDataException($"Unknown type: {type}");
            }
        }
    }
}