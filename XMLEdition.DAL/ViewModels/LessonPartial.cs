namespace XMLEdition.DAL.ViewModels
{
    public class LessonPartial
    {
        public Guid Id { get; set; }

        public string Theme { get; set; }

        public string Description { get; set; }

        public string VideoPath { get; set; }

        public string Body { get; set; }

        public string DateCreation { get; set; }

        public int? CourseItemId { get; set; }

        public int CourseId { get; set; }

        public double Rating { get; set; }
    }
}
