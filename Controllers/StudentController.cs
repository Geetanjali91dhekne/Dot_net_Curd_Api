using Azure;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using StudentCurdApp.Models;
using StudentCurdApp.Services;

namespace StudentCurdApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class StudentController : ControllerBase
    {
        private readonly IStudentService _contextService;
        public StudentController(IStudentService contextService)
        {
            _contextService = contextService;
        }
        [HttpGet]
        public List<Student> Get()
        {
            var response = _contextService.Get();
            return response;
        }
        
     }
}
