using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;

namespace Fm2ndParser
{
    public class Command
    {
        public string Name { get; set; }
        public uint Time { get; set; }
        public SkillReference AirSkill { get; set; }
        public SkillReference StandSkill { get; set; }
        public SkillReference StandFarSkill { get; set; }
        public SkillReference CrouchedSkill { get; set; }
        public ICollection<CommandStep> Steps { get; set; }
    }
}