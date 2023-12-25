using StudentCurdApp.Models;

namespace StudentCurdApp.Repositoris
{
    public class StudentRepositoris:IStudentRepositories
    {
        private readonly StudentContext _studentContext;

        public StudentRepositoris(StudentContext studentContext)
        {
            _studentContext = studentContext;
        }

        public List<Student> Get()
        {
            var response = _studentContext.Students.ToList();
            return response;
        }
    }


}
