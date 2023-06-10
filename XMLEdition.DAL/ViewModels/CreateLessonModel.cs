using Microsoft.AspNetCore.Http;

namespace XMLEdition.DAL.ViewModels
{
    public class CreateLessonModel
    {
        public string Theme { get; set; }

        public IFormFile VideoPath { get; set; }

        public string Description { get; set; }

        public string Body { get; set; }

        public string CourseId { get; set; }
    }
}
