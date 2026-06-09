namespace Fm2ndParser
{
    class JumpStoryEntry : StoryEntry
    {
        public StoryEntryJump If { get; set; }
        public byte Value { get; set; }
        public object GoToEvent { get; set; }
    }
}