using System;
using System.Collections.Generic;
using System.Text;

namespace Fm2ndParser
{
    public class SkillBlockReference : SkillReference
    {
        public byte Block { get; set; }
        public string BlockType { get; set; }
    }
}
