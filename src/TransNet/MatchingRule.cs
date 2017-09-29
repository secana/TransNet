namespace TransNet
{
    /// <summary>
    /// Matching rule for additional properties.
    /// </summary>
    public enum MatchingRule
    {
        /// <summary>
        /// The attribute will be used to distinguish two entities of the same type
        /// in Maltego.
        /// </summary>
        Strict,

        /// <summary>
        /// The attribute will *not* be used to distinguish two entities of the same type
        /// in Maltego.
        /// </summary>
        Loose
    }
}