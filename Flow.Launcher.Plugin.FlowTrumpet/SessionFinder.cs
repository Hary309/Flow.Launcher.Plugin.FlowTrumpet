using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Flow.Launcher.Plugin.FlowTrumpet
{
    internal class SessionFinder
    {
        readonly SessionManager _sessionManager;

        public SessionFinder(SessionManager sessionManager)
        {
            _sessionManager = sessionManager;
        }

        public List<Result> Process(Query query)
        {
            var firstSearch = query.FirstSearch.ToLower();

            return _sessionManager.SessionInfos
                .Where(x => x.Title.ToLower().StartsWith(firstSearch) || x.ProcessId.ToString().StartsWith(firstSearch))
                .Cast<Result>()
                .ToList();
        }
    }
}
