

// ReSharper disable once CheckNamespace
namespace Blog.Services
{
    public interface IUserServices
    {
        bool ValidateUser(string username, string password);
    }
}
