using System;
using System.IO;
using System.Text;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System.Collections.Generic;

namespace Fm2ndParser
{
    class Program
    {
        static void Main(string[] args)
        {
            if (!args.Any())
            {
                Console.WriteLine("You must specify a .player path as argument");
                return;
            }

            var cleanUp = args.Contains("-c");
            var newFiles = args.Contains("-n");
            args = args.Where(x => x != "-c" && x != "-n").ToArray();

            foreach (var filename in args)
            {
                doParse(filename, cleanUp, !newFiles);
            }
        }

        private static void doParse(string filename, bool cleanUp, bool overwrite)
        {
            try
            {
                var parser = new PlayerParse(filename);
                var player = parser.Parse();

                var contractResolver = new DynamicContractResolver()
                {
                    NamingStrategy = new CamelCaseNamingStrategy()
                };


                if (cleanUp)
                {
                    concatenateIBlocks(player);
                    contractResolver.AddPropertyToExclude(typeof(SkillReference), "number");
                    contractResolver.AddPropertyToExclude(typeof(SkillBlockReference), "number");
                }

                var json = JsonConvert.SerializeObject(player, new JsonSerializerSettings
                {
                    ContractResolver = contractResolver,
                    Formatting = Formatting.Indented
                });

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
                File.WriteAllText(jsonFilename, json);
            }
            catch (LockedFileException)
            {
                Console.WriteLine($"The file {filename} is locked, and can't be parsed.");
                Console.ReadLine();
            }
        }

        private static void concatenateIBlocks(Player player)
        {
            foreach (var skill in player.Skills)
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
