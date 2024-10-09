namespace StickyKeysAgent
{
    public class ConfigSettings
    {
        public bool StickyKeysOn { get; set; }
        public bool HotKeyActive { get; set; }
        public bool ConfirmHotKey { get; set; }
        public bool HotKeySound { get; set; }
        public bool AudibleFeedback { get; set; }
        public bool TriState { get; set; }
        public bool TwoKeysOff { get; set; }
        public bool TaskIndicator { get; set; }
        public bool Autostart { get; set; } // New property for autostart
        public bool FirstRun { get; set; } = true; // New property to track first run
    }
}
