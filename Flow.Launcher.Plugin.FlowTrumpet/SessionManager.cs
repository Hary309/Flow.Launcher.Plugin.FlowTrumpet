using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Flow.Launcher.Plugin.FlowTrumpet
{
    internal class SessionManager
    {
        private List<SessionInfo> _sessionInfos = new();

        public List<SessionInfo> SessionInfos => _sessionInfos;

        public void AddSession(SessionInfo sessionInfo)
        {
            _sessionInfos.Add(sessionInfo);
        }

        public SessionInfo GetSessionInfo(uint processId)
        {
            return _sessionInfos.SingleOrDefault(x => x.ProcessId == processId);
        }

        public void RemoveSession(uint processId)
        {
            var session = GetSessionInfo(processId);

            if (session != null)
            {
                _sessionInfos.Remove(session);
            }
        }
    }
}
