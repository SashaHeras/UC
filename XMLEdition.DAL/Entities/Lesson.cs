﻿namespace XMLEdition.DAL.Entities
{
    public class Lesson
    {
        public Guid Id { get; set; }

        public string Theme { get; set; }

        public string Description { get; set; }

        public string VideoPath { get; set; }

        public string Body { get; set; }

        public string DateCreation { get; set; }

        public int CourseItemId { get; set; }
    }
}
