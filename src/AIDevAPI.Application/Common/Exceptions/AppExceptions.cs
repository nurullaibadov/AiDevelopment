namespace AIDevAPI.Application.Common.Exceptions;

public class NotFoundException : Exception
{
    public NotFoundException(string message) : base(message)
    {
    }

    public NotFoundException(string name, object key) : base($"\"{name}\" ({key}) tapılmadı.")
    {
    }
}

public class BadRequestException : Exception
{
    public BadRequestException(string message) : base(message)
    {
    }
}

public class UnauthorizedAppException : Exception
{
    public UnauthorizedAppException(string message) : base(message)
    {
    }
}

public class ForbiddenException : Exception
{
    public ForbiddenException(string message) : base(message)
    {
    }
}

public class ValidationAppException : Exception
{
    public List<string> Errors { get; }

    public ValidationAppException(List<string> errors) : base("Validasiya xətası baş verdi.")
    {
        Errors = errors;
    }
}
