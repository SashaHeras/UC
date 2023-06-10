using Azure.Core;
using Microsoft.AspNetCore.Http;
using Microsoft.WindowsAzure.Storage;
using System.Text.Json;
using XMLEdition.DAL.EF;
using XMLEdition.DAL.Entities;
using XMLEdition.DAL.Repositories;

namespace XMLEdition.Core.Services
{
    public class CourseService
    {
        public CourseRepository _courseRepository;
        public CourseItemRepository _courseItemRepository;
        public CourseTypeRepository _courseTypeRepository;
        public LessonRepository _lessonRepository;

        public CourseService(CourseRepository courseRepository, CourseItemRepository courseItemRepository, 
            CourseTypeRepository courseTypeRepository, LessonRepository lessonRepository)
        {
            _courseRepository = courseRepository;
            _courseItemRepository = courseItemRepository;
            _courseTypeRepository = courseTypeRepository;
            _lessonRepository = lessonRepository;
        }

        /// <summary>
        /// Execute stored procedure to generate list of course elements
        /// </summary>
        /// <param name="id"></param>
        /// <returns>
        /// Elements list in Json format
        /// </returns>
        public List<CourseElement>? GetCourseElements(int id)
        {
            var elementsJson = _courseRepository.GetCourseElementsList(id.ToString());
            var result = JsonSerializer.Deserialize<List<CourseElement>>(elementsJson);
            return result;
        }

        /// <summary>
        /// Create new empty course
        /// </summary>
        /// <returns>
        /// new Course()
        /// {
        ///    Id = 0,
        ///   Name = "",
        ///   Price = 0
        /// };
        /// </returns>
        public Course CreateNewCourse()
        {
            return new Course()
            {
                Id = 0,
                Name = "",
                Price = 0
            };
        }

        /// <summary>
        /// Return IQuerable of course items
        /// </summary>
        /// <param name="courseId"></param>
        /// <returns></returns>
        public IQueryable<CourseItem> GetElementsByCourseId(int courseId)
        {
            return _courseItemRepository.GetCourseItemsByCourseId(courseId);
        }

        /// <summary>
        /// Save picture in Azure blob 
        /// </summary>
        /// <param name="picture"></param>
        /// <param name="courseId"></param>
        /// <returns></returns>
        public string SavePicture(IFormFile picture, int courseId)
        {
            string resPath = String.Empty;
            if (picture != null)
            {                
                if (courseId != null)
                {
                    string oldPictureName = _courseRepository.GetCourse(courseId).PicturePath;
                    if (oldPictureName != null)
                    {
                        DeleteFromAzure(oldPictureName);
                    }
                }
                resPath = SaveInAsync(picture).Result;
            }
            else if (courseId != 0 && picture == null)
            {
                resPath = _courseRepository.GetCourse(courseId).PicturePath;
            }

            return resPath;
        }

        /// <summary>
        /// Save video in Azure blob 
        /// </summary>
        /// <param name="video"></param>
        /// <param name="courseId"></param>
        /// <returns></returns>
        public string SaveVideo(IFormFile video, int courseId)
        {
            string resPath = String.Empty;
            if (video != null)
            {                
                if (courseId != null)
                {
                    string oldVideoName = _courseRepository.GetCourse(courseId).PreviewVideoPath;
                    if (oldVideoName != null)
                    {
                        DeleteFromAzure(oldVideoName);
                    }
                }
                resPath = SaveInAsync(video).Result;
            }
            else if (courseId != 0 && video == null)
            {
                resPath = _courseRepository.GetCourse(courseId).PreviewVideoPath;
            }

            return resPath;
        }

        /// <summary>
        /// Save media file in Azure blob and return it`s name in Azure blob
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        public async Task<string> SaveInAsync(IFormFile file)
        {
            string uploads = "C:\\Users\\acsel\\source\\repos\\XMLEdition\\XMLEdition\\wwwroot\\Pictures\\";
            string newName = Guid.NewGuid().ToString().Replace("-", "");

            string filePath = Path.Combine(uploads, file.FileName);
            using (Stream fileStream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(fileStream);
            }

            string connectionString = "DefaultEndpointsProtocol=https;AccountName=mystudystorage;AccountKey=F2DhOdWx3qBaoImpVaDkLDVCyErlLVghvKL5kcxYLL9V7KsOQobaH8wWSh4m48ACDDK/lnsyzd3Q+AStciFc5Q==;EndpointSuffix=core.windows.net";

            try
            {
                var storageAccount = CloudStorageAccount.Parse(connectionString);
                var blobClient = storageAccount.CreateCloudBlobClient();
                var container = blobClient.GetContainerReference("test");

                await container.CreateIfNotExistsAsync();

                var blockBlob = container.GetBlockBlobReference(newName);

                await using (var stream = System.IO.File.OpenRead(uploads + file.FileName))
                {
                    await blockBlob.UploadFromStreamAsync(stream);
                }

                return newName;
            }
            catch (Exception ex)
            {
                // handle exceptions
                return "";
            }
        }

        public async Task<bool> DeleteFromAzure(string name)
        {
            string connectionString = "DefaultEndpointsProtocol=https;AccountName=mystudystorage;AccountKey=F2DhOdWx3qBaoImpVaDkLDVCyErlLVghvKL5kcxYLL9V7KsOQobaH8wWSh4m48ACDDK/lnsyzd3Q+AStciFc5Q==;EndpointSuffix=core.windows.net";

            try
            {
                var storageAccount = CloudStorageAccount.Parse(connectionString);
                var blobClient = storageAccount.CreateCloudBlobClient();
                var container = blobClient.GetContainerReference("test");
                var blockBlob = container.GetBlockBlobReference(name);

                if (await blockBlob.DeleteIfExistsAsync())
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception ex)
            {
                return false;
            }

            return true;
        }

        public static string GetTimeSinceDate(DateTime date)
        {
            TimeSpan timeSpan = DateTime.Now - date;

            int totalDays = (int)timeSpan.TotalDays;
            int totalWeeks = totalDays / 7;
            int totalMonths = totalDays / 30;
            int totalYears = totalDays / 365;

            if (totalWeeks < 4)
            {
                return $"{totalWeeks} week{(totalWeeks == 1 ? "" : "s")} ago";
            }
            else if (totalMonths < 12)
            {
                return $"{totalMonths} month{(totalMonths == 1 ? "" : "s")} ago";
            }
            else
            {
                return $"{totalYears} year{(totalYears == 1 ? "" : "s")} ago";
            }
        }
    }
}
