using Fm2ndParser.Common;
using System.Collections.Generic;

namespace Fm2ndParser.Demo
{
    public class DemoFile : FMFile
    {
        public SkillReference BGM { get; set; }

        public uint Time { get; set; }
        public bool SkipWithInput { get; set; }
    }
}
