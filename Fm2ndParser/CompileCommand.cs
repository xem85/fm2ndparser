using CommandLine;
using CommandLine.Text;
using Fm2ndParser.Character;
using Fm2ndParser.Common;
using Fm2ndParser.Compilers;
using Fm2ndParser.Demo;
using Fm2ndParser.Kgt;
using Fm2ndParser.Stage;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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

        [Option('o', "output",
          Default = null,
          HelpText = "Specify output folder.")]
        public string Output { get; set; }
    }

    internal class CompileCommand : BaseCommand
    {
        private string inputFile;
        private string output;
        private ILogger<CompileCommand> logger;

        public CompileCommand(CompileOptions opts, ILogger<CompileCommand> logger)
        {
            this.inputFile = opts.InputFiles.Single();
            this.output = opts.Output ?? $"compiled_{generateDefaultOutputFolder()}";
            this.logger = logger;
        }
        public async Task Execute()
        {
            this.validateInputs(output);
            Directory.CreateDirectory(this.output);

            var jo = await readJObject(inputFile);
            if (inputFile.EndsWith(".kgt.json"))
                await compileKgt(inputFile);
            else
                await doCompile(inputFile, null);
        }


        private async Task compileKgt(string inputFile)
        {
            var kgt = (KGTFile)(await doCompile(inputFile, null));
            var baseDir = Path.GetDirectoryName(inputFile);

            foreach (var character in kgt.Characters)
            {
                var inputFilename = Path.Combine(baseDir, character.Name + ".player.json");
                await doCompile(inputFilename, kgt);
            }

            foreach (var stageName in kgt.Stages)
            {
                var inputFilename = Path.Combine(baseDir, stageName + ".stage.json");
                await doCompile(inputFilename, kgt);
            }

            foreach (var demoName in kgt.Demos)
            {
                var inputFilename = Path.Combine(baseDir, demoName + ".demo.json");
                await doCompile(inputFilename, kgt);
            }
        }

        private async Task<FMFile> doCompile(string inputFilename, KGTFile kgtRef)
        {
            logger.LogInformation($"Compiling {inputFilename}...");

            var compiledDir = this.output;
            var outputFilename = Path.Combine(compiledDir, Path.GetFileName(inputFilename));
            outputFilename = Path.ChangeExtension(outputFilename, null);

            var parts = inputFilename.Split('.');
            var extension = $".{parts[^2]}.{parts[^1]}";

            switch (extension)
            {
                case ".kgt.json":
                    var kgt = await toFMFile<KGTFile>(inputFilename);
                    var kgtCompiler = new KGTCompiler(kgt);
                    kgtCompiler.Compile(outputFilename);
                    return kgt;
                case ".player.json":
                    var player = await toFMFile<PlayerFile>(inputFilename);
                    var playerCompiler = new PlayerCompiler(player, kgtRef);
                    playerCompiler.Compile(outputFilename);
                    return player;
                case ".stage.json":
                    var stage = await toFMFile<StageFile>(inputFilename);
                    var stageCompiler = new StageCompiler(stage, kgtRef);
                    stageCompiler.Compile(outputFilename);
                    return stage;
                case ".demo.json":
                    var demo = await toFMFile<DemoFile>(inputFilename);
                    var demoCompiler = new DemoCompiler(demo, kgtRef);
                    demoCompiler.Compile(outputFilename);
                    return demo;
                default:
                    Console.WriteLine($"Unsupported file type '{extension}'. Expected .kgt.json, .player.json, .stage.json or .demo.json.");
                    return null;
            }
        }

        private async Task<T> toFMFile<T>(string inputFile) where T : FMFile, new()
        {
            var jo = await readJObject(inputFile);

            var settings = new JsonSerializerSettings
            {
                ContractResolver = new DynamicContractResolver()
                {
                    NamingStrategy = new CamelCaseNamingStrategy()
                },
                Formatting = Formatting.Indented,
                Converters = {
                    new BlockConverter() ,
                    new StoryEntryConverter(),
                }
            };

            var obj = jo.ToObject<T>(JsonSerializer.Create(settings));
            return obj;
        }


        private async Task<JObject> readJObject(string inputFile)
        {
            var data = await File.ReadAllTextAsync(inputFile);

            var jo = JsonConvert.DeserializeObject<JObject>(data);
            var type = jo["type"]?.Value<string>();

            if (string.IsNullOrWhiteSpace(type))
                throw new JsonSerializationException("Property 'type' is required");

            return jo;
        }
    }
}