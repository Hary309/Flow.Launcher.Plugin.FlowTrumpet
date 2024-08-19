namespace Flow.Launcher.Plugin.FlowTrumpet.Audio
{
    internal class AudioSessionInfo
    {
        public string Name { get; init; }
        public uint ProcessId { get; init; }
        public float Volume { get; set; }
        public string IcoPath { get; init; }
    }
}
