namespace OpenKh.Audio
{
    public interface IAudio
    {
        byte Channels { get; }
        int ChannelSize { get; }
        int SampleRate { get; }

    }
}
