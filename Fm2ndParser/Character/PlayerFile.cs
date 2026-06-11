using Fm2ndParser.Character.Story;
using Fm2ndParser.Common;
using System.Collections.Generic;

namespace Fm2ndParser.Character
{
    public class PlayerFile : FMFile
    {
        public IList<Command> Commands { get; set; }
        public PlayerSettings Settings { get; set; }
        public StoryMode StoryMode { get; set; }
        public ICollection<CommonImage> CommonImages { get; set; }
        public ICollection<CpuCommand> Cpu { get; set; }
        public ICollection<HitJunctionSkills> HitJunctionsSkills { get; set; }
        public PlayerBuiltInSkills BuiltInSkills { get; set; }
    }
}
