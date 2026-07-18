namespace BZApp.Utilities
{
    [System.Serializable]
    public struct LogOptions
    {
        public bool ShowDebugLogs;
        public bool ShowInfoLogs;
        public bool ShowWarningLogs;
        
        // Note: ShowErrorLogs is removed as error logs should not be ignored
        //public bool ShowErrorLogs;
    }
}