namespace XMLEdition.Models
{
    public class CommentModel
    {
        public int Id { get; set; }

        public string Text { get; set; }

        public int Rating { get; set; }

        public string DateAgo { get; set; }

        public string UserName { get; set; }

        public string ProfileImagePath { get; set; }
    }
}
