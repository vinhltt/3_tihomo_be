namespace CoreFinance.Domain.Exceptions;

/// <summary>
///     Exception thrown when an update operation fails. (EN)<br />
///     Ngoại lệ được ném ra khi thao tác cập nhật thất bại. (VI)
/// </summary>
[Serializable]
public class UpdateFailedException : Exception
{
    public UpdateFailedException()
    {
    }

    public UpdateFailedException(string message) : base(message)
    {
    }

    public UpdateFailedException(string message, Exception innerException) : base(message, innerException)
    {
    }
}