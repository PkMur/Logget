using LogGet.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LogGet.Controllers;

[Authorize]
public class UsuariosController : Controller
{
    private readonly Services.IUsuarioService _service;

    public UsuariosController(Services.IUsuarioService service)
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
        return View(new UsuarioViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Create(UsuarioViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        // Duplicate CPF check (normalize digits only)
        try
        {
            if (_service.ExistsByCpf(model.CPF))
            {
                ModelState.AddModelError("CPF", "CPF já cadastrado no sistema.");
                return View(model);
            }

            _service.Add(model);
            TempData["SuccessMessage"] = "Usuário cadastrado.";
            return RedirectToAction("Index");
        }
        catch (Exception ex)
        {
            // Log and show friendly error
            ModelState.AddModelError(string.Empty, "Ocorreu um erro ao salvar o usuário: " + ex.Message);
            return View(model);
        }
    }
}


