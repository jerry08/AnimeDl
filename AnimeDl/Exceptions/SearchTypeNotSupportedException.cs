namespace AnimeDl.Exceptions;

/// <summary>
/// Exception thrown when the search type is not supported.
/// </summary>
public class SearchTypeNotSupportedException : AnimeDlException
{
    /// <summary>
    /// Initializes an instance of <see cref="SearchTypeNotSupportedException"/>.
    /// </summary>
    public SearchTypeNotSupportedException(string message) : base(message)
    {
    }
}