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
    public IActionResult Edit(string id)
    {
        var user = _service.ListAll().FirstOrDefault(u => u.Id == id);
        if (user is null) return NotFound();

    // Exibir senha vazia na edição
        user.Senha = string.Empty;
        return View(user);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Edit(UsuarioViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        try
        {
            // Verificar CPF duplicado excluindo o usuário atual
            var existingWithCpf = _service.ListAll().FirstOrDefault(u => u.CPF != null && new string(u.CPF.Where(char.IsDigit).ToArray()) == new string(model.CPF.Where(char.IsDigit).ToArray()) && u.Id != model.Id);
            if (existingWithCpf != null)
            {
                ModelState.AddModelError("CPF", "CPF já cadastrado no sistema.");
                return View(model);
            }

            // Se o login estiver vazio, tentar definir para o primeiro nome
            if (string.IsNullOrWhiteSpace(model.Login) && !string.IsNullOrWhiteSpace(model.Nome))
            {
                var first = model.Nome.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries).FirstOrDefault();
                if (!string.IsNullOrWhiteSpace(first)) model.Login = first;
            }

            // Preservar o valor atual de IsActive já que o formulário de edição não o expõe
            var existing = _service.ListAll().FirstOrDefault(u => u.Id == model.Id);
            if (existing is null) return NotFound();
            model.IsActive = existing.IsActive;

            // Limpar senha do modelo - não será atualizada aqui
            model.Senha = string.Empty;

            _service.Update(model);
            TempData["SuccessMessage"] = "Usuário atualizado.";
            return RedirectToAction("Index");
        }
        catch (Exception ex)
        {
            ModelState.AddModelError(string.Empty, "Ocorreu um erro ao atualizar o usuário: " + ex.Message);
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
            var user = _service.ListAll().FirstOrDefault(u => u.Id == id);
            if (user is null) return NotFound();

            user.IsActive = false;
            _service.Update(user);
            TempData["SuccessMessage"] = "Usuário desativado.";
            return RedirectToAction("Index");
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = "Erro ao desativar usuário: " + ex.Message;
            return RedirectToAction("Index");
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Activate(string id)
    {
        try
        {
            var user = _service.ListAll().FirstOrDefault(u => u.Id == id);
            if (user is null) return NotFound();

            user.IsActive = true;
            _service.Update(user);
            TempData["SuccessMessage"] = "Usuário ativado.";
            return RedirectToAction("Index");
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = "Erro ao ativar usuário: " + ex.Message;
            return RedirectToAction("Index");
        }
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

    // Verificação duplicada de CPF (normaliza apenas os dígitos)
        try
        {
            if (_service.ExistsByCpf(model.CPF))
            {
                ModelState.AddModelError("CPF", "CPF já cadastrado no sistema.");
                return View(model);
            }

            // Exigir senha ao criar
            if (string.IsNullOrWhiteSpace(model.Senha))
            {
                ModelState.AddModelError("Senha", "Senha é obrigatória.");
                return View(model);
            }

            // Se o Login não foi informado, definir como o primeiro nome do usuário
            if (string.IsNullOrWhiteSpace(model.Login) && !string.IsNullOrWhiteSpace(model.Nome))
            {
                var first = model.Nome.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries).FirstOrDefault();
                if (!string.IsNullOrWhiteSpace(first)) model.Login = first;
            }

            _service.Add(model);
            TempData["SuccessMessage"] = "Usuário cadastrado.";
            return RedirectToAction("Index");
        }
        catch (Exception ex)
        {
            // Registrar e exibir mensagem de erro amigável
            ModelState.AddModelError(string.Empty, "Ocorreu um erro ao salvar o usuário: " + ex.Message);
            return View(model);
        }
    }
}


