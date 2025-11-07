using LogGet.Models;

namespace LogGet.Services;

public interface IUserService
{
    ApplicationUser? ValidateCredentials(string login, string senha);
}

