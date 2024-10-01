using Microsoft.AspNetCore.Identity;
using TodoApi.Models;

namespace TodoApi.Repositories
{
    public interface ITokenRepository
    {
        string CreateJWTToken(User user, List<string> roles);
    }
}