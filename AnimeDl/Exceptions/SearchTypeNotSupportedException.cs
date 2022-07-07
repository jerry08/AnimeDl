namespace AnimeDl.Exceptions;

/// <summary>
/// Exception thrown when the search type is not supported.
/// </summary>
public class SearchFilterNotSupportedException : AnimeDlException
{
    /// <summary>
    /// Initializes an instance of <see cref="SearchFilterNotSupportedException"/>.
    /// </summary>
    public SearchFilterNotSupportedException(string message) : base(message)
    {
    }
}