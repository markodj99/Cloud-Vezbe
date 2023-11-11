using Common.Interface;
using Common.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.ServiceFabric.Services.Remoting.Client;

namespace Client.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger) => _logger = logger;

        public IActionResult Index() => View();

        public IActionResult Privacy() => View();

        public IActionResult Book() => View();

        [HttpPost]
        public async Task<IActionResult> Book(ExampleModel model)
            => await ServiceProxy.Create<IValidator>(new Uri("fabric:/Cloud/Validator")).BuyBookAsync(model) switch
            {
                ReturnCode.Success => Ok("Sve proslo."),
                ReturnCode.ValidatorError => Ok("Greska pri validaciji."),
                ReturnCode.TransactionCoordinatorError => Ok("Greska kod koordinatora."),
                ReturnCode.BookStoreError => Ok("Korisnik porucio vise od 5 knjiga"),
                ReturnCode.BankError => Ok("Korisnik nema dovoljno novca."),
                _ => Ok("Nesto nisi dobro uradio Marko.")
            };
    }
}