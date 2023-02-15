namespace XMLEdition.Models
{
    public class CreateLessonModel
    {
        public string Theme { get; set; }

        public IFormFile VideoPath { get; set; }

        public string Description { get; set; }

        public string Body { get; set; }
    }
}
