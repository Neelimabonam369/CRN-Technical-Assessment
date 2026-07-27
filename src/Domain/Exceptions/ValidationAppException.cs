namespace Domain.Exceptions;

// Thrown when application-level validation fails; caught by the API's
// global exception middleware and translated into a 400 response.
public class ValidationAppException : Exception
{
    public IDictionary<string, string[]> Errors { get; }

    public ValidationAppException(IDictionary<string, string[]> errors)
        : base("One or more validation errors occurred.")
    {
        Errors = errors;
    }
}
