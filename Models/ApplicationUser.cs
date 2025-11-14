namespace LogGet.Models;

public class ApplicationUser
{
    // Usar string para acomodar ObjectId ou outros formatos de id provenientes da persistência.
    public string Id { get; init; } = string.Empty;
    public string Login { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public IReadOnlyCollection<string> Roles { get; init; } = Array.Empty<string>();
}

