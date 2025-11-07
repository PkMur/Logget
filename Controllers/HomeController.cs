using System.Diagnostics;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using LogGet.Models;

namespace LogGet.Controllers;

[
    Authorize
]
public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly Services.IEntregaService _entregaService;
    private readonly Services.IMotoristaService _motoristaService;
    private readonly Services.IUsuarioService _usuarioService;

    public HomeController(ILogger<HomeController> logger, Services.IEntregaService entregaService, Services.IMotoristaService motoristaService, Services.IUsuarioService usuarioService)
    {
        _logger = logger;
        _entregaService = entregaService;
        _motoristaService = motoristaService;
        _usuarioService = usuarioService;
    }

    public IActionResult Index()
    {
        var entregas = _entregaService.ListAll();
        var semMotorista = _entregaService.ListWithoutMotorista();
        var motoristas = _motoristaService.ListAll();
        var usuarios = _usuarioService.ListAll();

        var model = new HomeDashboardViewModel
        {
            TotalEntregas = entregas.Count(),
            EntregasSemMotorista = semMotorista.Count(),
            TotalMotoristas = motoristas.Count(),
            TotalUsuarios = usuarios.Count()
        };

        return View(model);
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
