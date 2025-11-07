using LogGet.Models;
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
            // map id robustly
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

            result.Add(m);
        }

        return result;
    }

    public void Add(MotoristaViewModel motorista)
    {
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
            { "Senha", motorista.Senha }
        };

        _collection.InsertOne(doc);
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
}
