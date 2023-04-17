﻿namespace XMLEdition.Data
{
    public class Course
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public string PicturePath { get; set; }

        public Guid AuthorId { get; set; }

        public bool Checked { get; set; }

        public decimal Price { get; set; }

        public DateTime LastEdittingDate { get; set; }
    }
}