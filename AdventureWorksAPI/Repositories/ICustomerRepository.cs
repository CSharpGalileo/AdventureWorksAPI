using AdventureWorksNS.Data;

namespace AdventureWorksAPI.Repositories
{
    public interface ICustomerRepository
    {
        /*
         * CRUD
         * C = Create (Crear)
         * R = Read (Leer)
         * U = Update (Actualizar)
         * D = Delete (Borrar)
         */
        Task<Customer> CreateAsync(Customer c);
        Task<IEnumerable<Customer>> RetrieveAllAsync();
        Task<Customer?> RetrieveAsync(int id);
        Task<Customer?> UpdateAsync(int id, Customer c);
        Task<bool?> DeleteAsync(int id);
    }
}
