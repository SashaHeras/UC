using XMLEdition.Data.Infrastructure;

namespace XMLEdition.Data.Repositories.Interfaces
{
    public interface ICourseRepository : ISingletoneService, IRepository<Course>
    {
        public string GetCourseElementsList(string courseId);

        public Course GetCourse(int courseId);

        public List<Course> GetAllAuthorsCourses(Guid uid);
    }
}
