﻿using XMLEdition.DAL.EF;
using XMLEdition.DAL.Entities;
using XMLEdition.DAL.Interfaces;

namespace XMLEdition.DAL.Repositories
{
    public class CourseTypeRepository : Repository<CourseItemType>, ICourseTypeRepository
    {
        private ProjectContext _context;
        
        public CourseTypeRepository(ProjectContext repositoryContext) : base(repositoryContext)
        {
            _context = repositoryContext;
        }

        /// <summary>
        /// Method return CourseItemType by id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public CourseItemType GetTypeById(int id)
        {
            return GetAll().Where(ct => ct.Id == id).FirstOrDefault();
        }

        public CourseItemType GetItemTypeByName(string name)
        {
            return GetAll().Where(cit => cit.Name == name).FirstOrDefault();
        }
    }
}
