using Microsoft.EntityFrameworkCore;
using XMLEdition.Data.Repositories.Interfaces;

namespace XMLEdition.Data.Repositories.Repositories
{
    public class CourseRepository : Repository<Course>, ICourseRepository
    {
        private AppContext _context;

        public CourseRepository(AppContext repositoryContext) : base(repositoryContext)
        {
            _context = repositoryContext;
        }

        public Course GetCourse(int courseId)
        {
            return GetAll().Where(course => course.Id == courseId).FirstOrDefault();
        }

        public string GetCourseElementsList(string courseId)
        {
            string query = "EXEC GetCourseElementsList @courseId = " + courseId;
            var result = _context.Database.SqlQueryRaw<string>(query).AsEnumerable().FirstOrDefault();
            return result;
        }
    }
}
