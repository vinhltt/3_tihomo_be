namespace CoreFinance.Domain.Exceptions;

/// <summary>
///     Exception thrown when a create operation fails. (EN)<br />
///     Ngoại lệ được ném ra khi thao tác tạo mới thất bại. (VI)
/// </summary>
[Serializable]
public class CreateFailedException : Exception
{
    public CreateFailedException()
    {
    }

    public CreateFailedException(string message) : base(message)
    {
    }

    public CreateFailedException(string message, Exception innerException) : base(message, innerException)
    {
    }
}