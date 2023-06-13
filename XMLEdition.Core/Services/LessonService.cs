using XMLEdition.DAL.Entities;
using XMLEdition.DAL.Repositories;
using XMLEdition.DAL.ViewModels;

namespace XMLEdition.Core.Services
{
    public class LessonService
    {
        private LessonRepository _lessonRepository;

        public LessonService(LessonRepository lessonRepository)
        {
            _lessonRepository = lessonRepository;
        }     

        public void CreateLesson(CourseItem ci, CreateLessonModel clm, string name)
        {
            Lesson newLesson = new Lesson()
            {
                Id = Guid.NewGuid(),
                Theme = clm.Theme,
                Description = clm.Description,
                Body = clm.Body,
                VideoPath = name,
                CourseItemId = ci.Id,
                DateCreation = DateTime.Now.ToShortDateString()
            };

            _lessonRepository.AddAsync(newLesson);
        }

        public Lesson GetLesson(Guid id)
        {
            return _lessonRepository.GetLessonById(id);
        }

        public void UpdateLesson(Lesson newLesson)
        {
            Lesson currentLesson = _lessonRepository.GetLessonById(newLesson.Id);

            currentLesson.Theme = newLesson.Theme;
            currentLesson.Description = newLesson.Description;
            currentLesson.Body = newLesson.Body;
            currentLesson.CourseItemId = newLesson.CourseItemId;
            currentLesson.DateCreation = DateTime.Now.ToShortDateString();

            _lessonRepository.UpdateAsync(currentLesson);
        }       

        public Lesson GetLessonByCourseItem(int courseItemId)
        {
            return _lessonRepository.GetLessonByCourseItemId(courseItemId);
        }
    }
}
