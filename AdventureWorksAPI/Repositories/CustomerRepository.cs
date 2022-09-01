using Microsoft.EntityFrameworkCore.ChangeTracking;
using AdventureWorksNS.Data;
using System.Collections.Concurrent;

namespace AdventureWorksAPI.Repositories
{
    public class CustomerRepository : ICustomerRepository
    {
        private static ConcurrentDictionary<int, Customer>? customerCache;
        //Comentario, puede usar Redis para un cache mas eficiente ==> Open Source
        private AdventureWorksDB db;

        public CustomerRepository(AdventureWorksDB injectedDB)
        {
            db = injectedDB;
            if (customerCache is null)
            {
                customerCache = new ConcurrentDictionary<int,Customer>(
                    db.Customers.ToDictionary(c=> c.CustomerId));
            }
        }

        public async Task<Customer> CreateAsync(Customer c)
        {
            EntityEntry<Customer> agregado = await db.Customers.AddAsync(c);
            int afectados = await db.SaveChangesAsync();
            if (afectados == 1)
            {
                if (customerCache is null) return c;
                return customerCache.AddOrUpdate(c.CustomerId, c, UpdateCache);
            }
            return null!;
        }

        private Customer UpdateCache(int id, Customer c)
        {
            Customer? viejo;
            if (customerCache is not null)
            {
                if (customerCache.TryGetValue(id, out viejo))
                {
                    if (customerCache.TryUpdate(id,c, viejo))
                    {
                        return c;
                    }
                }
            }
            return null!;
        }

        public Task<IEnumerable<Customer>> RetrieveAllAsync()
        {
            return Task.FromResult(customerCache is null ?
                Enumerable.Empty<Customer>() : customerCache.Values);
        }

        public Task<Customer?> RetrieveAsync(int id)
        {
            if (customerCache is null) return null!;
            customerCache.TryGetValue(id, out Customer? c);
            return Task.FromResult(c);
        }

        public async Task<Customer?> UpdateAsync(int id, Customer c)
        {
            db.Customers.Update(c);
            int afectados = await db.SaveChangesAsync();
            if (afectados == 1)
            {
                return UpdateCache(id, c);
            }
            return null;
        }

        public async Task<bool?> DeleteAsync(int id)
        {
            Customer? c = db.Customers.Find(id);
            if (c is null) return false;
            db.Customers.Remove(c);
            int afectados = await db.SaveChangesAsync();
            if (afectados == 1)
            {
                if (customerCache is null) return null;
                return customerCache.TryRemove(id, out c);
            }
            else
            {
                return null;
            }
        }

    }
}
