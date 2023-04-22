using XMLEdition.Data.Repositories.Interfaces;

namespace XMLEdition.Data.Repositories.Repositories
{
    public class Repository<TEntity> : IRepository<TEntity> where TEntity : class, new()
    {
        private readonly AppContext repositoryContext;

        // Конструктор
        public Repository(AppContext repositoryContext)
        {
            this.repositoryContext = repositoryContext;
        }

        public async Task<TEntity> AddAsync(TEntity entity)
        {
            if (entity == null)
            {
                throw new ArgumentNullException($"{nameof(AddAsync)} entity must not be null!!!");
            }

            try
            {
                await repositoryContext.AddAsync(entity);
                await repositoryContext.SaveChangesAsync();

                return entity;
            }
            catch (Exception ex)
            {
                throw new Exception($"{nameof(entity)} could not be saved: {ex.Message}");
            }
        }

        public IQueryable<TEntity> GetAll()
        {
            try
            {
                return repositoryContext.Set<TEntity>();
            }
            catch (Exception ex)
            {
                throw new Exception($" We can`t get entites!!!\n {ex.Message}");
            }
        }

        public async Task<TEntity> UpdateAsync(TEntity entity)
        {
            if (entity == null)
            {
                throw new ArgumentNullException($"{nameof(AddAsync)} entity must not be null!!!");
            }

            try
            {
                repositoryContext.Update(entity);
                await repositoryContext.SaveChangesAsync();

                return entity;
            }
            catch (Exception ex)
            {
                throw new Exception($"{nameof(entity)} could not be updated: {ex.Message}");
            }
        }
    }
}
