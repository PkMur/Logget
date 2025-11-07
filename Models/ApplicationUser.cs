namespace LogGet.Models;

public class ApplicationUser
{
    public Guid Id { get; init; }
    public string Login { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public IReadOnlyCollection<string> Roles { get; init; } = Array.Empty<string>();
}

