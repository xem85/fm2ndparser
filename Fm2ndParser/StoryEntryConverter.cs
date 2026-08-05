using Fm2ndParser.Character.Story;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Fm2ndParser
{
    public class StoryEntryConverter : JsonConverter<StoryEntry>
    {
        public override StoryEntry ReadJson(JsonReader reader, Type objectType, StoryEntry existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            JObject jsonObject = JObject.Load(reader);

            string typeName = jsonObject["type"]?.Value<string>();
            StoryEntry block = typeName switch
            {
                "F" => new FightStoryEntry(),
                "J" => new JumpStoryEntry(),
                "D" => new DemoStoryEntry(),
                "E" => new EndStoryEntry(),
                _ => throw new NotSupportedException($"Unknown story entry type: {typeName}")
            };

            serializer.Populate(jsonObject.CreateReader(), block);
            return block;
        }

        public override void WriteJson(JsonWriter writer, StoryEntry value, JsonSerializer serializer)
        {
            serializer.Serialize(writer, value);
        }

    }
}
