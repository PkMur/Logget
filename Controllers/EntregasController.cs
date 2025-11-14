using LogGet.Models;
using LogGet.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LogGet.Controllers;

[Authorize]
public class EntregasController : Controller
{
    private readonly IEntregaService _service;

    public EntregasController(IEntregaService service)
    {
        _service = service;
    }

    [HttpGet]
    public IActionResult Index(string? q)
    {
        var entregas = _service.ListAll(q);
        ViewBag.Query = q ?? string.Empty;
        return View(entregas);
    }

    [HttpGet]
    public IActionResult Create()
    {
        return View(new EntregaCreateViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Create(EntregaCreateViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }
        _service.Add(new Entrega
        {
            // NumeroPedido e CriadoEm são definidos pelo serviço
            DestinatarioNome = model.DestinatarioNome,
            DestinatarioDocumento = model.DestinatarioDocumento,
            EnderecoRua = model.EnderecoRua,
            EnderecoNumero = model.EnderecoNumero,
            EnderecoComplemento = model.EnderecoComplemento,
            EnderecoBairro = model.EnderecoBairro,
            EnderecoCidade = model.EnderecoCidade,
            RemetenteNome = model.RemetenteNome,
            QuantidadeVolumes = model.QuantidadeVolumes,
            Peso = model.Peso
        });

        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public IActionResult Edit(string numero)
    {
        if (string.IsNullOrWhiteSpace(numero)) return NotFound();

        // Normalizar número
        var digits = new string(numero.Where(char.IsDigit).ToArray());
        if (!string.IsNullOrWhiteSpace(digits) && digits.Length <= 4)
        {
            numero = digits.PadLeft(4, '0');
        }

        var entrega = _service.GetByNumeroPedido(numero);
        if (entrega == null) return NotFound();

        return View(entrega);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Edit(Entrega model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        try
        {
            _service.Update(model);
            TempData["SuccessMessage"] = "Entrega atualizada.";
            return RedirectToAction("Details", new { numero = model.NumeroPedido });
        }
        catch (Exception ex)
        {
            ModelState.AddModelError(string.Empty, "Ocorreu um erro ao atualizar a entrega: " + ex.Message);
            return View(model);
        }
    }

    [HttpGet]
    public IActionResult Despacho()
    {
        var items = _service.ListWithoutMotorista();
        ViewBag.Query = string.Empty;
        return View(items);
    }

    [HttpGet]
    public IActionResult CreateDespacho()
    {
    // Exibir formulário para criar um novo despacho: será necessária a lista de motoristas
        var motoristas = HttpContext.RequestServices.GetService<LogGet.Services.IMotoristaService>()?.ListAll();
        ViewBag.Motoristas = motoristas ?? Enumerable.Empty<LogGet.Models.MotoristaViewModel>();
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult CreateDespacho(string motoristaId, List<string> entregas)
    {
        if (string.IsNullOrWhiteSpace(motoristaId) || entregas == null || entregas.Count == 0)
        {
            TempData["ErrorMessage"] = "Selecione um motorista e adicione pelo menos uma entrega.";
            return RedirectToAction("CreateDespacho");
        }

        var motoristaService = HttpContext.RequestServices.GetService<LogGet.Services.IMotoristaService>();
    var motorista = motoristaService?.ListAll().FirstOrDefault(m => m.Id == motoristaId);
        if (motorista == null)
        {
            TempData["ErrorMessage"] = "Motorista não encontrado.";
            return RedirectToAction("CreateDespacho");
        }

        var errors = new List<string>();
        var usuarioNome = User.Identity?.Name ?? "Sistema";
        
        foreach (var num in entregas.Distinct())
        {
            if (!_service.AssignMotorista(num, motorista.Id, motorista.Nome, usuarioNome, out var error))
            {
                if (!string.IsNullOrWhiteSpace(error)) errors.Add(error);
            }
        }

        if (errors.Count > 0)
        {
            TempData["ErrorMessage"] = string.Join("; ", errors);
        }

    TempData["SuccessMessage"] = "Expedição criada.";
        return RedirectToAction("Despacho");
    }

    [HttpGet]
    public IActionResult Details(string numero)
    {
        if (string.IsNullOrWhiteSpace(numero)) return NotFound();

        // Normalizar: extrair dígitos e preencher para 4 casas (ex.: 1 -> 0001)
        var digits = new string(numero.Where(char.IsDigit).ToArray());
        if (!string.IsNullOrWhiteSpace(digits) && digits.Length <= 4)
        {
            numero = digits.PadLeft(4, '0');
        }

        var entrega = _service.GetByNumeroPedido(numero);
        if (entrega == null) return NotFound();
        return View(entrega);
    }

    [HttpGet]
    public IActionResult Exists(string numero)
    {
        if (string.IsNullOrWhiteSpace(numero)) return Json(new { exists = false });
        var entrega = _service.GetByNumeroPedido(numero);
        return Json(new { exists = entrega != null });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult MarcarComoEntregue(string numero)
    {
        if (string.IsNullOrWhiteSpace(numero))
        {
            TempData["ErrorMessage"] = "Número da entrega não informado.";
            return RedirectToAction("Index");
        }

        // Obter nome do usuário autenticado
        var usuarioNome = User.Identity?.Name ?? "Sistema";

        if (_service.MarcarComoEntregue(numero, usuarioNome, out var error))
        {
            TempData["SuccessMessage"] = "Entrega marcada como entregue.";
        }
        else
        {
            TempData["ErrorMessage"] = error ?? "Erro ao marcar entrega como entregue.";
        }

        return RedirectToAction("Details", new { numero });
    }
}


