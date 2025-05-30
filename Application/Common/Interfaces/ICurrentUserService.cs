namespace Application.Common.Interfaces
{
    public interface ICurrentUserService
    {
        string Email { get; }
        string FirstName { get; }
        string LastName { get; }
        string UserId { get; }
        string UserName { get; }
        string IpAddress { get; }
        bool IsAuthenticated { get; }
        List<string> Claims { get; }
    }
}