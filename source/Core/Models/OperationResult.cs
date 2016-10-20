namespace IdentityServer3.Core.Models
{
    /// <summary>
    /// General operation result.
    /// </summary>
    public class OperationResult
    {
        /// <summary>
        /// Indicates whether the operation is done.
        /// </summary>
        /// <value>
        ///   <c>true</c> if the operation is done; otherwise, <c>false</c>.
        /// </value>
        public bool IsDone { get; private set; }

        /// <summary>
        /// Indicates whether the operation was successful.
        /// </summary>
        /// <value>
        ///   <c>true</c> if the operation failed; otherwise, <c>false</c>.
        /// </value>
        public bool IsError { get { return !string.IsNullOrEmpty(Error); } }

        /// <summary>
        /// Gets the Error if any.
        /// </summary>
        /// <value>
        /// The Error.
        /// </value>
        public string Error { get; private set; }

        /// <summary>
        /// Initializes a successful <see cref="OperationResult"/>.
        /// </summary>
        /// <param name="isDone"><c>true</c> if the operation is done; otherwise, <c>false</c>.</param>
        public OperationResult(bool isDone)
        {
            IsDone = isDone;
        }

        /// <summary>
        /// Initializes an <see cref="OperationResult"/> indicating error.
        /// </summary>
        /// <param name="error">The error message.</param>
        public OperationResult(string error)
        {
            Error = error;
        }
    }
}