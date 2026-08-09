using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Fm2ndParser
{
    public class BlockConverter : JsonConverter<Block>
    {
        public override Block ReadJson(JsonReader reader, Type objectType, Block existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            JObject jsonObject = JObject.Load(reader);

            string typeName = jsonObject["type"]?.Value<string>();
            Block block = typeName switch
            {
                "Settings" => new SettingsBlock(),
                "M" => new MBlock(),
                "DS" => new DSBlock(),
                "S" => new SBlock(),
                "O" => new OBlock(),
                "E" => new EBlock(),
                "RC" => new RCBlock(),
                "SF" => new SFBlock(),
                "SG" => new SGBlock(),
                "SC" => new SCBlock(),
                "I" => new IBlock(),
                "EB" => new EBBlock(),
                "GS" => new GSBlock(),
                "GL" => new GLBlock(),
                "RP" => new RPBlock(),
                "GC" => new GCBlock(),
                "R" => new RBlock(),
                "DB" => new DBBlock(),
                "FA" => new FABlock(),
                "FD" => new FDBlock(),
                "PS" => new PSBlock(),
                "C" => new CBlock(),
                "V" => new VBlock(),
                "Rnd" => new RndBlock(),
                "COLOR" => new ColorBlock(),
                "COM" => new ComBlock(),
                "AI" => new AIBlock(),
                _ => throw new NotSupportedException($"Unknown block type: {typeName}")
            };

            serializer.Populate(jsonObject.CreateReader(), block);
            return block;
        }

        public override void WriteJson(JsonWriter writer, Block value, JsonSerializer serializer)
        {
            serializer.Serialize(writer, value);
        }

    }
}
