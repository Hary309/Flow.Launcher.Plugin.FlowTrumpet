using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Flow.Launcher.Plugin.FlowTrumpet
{
    internal class SessionController
    {
        private readonly SessionManager _sessionManager;

        public SessionController(SessionManager sessionManager)
        {
            _sessionManager = sessionManager;
        }

        public List<Result> Process(Query query)
        {
            var sessionInfo = _sessionManager.GetSessionInfo(uint.Parse(query.FirstSearch));

            if (sessionInfo == null)
            {
                return null;
            }

            return new List<Result>
            {
                new Result
                {
                    Title = sessionInfo.Title,
                    SubTitle = "Type number to set new volume",
                }
            };
        }
    }
}
