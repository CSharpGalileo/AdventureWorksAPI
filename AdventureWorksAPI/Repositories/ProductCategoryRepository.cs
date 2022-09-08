using Microsoft.EntityFrameworkCore.ChangeTracking;
using AdventureWorksNS.Data;
using System.Collections.Concurrent;

namespace AdventureWorksAPI.Repositories
{
    public class ProductCategoryRepository : IProductCategoryRepository
    {
        private static ConcurrentDictionary<int, ProductCategory>? productCategoryCache;
        //Comentario, puede usar Redis para un cache mas eficiente ==> Open Source
        private AdventureWorksDB db;

        public ProductCategoryRepository(AdventureWorksDB injectedDB)
        {
            db = injectedDB;
            if (productCategoryCache is null)
            {
                productCategoryCache = new ConcurrentDictionary<int,ProductCategory>(
                    db.ProductCategories.ToDictionary(c=> c.ProductCategoryId));
            }
        }

        public async Task<ProductCategory> CreateAsync(ProductCategory c)
        {
            EntityEntry<ProductCategory> agregado = await db.ProductCategories.AddAsync(c);
            int afectados = await db.SaveChangesAsync();
            if (afectados == 1)
            {
                if (productCategoryCache is null) return c;
                return productCategoryCache.AddOrUpdate(c.ProductCategoryId, c, UpdateCache);
            }
            return null!;
        }

        private ProductCategory UpdateCache(int id, ProductCategory c)
        {
            ProductCategory? viejo;
            if (productCategoryCache is not null)
            {
                if (productCategoryCache.TryGetValue(id, out viejo))
                {
                    if (productCategoryCache.TryUpdate(id,c, viejo))
                    {
                        return c;
                    }
                }
            }
            return null!;
        }

        public Task<IEnumerable<ProductCategory>> RetrieveAllAsync()
        {
            return Task.FromResult(productCategoryCache is null ?
                Enumerable.Empty<ProductCategory>() : productCategoryCache.Values);
        }

        public Task<ProductCategory?> RetrieveAsync(int id)
        {
            if (productCategoryCache is null) return null!;
            productCategoryCache.TryGetValue(id, out ProductCategory? c);
            return Task.FromResult(c);
        }

        public async Task<ProductCategory?> UpdateAsync(int id, ProductCategory c)
        {
            db.ProductCategories.Update(c);
            int afectados = await db.SaveChangesAsync();
            if (afectados == 1)
            {
                return UpdateCache(id, c);
            }
            return null;
        }

        public async Task<bool?> DeleteAsync(int id)
        {
            ProductCategory? c = db.ProductCategories.Find(id);
            if (c is null) return false;
            db.ProductCategories.Remove(c);
            int afectados = await db.SaveChangesAsync();
            if (afectados == 1)
            {
                if (productCategoryCache is null) return null;
                return productCategoryCache.TryRemove(id, out c);
            }
            else
            {
                return null;
            }
        }

    }
}
