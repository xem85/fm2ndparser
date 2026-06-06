using System.Collections.Generic;

namespace Fm2ndParser
{
    public class KGT : FMFile
    {
        public ICollection<string> Characters { get; set; }
        public List<string> HitJunctions { get; set; }
        public List<string> CommonImages { get; set; }
        public List<string> Stages { get; internal set; }
        public List<string> Demos { get; internal set; }
        public SelectionScreenSettings SelectionScreen { get; set; }
        public BaseSettings BaseSettings { get; internal set; }
    }
}
