using StudentCurdApp.Models;
using StudentCurdApp.Repositoris;

namespace StudentCurdApp.Services
{
    public class StudentService:IStudentService
    {
        private readonly IStudentRepositories _studentrepository;

        public StudentService(IStudentRepositories studentrepository)
        {
            _studentrepository = studentrepository;
        }

        public List<Student> Get()
        {
            var response = _studentrepository.Get();
            return response;
        }
    }
}
