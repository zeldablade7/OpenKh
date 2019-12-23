using System.Collections.Generic;

namespace OpenKh.Audio
{
    public interface IAudioContainer
    {
        IEnumerable<IAudio> Entries { get; }
    }
}