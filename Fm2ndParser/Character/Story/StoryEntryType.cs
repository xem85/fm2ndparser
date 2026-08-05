using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Runtime.Serialization;

namespace Fm2ndParser.Character.Story
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum StoryEntryType : byte
    {
        [EnumMember(Value = "N")]
        None = 0,

        [EnumMember(Value = "F")]
        Fight = 1,

        [EnumMember(Value = "D")]
        Demo = 2,

        [EnumMember(Value = "J")]
        IfDiversion = 3,

        [EnumMember(Value = "E")]
        End = 4,
    }
}