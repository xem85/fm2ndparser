using Fm2ndParser.Common;
using System.Collections.Generic;

namespace Fm2ndParser.Kgt
{
    public class KGTFile : FMFile
    {
        public ICollection<Character> Characters { get; set; }
        public List<HitJunction> HitJunctions { get; set; }
        public List<string> CommonImages { get; set; }
        public List<string> Stages { get; set; }
        public List<string> Demos { get; set; }
        public SelectionScreenSettings SelectionScreen { get; set; }
        public BaseSettings BaseSettings { get; set; }
        public KGTBuiltInSkills BuiltInSkills { get; set; }
    }
}
