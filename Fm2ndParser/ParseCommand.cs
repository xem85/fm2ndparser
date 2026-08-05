using CommandLine;
using CommandLine.Text;
using Fm2ndParser.Common;
using Fm2ndParser.Parsers;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Fm2ndParser
{

    [Verb("parse", HelpText = "Parse FM2k binary files into JSON files.")]
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

    public class ParseCommand
    {
        private string inputFile;
        private bool cleanUp;
        private bool overwrite;
        private bool doExportResources;
        private ILogger<ParseCommand> logger;

        public ParseCommand(string inputFile, bool cleanUp, bool newFile, bool doExportResources, ILogger<ParseCommand> logger)
        {
            this.inputFile = inputFile;
            this.cleanUp = cleanUp;
            this.overwrite = !newFile;
            this.doExportResources = doExportResources;
            this.logger = logger;
        }

        public async Task Execute()
        {
            var extension = Path.GetExtension(inputFile).ToLowerInvariant();

            switch (extension)
            {
                case ".kgt":
                    parseKgt(inputFile);
                    break;
                case ".player":
                    parseSingle(new PlayerParser(inputFile, null).Parse(), inputFile);
                    break;
                case ".stage":
                    parseSingle(new StageParser(inputFile, null).Parse(), inputFile);
                    break;
                case ".demo":
                    parseSingle(new DemoParser(inputFile, null).Parse(), inputFile);
                    break;
                default:
                    Console.WriteLine($"Unsupported file type '{extension}'. Expected .kgt, .player, .stage or .demo.");
                    break;
            }
        }

        private void parseSingle(FMFile fmFile, string filename)
        {
            doParse(fmFile, filename);
        }


        private void parseKgt(string kgtFile)
        {
            var baseDir = Path.GetDirectoryName(kgtFile);
            var parser = new KGTParser(kgtFile);
            var kgt = parser.Parse();
            doParse(kgt, kgtFile);

            foreach (var character in kgt.Characters)
            {
                var filename = Path.Combine(baseDir, character + ".player");
                var playerParser = new PlayerParser(filename, kgt);
                var player = playerParser.Parse();

                doParse(player, filename);
            }

            foreach (var stageName in kgt.Stages)
            {
                var filename = Path.Combine(baseDir, stageName + ".stage");
                var stageParser = new StageParser(filename, kgt);
                var stage = stageParser.Parse();
                doParse(stage, filename);
            }

            foreach (var demoName in kgt.Demos)
            {
                var filename = Path.Combine(baseDir, demoName + ".demo");
                var demoParser = new DemoParser(filename, kgt);
                var demo = demoParser.Parse();
                doParse(demo, filename);
            }
        }


        private void doParse(FMFile fmFile, string filename)
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
                    contractResolver.AddPropertyToExclude(typeof(ImageResource), "offset");
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
                int imageIndex = 0;
                foreach (var image in fmFile.Images)
                {
                    if (image.Data.Length == 0) continue;

                    var filename = $"{imageIndex:D4}.bmp";

                    if (image.PaletteType == PaletteType.Private)
                    {
                        var imagePath = Path.Combine(imageDir, filename);
                        var bmpStream = ToIndexedBmpStream(image, (byte[])null);
                        File.WriteAllBytes(imagePath, bmpStream.ToArray());
                    }
                    else
                    {
                        for (int p = 0; p < 8; p++)
                        {
                            var altPalette = getGlobalPalette(fmFile, p);

                            var dir = Path.Combine(outputDir, (p + 1).ToString());
                            Directory.CreateDirectory(dir);
                            var altImagePath = Path.Combine(dir, filename);
                            var bmpStream = ToIndexedBmpStream(image, altPalette);

                            File.WriteAllBytes(altImagePath, bmpStream.ToArray());
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

        #region Palette Conversion
        public static byte[] ToFM2kPalette(Color[] colors)
        {
            var result = colors.SelectMany(x => toFM2kColor(x)).ToArray();
            return result;
        }

        private static byte[] toFM2kColor(Color color)
        {
            if (color.A == 255)
            {
                var r = (byte)Math.Min((int)Math.Round((double)color.R / 8) * 8, 255);
                var g = (byte)Math.Min((int)Math.Round((double)color.G / 8) * 8, 255);
                var b = (byte)Math.Min((int)Math.Round((double)color.B / 8) * 8, 255);

                return new byte[] { b, g, r, 1 };
            }
            else
            {
                return new byte[] { 0, 0, 0, 0 };
            }
        }

        private static string toFM2kColorString(Color color)
        {
            var colorArray = toFM2kColor(color);
            var result = string.Join(" ", colorArray.Select(x => x.ToString("X2")));

            return result + " ";
        }

        public static MemoryStream ToIndexedBmpStream(ImageResource image, Palette alternativePalette = null)
        {
            var paletteData = alternativePalette != null ? ToFM2kPalette(alternativePalette.Colors) : null;
            return ToIndexedBmpStream(image.Width, image.Height, image.Data, paletteData);
        }

        public static MemoryStream ToIndexedBmpStream(ImageResource image, byte[] alternativePalette = null)
        {
            return ToIndexedBmpStream(image.Width, image.Height, image.Data, alternativePalette);
        }

        public static MemoryStream ToIndexedBmpStream(uint width, uint height, byte[] imageData, byte[] externalPalette = null)
        {
            if (width == 0 || height == 0)
            {
                return null;
            }

            if (imageData == null)
            {
                return null;
            }

            var pixelSize = checked((int)(width * height));
            var paletteSize = 1024;
            var hasEmbeddedPalette = externalPalette == null;
            var required = hasEmbeddedPalette ? pixelSize + paletteSize : pixelSize;
            if (imageData.Length < required)
            {
                return null;
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

            var stream = new MemoryStream();
            var writer = new BinaryWriter(stream);

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
            stream.Position = 0;
            return stream;
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
            if (palette == null || palette.Data.Length < 0x400)
            {
                return null;
            }

            var bmpPalette = new byte[0x400];
            Buffer.BlockCopy(palette.Data, 0, bmpPalette, 0, 0x400);
            return bmpPalette;
        }
        #endregion

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
}
