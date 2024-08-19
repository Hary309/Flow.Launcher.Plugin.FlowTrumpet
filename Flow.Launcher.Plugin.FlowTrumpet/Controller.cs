using Flow.Launcher.Plugin.FlowTrumpet.Audio;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Flow.Launcher.Plugin.FlowTrumpet
{
    internal class Controller
    {
        private readonly IPublicAPI _publicAPI;
        private readonly PluginMetadata _pluginMetadata;
        private readonly IAudioSessionManager _audioSessionManager;

        private float? _newVolume;

        public Controller(IPublicAPI publicAPI, PluginMetadata pluginMetadata, IAudioSessionManager audioSessionManager)
        {
            _publicAPI = publicAPI;
            _pluginMetadata = pluginMetadata;
            _audioSessionManager = audioSessionManager;
        }

        public IEnumerable<Result> GetDefaultSessionList()
        {
            var sessionsInfo = _audioSessionManager.GetSessionsInfo();

            foreach (var session in sessionsInfo)
            {
                var result = CreateResult(session);
                result.Action = _ =>
                {
                    _publicAPI.ChangeQuery($"{_pluginMetadata.ActionKeyword} {session.ProcessId} ");
                    return false;
                };

                yield return result;
            }

            if (sessionsInfo.Count() == 0)
            {
                yield return new Result()
                {
                    Title = "No active application"
                };
            }
        }

        public IEnumerable<Result> GetSessionOptions(uint processId, Action<AudioSessionInfo> onActivated)
        {
            var sessionInfo = _audioSessionManager.GetSessionInfo(processId);

            if (sessionInfo == null)
            {
                return new List<Result>();
            }

            var result = CreateResult(sessionInfo);

            result.SubTitle = "Type number to set new volume";
            result.Action = _ =>
            {
                onActivated(sessionInfo);
                return true;
            };

            return new List<Result>() { result };
        }

        public IEnumerable<Result> ProcessQuery(string firstSearch, string secondSearch)
        {
            if (firstSearch.Length == 0)
            {
                return GetDefaultSessionList().ToList();
            }

            if (firstSearch.All(char.IsDigit))
            {
                if (secondSearch.Length > 0 && secondSearch.All(char.IsDigit))
                {
                    _newVolume = uint.Parse(secondSearch) / 100.0f;
                }
                else
                {
                    _newVolume = null;
                }

                var results = GetSessionOptions(uint.Parse(firstSearch), x =>
                {
                    if (_newVolume != null)
                    {
                        _audioSessionManager.SetSessionVolume(x.ProcessId, _newVolume.Value);
                    }
                }).ToList();

                if (results.Count > 0)
                {
                    return results;
                }
            }

            var lowerFirstSearch = firstSearch.ToLower();

            var filtered = GetDefaultSessionList().Where(x =>
                x.Title.ToLower().StartsWith(lowerFirstSearch) || x.SubTitle.Contains(lowerFirstSearch));

            if (filtered.Count() == 0)
            {
                return new List<Result>()
                {
                    new Result()
                    {
                        Title = "Process not found"
                    }
                };
            }
            else
            {
                return filtered;
            }
        }

        private Result CreateResult(AudioSessionInfo session)
        {
            return new Result
            {
                Title = $"{session.Name} - {(uint)(session.Volume * 100)}%",
                SubTitle = $"Id: {session.ProcessId}",
            };
        }
    }
}
