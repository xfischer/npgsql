namespace edb_rebrand
{
    internal struct Report
    {
        public Report() { }
        private Report(int filesProcessed, int directoryProcessed, int filesIgnored)
        {
            numFilesProcessed= filesProcessed;
            numDirectoryProcessed= directoryProcessed;
            numFilesIgnored= filesIgnored;
        }
        public int numFilesProcessed { get; internal set; }
        public int numDirectoryProcessed { get; internal set; }
        public int numFilesIgnored { get; internal set; }

        internal Dictionary<string, (string NewContent, int NumOccurences)> NewContentsByFile { get; set; } = new();

        public static Report operator +(Report a, Report b)
        => new Report(a.numFilesProcessed + b.numFilesProcessed,
                        a.numDirectoryProcessed + b.numDirectoryProcessed,
                        a.numFilesIgnored + b.numFilesIgnored)
        { NewContentsByFile = new(a.NewContentsByFile.Concat(b.NewContentsByFile)) };
    }
    
}