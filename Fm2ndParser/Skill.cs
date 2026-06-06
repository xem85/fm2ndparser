using System.Collections.Generic;
using System.Linq;

namespace Fm2ndParser
{
    internal class Skill
    {
        public int Type { get; set; }
        public string Name { get; set; }
        public short Position { get; set; }
        public Settings Settings
        {
            get { return Blocks.First() as Settings; }
        }
        public ICollection<Block> Blocks { get; set; }
    }
}