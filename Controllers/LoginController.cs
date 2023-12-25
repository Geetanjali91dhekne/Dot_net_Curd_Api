using AuthenticationService.Models;
using AuthenticationService.Repositories;
using AuthenticationService.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace AuthenticationService.Controller
{

    [Route("api/[controller]")]
    [ApiController]
    public class LoginController : ControllerBase
    {
        private readonly ILoginService _service;
        private readonly ILogger<LoginController> _logger;


        public LoginController(ILoginService service, ILogger<LoginController> logger)
        {
            _service = service;
            _logger = logger;
        }


        [AllowAnonymous]
        [HttpPost]
        public IActionResult Index(Login login)
        {
            _logger.LogInformation("Get User Details");
            try
            {
                var response = _service.GetLoginUser(login.domainId,login.password);
                if (response.data == null)
                {
                    _logger.LogError("Details not present");
                }
                return Ok(response);
            }
            catch (Exception)
            {
                _logger.LogError("An error occurred while retrieving details of logbook WtgBreakdown.");
                return StatusCode(500, "An error occurred while retrieving details of logbook WtgBreakdown.");
            }
        }


    }

}
