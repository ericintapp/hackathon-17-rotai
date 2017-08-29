namespace OpenMachineLearningService.Models
{
    /// <summary>
    ///     Defines an input.
    /// </summary>
    public class Input
    {
        #region Public Properties

        /// <summary>
        ///     The input (or feature) ID.
        /// </summary>
        public string InputId { get; set; }

        /// <summary>
        ///     The value of the input.
        /// </summary>
        public string Value { get; set; }

        #endregion
    }
}