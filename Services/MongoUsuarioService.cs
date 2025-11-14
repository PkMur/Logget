using LogGet.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;

namespace LogGet.Services;

public class MongoUsuarioService : IUsuarioService
{
    private readonly IMongoCollection<BsonDocument> _collection;

    public MongoUsuarioService(IOptions<MongoEntregaService.MongoSettings> options)
    {
        var settings = options.Value;
        var client = new MongoClient(settings.ConnectionString);
        var db = client.GetDatabase(settings.DatabaseName);
        _collection = db.GetCollection<BsonDocument>("usuarios");
    }

    public IEnumerable<UsuarioViewModel> ListAll(string? query = null)
    {
        var filter = FilterDefinition<BsonDocument>.Empty;
        if (!string.IsNullOrWhiteSpace(query))
        {
            var q = query.Trim();
            filter = Builders<BsonDocument>.Filter.Or(
                Builders<BsonDocument>.Filter.Regex("Nome", q),
                Builders<BsonDocument>.Filter.Regex("CPF", q),
                Builders<BsonDocument>.Filter.Regex("Email", q)
            );
        }

        var docs = _collection.Find(filter).ToList();
        var result = new List<UsuarioViewModel>();
        foreach (var doc in docs)
        {
            var u = new UsuarioViewModel();
            if (doc.Contains("_id"))
            {
                var idVal = doc["_id"];
                if (idVal.BsonType == BsonType.ObjectId)
                {
                    u.Id = idVal.AsObjectId.ToString();
                }
                else if (idVal.BsonType == BsonType.Binary)
                {
                    try { u.Id = new Guid(idVal.AsBsonBinaryData.Bytes).ToString(); }
                    catch { u.Id = idVal.ToString(); }
                }
                else
                {
                    u.Id = idVal.ToString();
                }
            }

            u.Nome = doc.GetValue("Nome", BsonString.Create(string.Empty)).AsString;
            u.CPF = doc.GetValue("CPF", BsonString.Create(string.Empty)).AsString;
            u.RG = doc.GetValue("RG", BsonString.Create(string.Empty)).AsString;
            u.Email = doc.GetValue("Email", BsonString.Create(string.Empty)).AsString;
            u.Login = doc.GetValue("Login", BsonString.Create(string.Empty)).AsString;
            u.Senha = doc.GetValue("Senha", BsonString.Create(string.Empty)).AsString;
            u.Telefone = doc.GetValue("Telefone", BsonString.Create(string.Empty)).AsString;
            u.IsActive = doc.GetValue("IsActive", BsonBoolean.Create(true)).AsBoolean;

            result.Add(u);
        }

        return result;
    }

    public void Add(UsuarioViewModel usuario)
    {
    // Hashear a senha antes de persistir
        var hasher = new PasswordHasher<object>();
        var senhaHash = string.Empty;
        if (!string.IsNullOrEmpty(usuario.Senha))
        {
            senhaHash = hasher.HashPassword(null, usuario.Senha);
        }

        var doc = new BsonDocument
        {
            { "Nome", usuario.Nome },
            { "CPF", usuario.CPF },
            { "RG", usuario.RG },
            { "Email", usuario.Email },
            { "Login", usuario.Login },
            { "Senha", senhaHash },
            { "Telefone", usuario.Telefone },
            { "IsActive", usuario.IsActive }
        };

        _collection.InsertOne(doc);
    }

    public void Update(UsuarioViewModel usuario)
    {
        if (string.IsNullOrWhiteSpace(usuario.Id)) throw new InvalidOperationException("Id do usuário é obrigatório.");

        var filter = Builders<BsonDocument>.Filter.Eq("_id", new ObjectId(usuario.Id));
        var update = new List<UpdateDefinition<BsonDocument>>
        {
            Builders<BsonDocument>.Update.Set("Nome", usuario.Nome),
            Builders<BsonDocument>.Update.Set("CPF", usuario.CPF),
            Builders<BsonDocument>.Update.Set("RG", usuario.RG),
            Builders<BsonDocument>.Update.Set("Email", usuario.Email),
            Builders<BsonDocument>.Update.Set("Login", usuario.Login),
            Builders<BsonDocument>.Update.Set("Telefone", usuario.Telefone),
            Builders<BsonDocument>.Update.Set("IsActive", usuario.IsActive)
        };

    // Se senha fornecida, hashear e atualizar; caso contrário não alterar o campo Senha
        if (!string.IsNullOrEmpty(usuario.Senha))
        {
            var hasher = new PasswordHasher<object>();
            var senhaHash = hasher.HashPassword(null, usuario.Senha);
            update.Add(Builders<BsonDocument>.Update.Set("Senha", senhaHash));
        }

        var combined = Builders<BsonDocument>.Update.Combine(update);
        _collection.UpdateOne(filter, combined);
    }

    public bool ExistsByCpf(string cpf)
    {
        if (string.IsNullOrWhiteSpace(cpf)) return false;
        var norm = new string(cpf.Where(char.IsDigit).ToArray());
    // Buscar valores de CPF e comparar normalizados (seguros para formatos mistos de armazenamento)
        var docs = _collection.Find(FilterDefinition<BsonDocument>.Empty).Project(Builders<BsonDocument>.Projection.Include("CPF").Exclude("_id")).ToList();
        foreach (var d in docs)
        {
            if (d.Contains("CPF"))
            {
                var stored = d.GetValue("CPF").AsString;
                var sNorm = new string(stored.Where(char.IsDigit).ToArray());
                if (sNorm == norm) return true;
            }
        }

        return false;
    }

    public bool ChangePassword(string id, string senhaAtual, string novaSenha)
    {
        if (string.IsNullOrWhiteSpace(id)) throw new InvalidOperationException("Id do usuário é obrigatório.");

        // Buscar usuário
        var filter = Builders<BsonDocument>.Filter.Eq("_id", new ObjectId(id));
        var doc = _collection.Find(filter).FirstOrDefault();
        if (doc is null) return false;

        // Verificar senha atual
        var senhaArmazenada = doc.GetValue("Senha", BsonString.Create(string.Empty)).AsString;
        var hasher = new PasswordHasher<object>();
        var verificationResult = hasher.VerifyHashedPassword(null, senhaArmazenada, senhaAtual);
        
        if (verificationResult == PasswordVerificationResult.Failed)
        {
            return false; // Senha atual incorreta
        }

        // Hashear nova senha e atualizar
        var novaSenhaHash = hasher.HashPassword(null, novaSenha);
        var update = Builders<BsonDocument>.Update.Set("Senha", novaSenhaHash);
        _collection.UpdateOne(filter, update);

        return true;
    }
}
