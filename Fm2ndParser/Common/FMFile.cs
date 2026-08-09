using System.Collections.Generic;

namespace Fm2ndParser.Common
{
    public class FMFile
    {
        public string Type { get; set; }
        public bool Loaded { get; set; }
        public string Name { get; set; }
        public ICollection<Skill> Skills { get; set; }
        public ICollection<ImageResource> Images { get; set; }
        public ICollection<Palette> GlobalPalettes { get; set; }
        public ICollection<SoundResource> Sounds { get; set; }
    }
}
