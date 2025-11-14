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

    [HttpGet]
    public IActionResult Edit(string id)
    {
        var motorista = _service.ListAll().FirstOrDefault(m => m.Id == id);
        if (motorista is null) return NotFound();

        motorista.Senha = string.Empty;
        return View(motorista);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Edit(MotoristaViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        try
        {
            var existingWithCpf = _service.ListAll().FirstOrDefault(m => m.CPF != null && new string(m.CPF.Where(char.IsDigit).ToArray()) == new string(model.CPF.Where(char.IsDigit).ToArray()) && m.Id != model.Id);
            if (existingWithCpf != null)
            {
                ModelState.AddModelError("CPF", "CPF já cadastrado no sistema.");
                return View(model);
            }

            var existing = _service.ListAll().FirstOrDefault(m => m.Id == model.Id);
            if (existing is null) return NotFound();
            model.IsActive = existing.IsActive;

            // Limpar senha do modelo - não será atualizada aqui
            model.Senha = string.Empty;

            _service.Update(model);
            TempData["SuccessMessage"] = "Motorista atualizado.";
            return RedirectToAction("Index");
        }
        catch (Exception ex)
        {
            ModelState.AddModelError(string.Empty, "Ocorreu um erro ao atualizar o motorista: " + ex.Message);
            return View(model);
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult ChangePassword(string id, ChangePasswordViewModel model)
    {
        if (!ModelState.IsValid)
        {
            TempData["ErrorMessage"] = "Dados inválidos para alteração de senha.";
            return RedirectToAction("Edit", new { id });
        }

        try
        {
            var success = _service.ChangePassword(id, model.SenhaAtual, model.NovaSenha);
            if (success)
            {
                TempData["SuccessMessage"] = "Senha alterada com sucesso.";
            }
            else
            {
                TempData["ErrorMessage"] = "Senha atual incorreta.";
            }
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = "Erro ao alterar senha: " + ex.Message;
        }

        return RedirectToAction("Edit", new { id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Deactivate(string id)
    {
        try
        {
            var motorista = _service.ListAll().FirstOrDefault(m => m.Id == id);
            if (motorista is null) return NotFound();

            motorista.IsActive = false;
            _service.Update(motorista);
            TempData["SuccessMessage"] = "Motorista desativado.";
            return RedirectToAction("Index");
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = "Erro ao desativar motorista: " + ex.Message;
            return RedirectToAction("Index");
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Activate(string id)
    {
        try
        {
            var motorista = _service.ListAll().FirstOrDefault(m => m.Id == id);
            if (motorista is null) return NotFound();

            motorista.IsActive = true;
            _service.Update(motorista);
            TempData["SuccessMessage"] = "Motorista ativado.";
            return RedirectToAction("Index");
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = "Erro ao ativar motorista: " + ex.Message;
            return RedirectToAction("Index");
        }
    }
}


