namespace Application.Common.Models;

public class Result
{
    internal Result(bool succeeded, IEnumerable<string> errors, IEnumerable<string> messages)
    {
        Succeeded = succeeded;
        Errors = errors;
        Messages = messages;
    }

    public bool Succeeded { get; set; }

    public IEnumerable<string> Errors { get; set; }

    public IEnumerable<string> Messages { get; set; }

    public static Result Success()
    {
        return new Result(true, [], []);
    }

    public static Result Success(string message)
    {
        IEnumerable<string> messages = [message];
        return new Result(true, [], messages);
    }

    public static Result Success(IEnumerable<string> messages)
    {
        return new Result(true, [], messages);
    }

    public static Result Failure(IEnumerable<string> errors)
    {
        return new Result(false, errors, []);
    }

    public static Result Failure(string error)
    {
        IEnumerable<string> errors = [error];
        return new Result(false, errors, []);
    }

    public static Result ExceptionFailure(Exception ex)
    {
        if (ex is null)
        {
            return Failure("No exception description was found.");
        }

        if (ex!.InnerException != null && ex.InnerException.Message != null)
        {
            return Failure(ex.Message + Environment.NewLine + ex.InnerException.Message);
        }

        return Failure(ex.Message);
    }
}
