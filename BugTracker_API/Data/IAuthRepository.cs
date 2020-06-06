using System.Threading.Tasks;

public interface IAuthRepository
{
    Task<int> Register(User user, string password);
    Task<string> Login(string username, string password);
    Task<bool> UserExists(string username);
}