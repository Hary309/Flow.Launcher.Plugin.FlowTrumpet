using CoreAudio;
using System;
using System.Collections.Generic;
using System.Collections;
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

        private List<AudioSessionInfo> _sessions = new();

        public AudioSessionManager()
        {
            _deviceEnumerator = new MMDeviceEnumerator(Guid.NewGuid());
            _activeDevice = _deviceEnumerator.GetDefaultAudioEndpoint(DataFlow.Render, Role.Multimedia);

            _activeDevice.AudioSessionManager2.OnSessionCreated += HandleSessionCreated;

            _notfClient = new MMNotificationClient(_deviceEnumerator);

            _notfClient.DefaultDeviceChanged += (sender, e) =>
            {
                _activeDevice = _deviceEnumerator.GetDevice(e.DeviceId);
            };

            if (_activeDevice == null)
            {
                throw new Exception("Device not found.");
            }

            foreach (var session in _activeDevice.AudioSessionManager2.Sessions)
            {
                _sessions.Add(CreateSessionInfo(session));
            }
        }

        private void HandleSessionCreated(object sender, CoreAudio.Interfaces.IAudioSessionControl2 newSession)
        {
            AudioSessionManager2 asm = (AudioSessionManager2)sender;

            newSession.GetProcessId(out uint newSessionId);

            asm.RefreshSessions();
            var session = asm.Sessions.FirstOrDefault(x => x.ProcessID == newSessionId);

            if (session == null)
            {
                return;
            }

            _sessions.Add(CreateSessionInfo(session));
        }

        public AudioSessionInfo GetSessionInfo(uint processId)
        {
            return _sessions.FirstOrDefault(x => x.ProcessId == processId);
        }

        public IEnumerable<AudioSessionInfo> GetSessionsInfo()
        {
            return _sessions;
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
            if (session == null)
            {
                return null;
            }

            if (session.State == AudioSessionState.AudioSessionStateExpired)
            {
                return null;
            }

            session.OnStateChanged += Session_OnStateChanged;

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
                IcoPath = ProcessHelper.TryGetProcessIconFilename(p),
            };
        }

        private void Session_OnStateChanged(object sender, AudioSessionState newState)
        {
            if (newState == AudioSessionState.AudioSessionStateExpired)
            {
                AudioSessionControl2 session = (AudioSessionControl2)sender;

                var sessionInfo = _sessions.FirstOrDefault(x => x.ProcessId == session.ProcessID);

                if (sessionInfo == null)
                {
                    return;
                }

                _sessions.Remove(sessionInfo);
            }
        }
    }
}
