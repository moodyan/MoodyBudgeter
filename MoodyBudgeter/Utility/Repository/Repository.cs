using System.Linq;
using System.Threading.Tasks;

namespace MoodyBudgeter.Utility.Repository
{
    /// <summary>
    /// The Repository base class defines the ways in which we can retrieve data from the database
    /// It using a generic type so an implementation of this can use the business model that the data will return in.
    /// All methods are async. We want DB actions to be asynchronous so the thread can do other things while the execution is in the database.
    /// Repo users need to remember to use .ToListAsync when using a .GetAll() method. There is no way to enfoce this inside repo the method
    /// </summary>
    public abstract class Repository<T> where T : class
    {

        public Repository()
        {
        }

        /// <summary>
        /// This method retrieves a querable collection of data that will only execute after the caller has filtered.
        /// </summary>
        /// <returns>Queryable list of business models.</returns>
        public abstract IQueryable<T> GetAll();

        /// <summary>
        /// This finds a record using the unique identifier of the table.
        /// Always filter by PortalId.
        /// </summary>
        /// <param name="id">The unique identifier of the record to find.</param>
        /// <returns>A single business model.</returns>
        public abstract Task<T> Find(int id);

        /// <summary>
        /// This creates a record in the database.
        /// Make sure to always return the result db model translated back into a business model to include the Id.
        /// </summary>
        /// <param name="Entity">The record to create.</param>
        /// <returns>The result business model that represents the created record.</returns>
        public abstract Task<T> Create(T entity);

        /// <summary>
        /// Updates a record in the database
        /// Make sure to always return the result db model translated back into a business model.
        /// </summary>
        /// <param name="Entity">The record to update</param>
        /// <returns>The business model that represents the updated record.</returns>
        public abstract Task<T> Update(T entity);

        /// <summary>
        /// Deletes a record from the database.
        /// Be sure to always filter by the PortalId.
        /// </summary>
        /// <param name="id">The identifier of the record to delete.</param>
        public abstract Task Delete(int id);
    }
}
