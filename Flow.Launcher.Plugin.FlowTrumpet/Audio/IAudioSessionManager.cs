using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Flow.Launcher.Plugin.FlowTrumpet.Audio
{
    internal interface IAudioSessionManager
    {
        IEnumerable<AudioSessionInfo> GetSessionsInfo();
        AudioSessionInfo GetSessionInfo(uint processId);
        void SetSessionVolume(uint processId, float newVolume);
    }
}
