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

    public bool AssignMotorista(string numeroPedido, string motoristaId, string motoristaNome, string usuarioAutor, out string? error)
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

    // Atualizar motorista, status e adicionar uma movimentação
        var movimentacao = new Movimentacao { Status = "Em rota", Data = DateTime.UtcNow, Autor = usuarioAutor, Observacao = $"Despachado para {motoristaNome}" };
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
    // Garantir que NumeroPedido seja atribuído quando não fornecido. Usa uma sequência simples baseada em contagem.
        if (string.IsNullOrWhiteSpace(entrega.NumeroPedido))
        {
            var count = (int)_collection.CountDocuments(FilterDefinition<Entrega>.Empty);
            var next = count + 1;
            entrega.NumeroPedido = $"{next:0000}";
        }

        if (entrega.CriadoEm == default) entrega.CriadoEm = DateTime.UtcNow;
        if (string.IsNullOrWhiteSpace(entrega.Status)) entrega.Status = "Criada";
        if (entrega.Movimentacoes == null) entrega.Movimentacoes = new List<Movimentacao>();
        entrega.Movimentacoes.Add(new Movimentacao { Status = entrega.Status, Data = DateTime.UtcNow, Autor = "Sistema", Observacao = "Registro de entrega" });

        _collection.InsertOne(entrega);
    }

    public void Update(Entrega entrega)
    {
        if (string.IsNullOrWhiteSpace(entrega.Id))
            throw new InvalidOperationException("Id da entrega é obrigatório.");

        var filter = Builders<Entrega>.Filter.Eq(e => e.Id, entrega.Id);
        var existing = _collection.Find(filter).FirstOrDefault();
        if (existing == null)
            throw new InvalidOperationException("Entrega não encontrada.");

        // Atualizar apenas campos editáveis (não altera status, motorista, movimentações)
        var update = Builders<Entrega>.Update
            .Set(e => e.DestinatarioNome, entrega.DestinatarioNome)
            .Set(e => e.DestinatarioDocumento, entrega.DestinatarioDocumento)
            .Set(e => e.EnderecoRua, entrega.EnderecoRua)
            .Set(e => e.EnderecoNumero, entrega.EnderecoNumero)
            .Set(e => e.EnderecoComplemento, entrega.EnderecoComplemento)
            .Set(e => e.EnderecoBairro, entrega.EnderecoBairro)
            .Set(e => e.EnderecoCidade, entrega.EnderecoCidade)
            .Set(e => e.RemetenteNome, entrega.RemetenteNome)
            .Set(e => e.QuantidadeVolumes, entrega.QuantidadeVolumes)
            .Set(e => e.Peso, entrega.Peso);

        _collection.UpdateOne(filter, update);
    }

    public bool MarcarComoEntregue(string numeroPedido, string usuarioAutor, out string? error)
    {
        error = null;
        var filter = Builders<Entrega>.Filter.Eq(e => e.NumeroPedido, numeroPedido);
        var entrega = _collection.Find(filter).FirstOrDefault();
        if (entrega == null)
        {
            error = $"Entrega {numeroPedido} não encontrada.";
            return false;
        }

        if (entrega.Status == "Entregue")
        {
            error = $"Entrega {numeroPedido} já foi finalizada.";
            return false;
        }

        // Atualizar status e adicionar movimentação
        var movimentacao = new Movimentacao 
        { 
            Status = "Entregue", 
            Data = DateTime.UtcNow, 
            Autor = usuarioAutor, 
            Observacao = "Entrega finalizada" 
        };
        
        var update = Builders<Entrega>.Update
            .Set(e => e.Status, "Entregue")
            .Push(e => e.Movimentacoes, movimentacao);

        _collection.UpdateOne(filter, update);
        return true;
    }
}


