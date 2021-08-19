using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using DevStore.WebApp.MVC.Models;
using DevStore.WebApp.MVC.Services;

namespace DevStore.WebApp.MVC.Controllers
{
    [Authorize]
    public class ClienteController : MainController
    {
        private readonly IClientService _clientService;

        public ClienteController(IClientService clientService)
        {
            _clientService = clientService;
        }

        [HttpPost]
        public async Task<IActionResult> NovoEndereco(AddressViewModel address)
        {
            var Response = await _clientService.AddAddress(address);

            if (ResponsePossuiErros(Response)) TempData["Errors"] = 
                ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage)).ToList();

            return RedirectToAction("EnderecoEntrega", "Pedido");
        }
    }
}