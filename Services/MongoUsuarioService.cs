using LogGet.Models;
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
            u.NomeDaMae = doc.GetValue("NomeDaMae", BsonString.Create(string.Empty)).AsString;
            u.Email = doc.GetValue("Email", BsonString.Create(string.Empty)).AsString;
            u.Login = doc.GetValue("Login", BsonString.Create(string.Empty)).AsString;
            u.Senha = doc.GetValue("Senha", BsonString.Create(string.Empty)).AsString;

            result.Add(u);
        }

        return result;
    }

    public void Add(UsuarioViewModel usuario)
    {
        var doc = new BsonDocument
        {
            { "Nome", usuario.Nome },
            { "CPF", usuario.CPF },
            { "RG", usuario.RG },
            { "NomeDaMae", usuario.NomeDaMae },
            { "Email", usuario.Email },
            { "Login", usuario.Login },
            { "Senha", usuario.Senha }
        };

        _collection.InsertOne(doc);
    }

    public bool ExistsByCpf(string cpf)
    {
        if (string.IsNullOrWhiteSpace(cpf)) return false;
        var norm = new string(cpf.Where(char.IsDigit).ToArray());
        // Fetch CPF values and compare normalized (safe for mixed storage formats)
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
}
