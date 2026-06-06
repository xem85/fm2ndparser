using CommandLine;
using CommandLine.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Resources;
using System.Text;

namespace Fm2ndParser
{
    class Program
    {
        class Options
        {
            [Usage(ApplicationAlias = "Fm2ndParser")]
            public static IEnumerable<Example> Examples
            {
                get
                {
                    yield return new Example("Single file", new Options { InputFiles = new[] { "character1.player" } });
                    yield return new Example("Parse and Clean up", new Options { InputFiles = new[] { "character1.player" }, CleanUp = true });
                    yield return new Example("Multiple files", new Options { InputFiles = new[] { "character1.player", "character2.player" } });
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

        static void Main(string[] args)
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

            CommandLine.Parser.Default.ParseArguments<Options>(args)
                .WithParsed(RunOptions)
                .WithNotParsed(HandleParseError);
        }

        private static void HandleParseError(IEnumerable<Error> obj)
        {
            //Console.WriteLine("You must specify a .player path as argument");
        }

        private static void RunOptions(Options options)
        {
            var kgtFile = options.InputFiles.Single();
            var baseDir = Path.GetDirectoryName(kgtFile);
            var parser = new KGTParser(kgtFile);
            var kgt = parser.Parse();
            doParse(kgt, kgtFile, options.CleanUp, !options.NewFiles, options.ExportResources);

            foreach (var character in kgt.Characters)
            {
                var filename = Path.Combine(baseDir, character + ".player");
                var playerParser = new PlayerParser(filename, kgt);
                var player = playerParser.Parse();

                doParse(player, filename, options.CleanUp, !options.NewFiles, options.ExportResources);
            }
        }

        private static void doParse(FMFile fmFile, string filename, bool cleanUp, bool overwrite, bool doExportResources)
        {
            try
            {
                string jsonFilename;
                if (overwrite)
                {
                    jsonFilename = getJsonFilename(filename);
                }
                else
                {
                    jsonFilename = getFreeJsonFilename(filename);
                    if (File.Exists(jsonFilename))
                    {
                        throw new Exception("File exists: " + jsonFilename);
                    }
                }
                if (doExportResources)
                    exportResources(fmFile, jsonFilename);

                var contractResolver = new DynamicContractResolver()
                {
                    NamingStrategy = new CamelCaseNamingStrategy()
                };

                if (cleanUp)
                {
                    concatenateIBlocks(fmFile);
                    contractResolver.AddPropertyToExclude(typeof(ImageResource), "data");
                    contractResolver.AddPropertyToExclude(typeof(SoundResource), "data");
                    contractResolver.AddPropertyToExclude(typeof(SkillReference), "number");
                    contractResolver.AddPropertyToExclude(typeof(SkillBlockReference), "number");
                    contractResolver.AddPropertyToExclude(typeof(Skill), "index");
                }

                var json = JsonConvert.SerializeObject(fmFile, new JsonSerializerSettings
                {
                    ContractResolver = contractResolver,
                    Formatting = Formatting.Indented
                });

                File.WriteAllText(jsonFilename, json);
            }
            catch (LockedFileException)
            {
                Console.WriteLine($"The file {filename} is locked, and can't be parsed.");
                Console.ReadLine();
            }
        }

        private static void concatenateIBlocks(FMFile fmFile)
        {
            foreach (var skill in fmFile.Skills)
            {
                var skillBlocks = new List<Block>();
                IBlock lastI = null;
                foreach (var block in skill.Blocks)
                {
                    block.Index = 0; // skillBlocks.Count();
                    if (block is FABlock)
                    {
                        var faBlock = block as FABlock;
                        faBlock.X = 0;
                        faBlock.Y = 0;
                        faBlock.Width = 0;
                        faBlock.Height = 0;
                    }
                    if (block is IBlock)
                    {
                        var iBlock = block as IBlock;
                        if (lastI != null)
                        {
                            lastI.Wait += iBlock.Wait;
                        }
                        else
                        {
                            skillBlocks.Add(iBlock);

                            iBlock.I = 0;
                            iBlock.X = 0;
                            iBlock.Y = 0;
                            iBlock.TurnX = false;
                            iBlock.TurnY = false;
                            lastI = iBlock;
                        }
                    }
                    else
                    {
                        skillBlocks.Add(block);
                        lastI = null;
                    }
                }
                skill.Blocks = skillBlocks;
            }
        }

        private static void exportResources(FMFile fmFile, string jsonFilename)
        {
            var baseName = Path.GetFileNameWithoutExtension(jsonFilename);
            var outputDir = Path.Combine(Path.GetDirectoryName(jsonFilename) ?? string.Empty, baseName);
            Directory.CreateDirectory(outputDir);

            var imageDir = Path.Combine(outputDir, "img");
            var soundDir = Path.Combine(outputDir, "snd");
            Directory.CreateDirectory(imageDir);
            Directory.CreateDirectory(soundDir);

            if (fmFile.Images != null)
            {
                var altPaletteDirs = new List<string>();
                for (int i = 1; i <= 7; i++)
                {
                    var dir = Path.Combine(outputDir, i.ToString());
                    Directory.CreateDirectory(dir);
                    altPaletteDirs.Add(dir);
                }

                int imageIndex = 0;
                foreach (var image in fmFile.Images)
                {
                    var filename = $"{imageIndex:D4}.bmp";

                    if (image.PaletteType == 1)
                    {
                        var imagePath = Path.Combine(imageDir, filename);
                        writeIndexedBmp(imagePath, image.Width, image.Height, image.Data, null);
                    }
                    else
                    {
                        var defaultPalette = getGlobalPalette(fmFile, 0);
                        var imagePath = Path.Combine(imageDir, filename);
                        writeIndexedBmp(imagePath, image.Width, image.Height, image.Data, defaultPalette);

                        for (int p = 1; p <= 7; p++)
                        {
                            var altPalette = getGlobalPalette(fmFile, p);
                            var altImagePath = Path.Combine(altPaletteDirs[p - 1], filename);
                            writeIndexedBmp(altImagePath, image.Width, image.Height, image.Data, altPalette);
                        }
                    }

                    imageIndex++;
                }
            }

            if (fmFile.Sounds != null)
            {
                int soundIndex = 0;
                foreach (var sound in fmFile.Sounds)
                {
                    var soundPath = Path.Combine(soundDir, $"{soundIndex:D4}.wav");
                    File.WriteAllBytes(soundPath, sound.Data ?? Array.Empty<byte>());
                    soundIndex++;
                }
            }
        }

        private static void writeIndexedBmp(string outputPath, uint width, uint height, byte[] imageData, byte[] externalPalette)
        {
            if (width == 0 || height == 0)
            {
                return;
            }

            if (imageData == null)
            {
                return;
            }

            var pixelSize = checked((int)(width * height));
            var paletteSize = 1024;
            var hasEmbeddedPalette = externalPalette == null;
            var required = hasEmbeddedPalette ? pixelSize + paletteSize : pixelSize;
            if (imageData.Length < required)
            {
                return;
            }

            byte[] palette;
            int pixelOffset;

            if (hasEmbeddedPalette)
            {
                palette = new byte[paletteSize];
                Buffer.BlockCopy(imageData, 0, palette, 0, paletteSize);
                pixelOffset = paletteSize;
            }
            else
            {
                palette = new byte[paletteSize];
                if (externalPalette != null)
                {
                    Buffer.BlockCopy(externalPalette, 0, palette, 0, Math.Min(paletteSize, externalPalette.Length));
                }
                pixelOffset = 0;
            }

            var rowStride = ((int)width + 3) & ~3;
            var pixelArraySize = rowStride * (int)height;
            var fileHeaderSize = 14;
            var dibHeaderSize = 40;
            var dataOffset = fileHeaderSize + dibHeaderSize + paletteSize;
            var fileSize = dataOffset + pixelArraySize;

            using var stream = new FileStream(outputPath, FileMode.Create, FileAccess.Write, FileShare.None);
            using var writer = new BinaryWriter(stream);

            writer.Write((byte)'B');
            writer.Write((byte)'M');
            writer.Write(fileSize);
            writer.Write((short)0);
            writer.Write((short)0);
            writer.Write(dataOffset);

            writer.Write(dibHeaderSize);
            writer.Write((int)width);
            writer.Write((int)height);
            writer.Write((short)1);
            writer.Write((short)8);
            writer.Write(0);
            writer.Write(pixelArraySize);
            writer.Write(2835);
            writer.Write(2835);
            writer.Write(256);
            writer.Write(256);

            writer.Write(palette);

            var rowBuffer = new byte[rowStride];
            for (int y = (int)height - 1; y >= 0; y--)
            {
                Array.Clear(rowBuffer, 0, rowBuffer.Length);
                Buffer.BlockCopy(imageData, pixelOffset + y * (int)width, rowBuffer, 0, (int)width);
                writer.Write(rowBuffer);
            }
        }

        private static byte[] getGlobalPalette(FMFile fmFile, int paletteIndex)
        {
            if (fmFile?.GlobalPalettes == null)
            {
                return null;
            }

            var palettes = fmFile.GlobalPalettes.ToList();
            if (paletteIndex < 0 || paletteIndex >= palettes.Count)
            {
                return null;
            }

            var palette = palettes[paletteIndex];
            if (palette == null || palette.Length < 0x400)
            {
                return null;
            }

            var bmpPalette = new byte[0x400];
            Buffer.BlockCopy(palette, 0, bmpPalette, 0, 0x400);
            return bmpPalette;
        }

        private static string getJsonFilename(string filename)
        {
            return Path.GetFileNameWithoutExtension(filename) + ".json";
        }

        private static string getFreeJsonFilename(string filename)
        {
            int i = 0;
            var jsonFilename = getJsonFilename(filename);
            while (File.Exists(jsonFilename))
            {
                jsonFilename = $"{Path.GetFileNameWithoutExtension(filename)}_{i}.json";
                i++;
            }

            return jsonFilename;
        }
    }


    public class DynamicContractResolver : DefaultContractResolver
    {
        private readonly Dictionary<Type, List<string>> _propertyNameToExclude;

        public DynamicContractResolver()
        {
            _propertyNameToExclude = new Dictionary<Type, List<string>>();
        }

        public void AddPropertyToExclude(Type type, string name)
        {
            if (!_propertyNameToExclude.ContainsKey(type))
            {
                _propertyNameToExclude.Add(type, new List<string>());
            }
            _propertyNameToExclude[type].Add(name);
        }

        protected override IList<JsonProperty> CreateProperties(Type type, MemberSerialization memberSerialization)
        {
            IList<JsonProperty> properties = base.CreateProperties(type, memberSerialization);

            // only serializer properties that are not named after the specified property.
            properties =
                properties.Where(p =>
                    !_propertyNameToExclude.ContainsKey(type) ||
                    !_propertyNameToExclude[type].Contains(p.PropertyName)
                ).ToList();

            return properties;
        }
    }
}
