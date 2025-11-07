using LogGet.Models;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace LogGet.Services;

public class MongoEntregaService : IEntregaService
{
    private readonly IMongoCollection<Entrega> _collection;

    public class MongoSettings
    {
        public string ConnectionString { get; set; } = string.Empty;
        public string DatabaseName { get; set; } = "LogGet";
    }

    public MongoEntregaService(IOptions<MongoSettings> options)
    {
        var settings = options.Value;
        var client = new MongoClient(settings.ConnectionString);
        var db = client.GetDatabase(settings.DatabaseName);
        _collection = db.GetCollection<Entrega>("entregas");
    }

    public IEnumerable<Entrega> ListAll(string? query = null)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            return _collection.Find(FilterDefinition<Entrega>.Empty)
                .SortByDescending(e => e.CriadoEm)
                .ToList();
        }

        var filter = Builders<Entrega>.Filter.Or(
            Builders<Entrega>.Filter.Regex(e => e.NumeroPedido, query),
            Builders<Entrega>.Filter.Regex(e => e.DestinatarioNome, query),
            Builders<Entrega>.Filter.Regex(e => e.EnderecoRua, query),
            Builders<Entrega>.Filter.Regex(e => e.EnderecoCidade, query)
        );

        return _collection.Find(filter).SortByDescending(e => e.CriadoEm).ToList();
    }

    public Entrega? GetByNumeroPedido(string numeroPedido)
    {
        return _collection.Find(e => e.NumeroPedido == numeroPedido).FirstOrDefault();
    }

    public IEnumerable<Entrega> ListWithoutMotorista(string? query = null)
    {
        var filter = Builders<Entrega>.Filter.Or(
            Builders<Entrega>.Filter.Eq(e => e.MotoristaId, string.Empty),
            Builders<Entrega>.Filter.Eq(e => e.MotoristaId, null as string)
        );

        if (!string.IsNullOrWhiteSpace(query))
        {
            var q = query.Trim();
            filter = Builders<Entrega>.Filter.And(filter, Builders<Entrega>.Filter.Or(
                Builders<Entrega>.Filter.Regex(e => e.NumeroPedido, q),
                Builders<Entrega>.Filter.Regex(e => e.DestinatarioNome, q),
                Builders<Entrega>.Filter.Regex(e => e.RemetenteNome, q)
            ));
        }

        return _collection.Find(filter).SortByDescending(e => e.CriadoEm).ToList();
    }

    public bool AssignMotorista(string numeroPedido, string motoristaId, string motoristaNome, out string? error)
    {
        error = null;
        var filter = Builders<Entrega>.Filter.Eq(e => e.NumeroPedido, numeroPedido);
        var entrega = _collection.Find(filter).FirstOrDefault();
        if (entrega == null)
        {
            error = $"Entrega {numeroPedido} não encontrada.";
            return false;
        }

        if (!string.IsNullOrWhiteSpace(entrega.MotoristaId))
        {
            error = $"Entrega {numeroPedido} já possui motorista vinculado.";
            return false;
        }

        // Update motorista, status and add a movimentacao
        var movimentacao = new Movimentacao { Status = "Em rota", Data = DateTime.UtcNow, Autor = motoristaNome, Observacao = "Despachado" };
        var update = Builders<Entrega>.Update
            .Set(e => e.MotoristaId, motoristaId)
            .Set(e => e.MotoristaNome, motoristaNome)
            .Set(e => e.Status, "Em rota")
            .Push(e => e.Movimentacoes, movimentacao);

        _collection.UpdateOne(filter, update);
        return true;
    }

    public void Add(Entrega entrega)
    {
        // Ensure NumeroPedido is assigned when not provided. Use a simple count-based sequence.
        if (string.IsNullOrWhiteSpace(entrega.NumeroPedido))
        {
            var count = (int)_collection.CountDocuments(FilterDefinition<Entrega>.Empty);
            var next = count + 1;
            entrega.NumeroPedido = $"ENT{next:0000}";
        }

        if (entrega.CriadoEm == default) entrega.CriadoEm = DateTime.UtcNow;
        if (string.IsNullOrWhiteSpace(entrega.Status)) entrega.Status = "Criada";
        if (entrega.Movimentacoes == null) entrega.Movimentacoes = new List<Movimentacao>();
        entrega.Movimentacoes.Add(new Movimentacao { Status = entrega.Status, Data = DateTime.UtcNow, Autor = "Sistema", Observacao = "Registro de entrega" });

        _collection.InsertOne(entrega);
    }
}


