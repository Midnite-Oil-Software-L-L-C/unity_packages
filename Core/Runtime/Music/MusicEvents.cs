namespace MidniteOilSoftware.Core.Music
{
    public struct PlayMusicEvent
    {
        public string MusicGroupName { get; }

        public PlayMusicEvent(string musicGroupName)
        {
            MusicGroupName = musicGroupName;
        }
    }
    
    public struct StopAllMusicEvent
    {
    }
}
