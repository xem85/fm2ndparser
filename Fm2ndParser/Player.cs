using System.Collections.Generic;

namespace Fm2ndParser
{
    public class Player
    {
        public string Type { get; set; }
        public string Name { get; set; }
        public ICollection<Skill> Skills { get; set; }
    }
}