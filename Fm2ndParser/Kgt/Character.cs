using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;

namespace Fm2ndParser.Kgt
{
    public class Character
    {
        public string Name { get; set; }
        public bool EnabledForStoryMode { get; set; }
        public bool EnabledForVsMode { get; set; }
    }
}
