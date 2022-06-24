using System;

namespace AnimeDl.Exceptions;

/// <summary>
/// Exception thrown within <see cref="AnimeDl"/>.
/// </summary>
public class AnimeDlException : Exception
{
    /// <summary>
    /// Initializes an instance of <see cref="AnimeDlException"/>.
    /// </summary>
    /// <param name="message"></param>
    public AnimeDlException(string message) : base(message)
    {
    }
}