namespace MidniteOilSoftware.Core.Music
{
    public struct PlayMusicEvent
    {
        public string MusicGroupName { get; }
        public bool Loop { get; }

        public PlayMusicEvent(string musicGroupName, bool loop = false)
        {
            MusicGroupName = musicGroupName;
            Loop = loop;
        }
    }
    
    public struct StopAllMusicEvent
    {
    }
}
