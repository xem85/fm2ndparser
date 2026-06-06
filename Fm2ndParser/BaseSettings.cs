namespace Fm2ndParser
{
    public class BaseSettings
    {
        public bool Offset { get; set; }
        public bool StoryMode { get; set; }
        public bool VsMode { get; set; }
        public bool VsTeamMode { get; set; }
        public bool LockSource { get; set; }
        public bool NumbersOnHPLifeBar { get; set; }
        public bool CursorAppearsPressingAButton { get; set; }
        public StiffTime StiffTime { get; set; }
        internal ScreenSelect Select { get; set; }
    }
}