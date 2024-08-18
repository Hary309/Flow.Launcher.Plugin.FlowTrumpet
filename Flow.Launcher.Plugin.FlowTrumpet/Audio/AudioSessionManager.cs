using CoreAudio;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Xml.Linq;

namespace Flow.Launcher.Plugin.FlowTrumpet.Audio
{
    internal class AudioSessionManager : IAudioSessionManager
    {
        private MMDevice _activeDevice;

        private MMDeviceEnumerator _deviceEnumerator;
        private MMNotificationClient _notfClient;

        public AudioSessionManager()
        {
            _deviceEnumerator = new MMDeviceEnumerator(Guid.NewGuid());
            _deviceEnumerator.GetDefaultAudioEndpoint(DataFlow.Render, Role.Multimedia);

            _notfClient = new MMNotificationClient(_deviceEnumerator);

            _notfClient.DefaultDeviceChanged += (sender, e) =>
            {
                _activeDevice = _deviceEnumerator.GetDevice(e.DeviceId);
            };

            foreach (var dev in _deviceEnumerator.EnumerateAudioEndPoints(DataFlow.Render, DeviceState.Active))
            {
                if (dev.Selected)
                {
                    _activeDevice = dev;
                    break;
                }
            }

            if (_activeDevice == null)
            {
                throw new Exception("Device not found.");
            }
        }

        public AudioSessionInfo GetSessionInfo(uint processId)
        {
            var session = _activeDevice.AudioSessionManager2.Sessions.FirstOrDefault(x => x.ProcessID == processId);

            return CreateSessionInfo(session);
        }

        public IEnumerable<AudioSessionInfo> GetSessionsInfo()
        {
            foreach (var session in _activeDevice.AudioSessionManager2.Sessions)
            {
                yield return CreateSessionInfo(session);
            }
        }

        public void SetSessionVolume(uint processId, float newVolume)
        {
            var session = _activeDevice.AudioSessionManager2.Sessions.FirstOrDefault(x => x.ProcessID == processId);

            if (session == null)
            {
                return;
            }

            session.SimpleAudioVolume.MasterVolume = Math.Clamp(newVolume, 0.0f, 1.0f);
        }

        private AudioSessionInfo CreateSessionInfo(AudioSessionControl2 session)
        {
            Process p = Process.GetProcessById((int)session.ProcessID);
            var name = session.IsSystemSoundsSession ? "System Sounds" : session.DisplayName;
            if (name == "")
            {
                name = p.ProcessName;
            }

            return new AudioSessionInfo
            {
                Name = name,
                ProcessId = session.ProcessID,
                Volume = session.SimpleAudioVolume.MasterVolume,
            };
        }
    }
}
