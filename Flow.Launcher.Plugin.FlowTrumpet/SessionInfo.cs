using CoreAudio;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Flow.Launcher.Plugin.FlowTrumpet
{
    internal class SessionInfo : Result
    {
        private float _volume = 0.0f;

        public float Volume { get => _volume; }
        public string Name { get; init; }
        public uint ProcessId { get; init; }

        public SessionInfo(string name, float volume, uint processId)
        {
            Name = name;
            _volume = volume;
            ProcessId = processId;

            UpdateData();
        }

        public void SetVolume(float volume)
        {
            _volume = volume;
            UpdateData();
        }

        private void UpdateData()
        {
            Title = $"{Name} -- {_volume * 100}%";
            SubTitle = $"Id: {ProcessId}";
        }
    }
}
