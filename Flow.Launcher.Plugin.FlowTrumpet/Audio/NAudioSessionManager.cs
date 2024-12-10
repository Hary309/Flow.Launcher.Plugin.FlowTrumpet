using NAudio.CoreAudioApi;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Flow.Launcher.Plugin.FlowTrumpet.Audio
{
    internal class NAudioSessionManager : IAudioSessionManager
    {
        private MMDeviceEnumerator deviceEnumerator = new();

        public MMDevice GetDevice()
        {
            var datetime1 = DateTime.Now;
            var devices = new List<MMDevice>();

            return deviceEnumerator.GetDefaultAudioEndpoint(DataFlow.Render, Role.Console);
        }

        public AudioSessionInfo GetSessionInfo(uint processId)
        {
            var device = GetDevice();
            var sessions = device.AudioSessionManager.Sessions;

            var session = FindSession(processId);

            if (session != null)
            {
                return CreateSessionInfo(session);
            }

            return null;
        }

        public IEnumerable<AudioSessionInfo> GetSessionsInfo()
        {
            var device = GetDevice();
            var sessions = device.AudioSessionManager.Sessions;

            for (int i = 0; i < sessions.Count; i++)
            {
                var session = sessions[i];

                yield return CreateSessionInfo(session);
            }
        }

        public void SetSessionVolume(uint processId, float newVolume)
        {
            var session = FindSession(processId);

            if (session != null)
            {
                session.SimpleAudioVolume.Volume = newVolume;
            }
        }

        private AudioSessionControl FindSession(uint processId)
        {
            var device = GetDevice();
            var sessions = device.AudioSessionManager.Sessions;

            for (int i = 0; i < sessions.Count; i++)
            {
                var session = sessions[i];

                if (session.GetProcessID == processId)
                {
                    return session;
                }
            }

            return null;
        }

        static private AudioSessionInfo CreateSessionInfo(AudioSessionControl session)
        {
            Process p = Process.GetProcessById((int)session.GetProcessID);
            var name = session.IsSystemSoundsSession ? "System Sounds" : session.DisplayName;
            if (name == "")
            {
                name = p.ProcessName;
            }

            return new AudioSessionInfo()
            {
                ProcessId = session.GetProcessID,
                IcoPath = ProcessHelper.TryGetProcessIconFilename(p),
                Name = name,
                Volume = session.SimpleAudioVolume.Volume,
            };
        }
    }
}
