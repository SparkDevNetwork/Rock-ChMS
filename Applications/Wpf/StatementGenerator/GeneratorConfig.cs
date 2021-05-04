namespace Rock.Apps.StatementGenerator
{
    public class GeneratorConfig
    {
        /// <summary>
        /// The number of times that the generation process has re-started. This will default to 1 (one being the first run).
        /// </summary>
        /// <value>
        /// The run attempts.
        /// </value>
        public int RunAttempts { get; set; }
    }
}
