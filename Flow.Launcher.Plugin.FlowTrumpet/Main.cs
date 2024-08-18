using Flow.Launcher.Plugin;
using Flow.Launcher.Plugin.FlowTrumpet.Audio;
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
        private PluginInitContext _context;

        private IAudioSessionManager _audioSessionManager;
        private Controller _sessionController;

        public void Init(PluginInitContext context)
        {
            _context = context;
            
            _audioSessionManager = new AudioSessionManager();
            _sessionController = new Controller(_context.API, _context.CurrentPluginMetadata, _audioSessionManager);
        }

        public List<Result> Query(Query query)
        {
            return _sessionController.ProcessQuery(query.FirstSearch, query.SecondSearch).ToList();
        }
    }
}