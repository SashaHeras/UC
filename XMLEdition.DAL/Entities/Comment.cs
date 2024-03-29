﻿namespace XMLEdition.DAL.Entities
{
    public class Comment
    {
        public int Id { get; set; }

        public string Text { get; set; }

        public int Rating { get; set; }

        public DateTime DateCreation { get; set; }

        public Guid UserId { get; set; }

        public int CourseId { get; set; }
    }
}
