using XMLEdition.DAL.Entities;
using XMLEdition.DAL.Infrastructure;

namespace XMLEdition.DAL.Interfaces
{
    public interface ICourseRepository : ISingletoneService, IRepository<Course>
    {
        public string GetCourseElementsList(string courseId);

        public Course GetCourse(int courseId);

        public List<Course> GetAllAuthorsCourses(Guid uid);
    }
}
