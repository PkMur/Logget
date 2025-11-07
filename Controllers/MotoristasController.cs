using LogGet.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LogGet.Controllers;

[Authorize]
public class MotoristasController : Controller
{
    private readonly Services.IMotoristaService _service;

    public MotoristasController(Services.IMotoristaService service)
    {
        _service = service;
    }

    [HttpGet]
    public IActionResult Index(string? q)
    {
        var items = _service.ListAll(q);
        ViewBag.Query = q ?? string.Empty;
        return View(items);
    }

    [HttpGet]
    public IActionResult Create()
    {
        return View(new MotoristaViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Create(MotoristaViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        try
        {
            if (_service.ExistsByCpf(model.CPF))
            {
                ModelState.AddModelError("CPF", "CPF já cadastrado no sistema.");
                return View(model);
            }

            _service.Add(model);
            TempData["SuccessMessage"] = "Motorista cadastrado.";
            return RedirectToAction("Index");
        }
        catch (Exception ex)
        {
            ModelState.AddModelError(string.Empty, "Ocorreu um erro ao salvar o motorista: " + ex.Message);
            return View(model);
        }
    }
}


