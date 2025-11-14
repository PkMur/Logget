using LogGet.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;

namespace LogGet.Services;

public class MongoMotoristaService : IMotoristaService
{
    private readonly IMongoCollection<BsonDocument> _collection;

    public MongoMotoristaService(IOptions<MongoEntregaService.MongoSettings> options)
    {
        var settings = options.Value;
        var client = new MongoClient(settings.ConnectionString);
        var db = client.GetDatabase(settings.DatabaseName);
        _collection = db.GetCollection<BsonDocument>("motoristas");
    }

    public IEnumerable<MotoristaViewModel> ListAll(string? query = null)
    {
        var filter = FilterDefinition<BsonDocument>.Empty;
        if (!string.IsNullOrWhiteSpace(query))
        {
            var q = query.Trim();
            filter = Builders<BsonDocument>.Filter.Or(
                Builders<BsonDocument>.Filter.Regex("Nome", q),
                Builders<BsonDocument>.Filter.Regex("CPF", q),
                Builders<BsonDocument>.Filter.Regex("Veiculo", q)
            );
        }

        var docs = _collection.Find(filter).ToList();
        var result = new List<MotoristaViewModel>();
        foreach (var doc in docs)
        {
            var m = new MotoristaViewModel();
            // mapear id de forma robusta
            if (doc.Contains("_id"))
            {
                var idVal = doc["_id"];
                if (idVal.BsonType == BsonType.ObjectId)
                {
                    m.Id = idVal.AsObjectId.ToString();
                }
                else if (idVal.BsonType == BsonType.Binary)
                {
                    try
                    {
                        var bytes = idVal.AsBsonBinaryData.Bytes;
                        m.Id = new Guid(bytes).ToString();
                    }
                    catch
                    {
                        m.Id = idVal.ToString();
                    }
                }
                else
                {
                    m.Id = idVal.ToString();
                }
            }

            m.Nome = doc.GetValue("Nome", BsonString.Create(string.Empty)).AsString;
            m.CPF = doc.GetValue("CPF", BsonString.Create(string.Empty)).AsString;
            m.RG = doc.GetValue("RG", BsonString.Create(string.Empty)).AsString;
            m.Email = doc.GetValue("Email", BsonString.Create(string.Empty)).AsString;
            m.TipoHabilitacao = doc.GetValue("TipoHabilitacao", BsonString.Create(string.Empty)).AsString;
            m.NumeroCnh = doc.GetValue("NumeroCnh", BsonString.Create(string.Empty)).AsString;
            m.Veiculo = doc.GetValue("Veiculo", BsonString.Create(string.Empty)).AsString;
            m.Login = doc.GetValue("Login", BsonString.Create(string.Empty)).AsString;
            m.Senha = doc.GetValue("Senha", BsonString.Create(string.Empty)).AsString;
            m.IsActive = doc.GetValue("IsActive", BsonBoolean.Create(true)).AsBoolean;

            result.Add(m);
        }

        return result;
    }

    public void Add(MotoristaViewModel motorista)
    {
        // Hashear a senha antes de persistir
        var hasher = new PasswordHasher<object>();
        var senhaHash = string.Empty;
        if (!string.IsNullOrEmpty(motorista.Senha))
        {
            senhaHash = hasher.HashPassword(null, motorista.Senha);
        }

        var doc = new BsonDocument
        {
            { "Nome", motorista.Nome },
            { "CPF", motorista.CPF },
            { "RG", motorista.RG },
            { "Email", motorista.Email },
            { "TipoHabilitacao", motorista.TipoHabilitacao },
            { "NumeroCnh", motorista.NumeroCnh },
            { "Veiculo", motorista.Veiculo },
            { "Login", motorista.Login },
            { "Senha", senhaHash },
            { "IsActive", motorista.IsActive }
        };

        _collection.InsertOne(doc);
    }

    public void Update(MotoristaViewModel motorista)
    {
        if (string.IsNullOrWhiteSpace(motorista.Id)) throw new InvalidOperationException("Id do motorista é obrigatório.");

        var filter = Builders<BsonDocument>.Filter.Eq("_id", new ObjectId(motorista.Id));
        var update = new List<UpdateDefinition<BsonDocument>>
        {
            Builders<BsonDocument>.Update.Set("Nome", motorista.Nome),
            Builders<BsonDocument>.Update.Set("CPF", motorista.CPF),
            Builders<BsonDocument>.Update.Set("RG", motorista.RG),
            Builders<BsonDocument>.Update.Set("Email", motorista.Email),
            Builders<BsonDocument>.Update.Set("TipoHabilitacao", motorista.TipoHabilitacao),
            Builders<BsonDocument>.Update.Set("NumeroCnh", motorista.NumeroCnh),
            Builders<BsonDocument>.Update.Set("Veiculo", motorista.Veiculo),
            Builders<BsonDocument>.Update.Set("Login", motorista.Login),
            Builders<BsonDocument>.Update.Set("IsActive", motorista.IsActive)
        };

        if (!string.IsNullOrEmpty(motorista.Senha))
        {
            update.Add(Builders<BsonDocument>.Update.Set("Senha", motorista.Senha));
        }

        var combined = Builders<BsonDocument>.Update.Combine(update);
        _collection.UpdateOne(filter, combined);
    }

    public bool ExistsByCpf(string cpf)
    {
        if (string.IsNullOrWhiteSpace(cpf)) return false;
        var norm = new string(cpf.Where(char.IsDigit).ToArray());
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
        if (string.IsNullOrWhiteSpace(id)) throw new InvalidOperationException("Id do motorista é obrigatório.");

        // Buscar motorista
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
