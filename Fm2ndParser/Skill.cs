using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;

namespace Fm2ndParser
{
    public class Skill
    {
        public int Type { get; set; }
        public int Index { get;  set; }
        public string Name { get; set; }
        [JsonIgnore]
        public short Position { get; set; }
        public Settings Settings
        {
            get { return Blocks.FirstOrDefault() as Settings; }
        }
        public ICollection<Block> Blocks { get; set; }
    }
}