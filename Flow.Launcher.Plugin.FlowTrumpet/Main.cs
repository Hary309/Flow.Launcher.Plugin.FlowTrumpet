using CoreAudio;
using Flow.Launcher.Plugin;
using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;


namespace Flow.Launcher.Plugin.FlowTrumpet
{
    public class FlowTrumpet : IPlugin
    {
        private MMDevice _device;
        private PluginInitContext _context;

        private readonly SessionManager _sessionManager = new();
        private readonly SessionFinder _sessionFinder;
        private readonly SessionController _sessionController;

        public FlowTrumpet()
        {
            _sessionFinder = new(_sessionManager);
            _sessionController = new(_sessionManager);
        }

        public void Init(PluginInitContext context)
        {
            _context = context;

            CoreAudio.MMDeviceEnumerator devEnum = new CoreAudio.MMDeviceEnumerator(Guid.NewGuid());

            devEnum.GetDefaultAudioEndpoint(CoreAudio.DataFlow.Render, CoreAudio.Role.Multimedia);

            MMDeviceEnumerator deviceEnumerator = new MMDeviceEnumerator(Guid.NewGuid());

            foreach (var dev in deviceEnumerator.EnumerateAudioEndPoints(DataFlow.Render, DeviceState.Active))
            {
                if (dev.Selected)
                {
                    _device = dev;
                    break;
                }
            }

            if (_device == null)
            {
                throw new Exception("Device not found.");
            }

            _device.AudioSessionManager2.OnSessionCreated += HandleSessionCreated;

            SessionCollection sessions = _device.AudioSessionManager2.Sessions;

            foreach (AudioSessionControl2 session in sessions)
            {
                AddSession(session);
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
                throw new Exception("Cannot find session)");
            }

            if (session.State == AudioSessionState.AudioSessionStateExpired)
            {
                return;
            }

            session.OnStateChanged += OnStateChanged;
        }

        private void AddSession(AudioSessionControl2 session)
        {
            Process p = Process.GetProcessById((int)session.ProcessID);
            var name = session.IsSystemSoundsSession ? "System Sounds" : session.DisplayName;
            if (name == "")
            {
                name = p.ProcessName;
            }

            var sessionInfo = new SessionInfo(name, session.SimpleAudioVolume.MasterVolume, session.ProcessID);
            sessionInfo.AutoCompleteText = $"{_context.CurrentPluginMetadata.ActionKeyword} {session.ProcessID}";
            sessionInfo.Action = _ =>
            {
                _context.API.ChangeQuery($"{_context.CurrentPluginMetadata.ActionKeyword} {session.ProcessID} ");
                return false;
            };

            session.OnSimpleVolumeChanged += (sender, newVolume, newMute) =>
            {
                sessionInfo.SetVolume(newVolume);
            };

            _sessionManager.AddSession(sessionInfo);
        }
        private void OnStateChanged(object sender, AudioSessionState newState)
        {
            switch (newState)
            {
                case AudioSessionState.AudioSessionStateInactive:
                    break;
                case AudioSessionState.AudioSessionStateActive:
                    break;
                case AudioSessionState.AudioSessionStateExpired:
                    break;
            }
        }

        public List<Result> Query(Query query)
        {
            if (query.FirstSearch.Length == 0)
            {
                return _sessionManager.SessionInfos.Cast<Result>().ToList();
            }

            if (query.FirstSearch.All(char.IsDigit))
            {
                var result = _sessionController.Process(query);

                if (result?.Count > 0)
                {
                    return result;
                }
            }

            return _sessionFinder.Process(query);
        }
    }
}