using Microsoft.AspNetCore.Mvc;
using AdventureWorksNS.Data;
using AdventureWorksAPI.Repositories;

namespace AdventureWorksAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductCategoryController : ControllerBase
    {
        private readonly IProductCategoryRepository repo;

        public ProductCategoryController(IProductCategoryRepository repo)
        {
            this.repo = repo;
        }

        [HttpGet]
        [ProducesResponseType(200, Type = typeof(IEnumerable<ProductCategory>))]
        public async Task<IEnumerable<ProductCategory>> GetProductCategories(string? productCategoryName) 
        { 
            if (string.IsNullOrEmpty(productCategoryName))
            {
                return await repo.RetrieveAllAsync();
            }
            else
            {
                return (await repo.RetrieveAllAsync())
                        .Where(productCategory => productCategory.Name == productCategoryName);
            }
        }

        [HttpGet("{id}", Name = nameof(GetProductCategories))] //Ruta
        [ProducesResponseType(200, Type = typeof(ProductCategory))]
        [ProducesResponseType(404)]
        public async Task<IActionResult> GetProductCategories(int id)
        {
            ProductCategory? c = await repo.RetrieveAsync(id);
            if (c == null)
            {
                return NotFound(); //Error 404
            }
            return Ok(c);
        }

        [HttpPost]
        [ProducesResponseType(201, Type = typeof(ProductCategory))]
        [ProducesResponseType(400)]
        public async Task<IActionResult> Create([FromBody] ProductCategory p)
        {
            if (p == null)
            {
                return BadRequest(); //400
            }
            ProductCategory? addProductCategory = await repo.CreateAsync(p);
            if (addProductCategory == null)
            {
                return BadRequest("El repositorio fallo al crear el ProductCategory");
            }
            else
            {
                return CreatedAtRoute(
                        routeName: nameof(GetProductCategories),
                        routeValues: new {id = addProductCategory.ProductCategoryId},
                        value: addProductCategory);
            }
        }

        [HttpPut("{id}")]
        [ProducesResponseType(204)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> Update(int id, [FromBody] ProductCategory c)
        {
            if (c == null || c.ProductCategoryId != id)
            {
                return BadRequest(); //400
            }
            ProductCategory? existe = await repo.RetrieveAsync(id);
            if (existe == null)
            {
                return NotFound(); //404
            }
            await repo.UpdateAsync(id, c);
            return new NoContentResult(); //204
        }

        [HttpDelete("{id}")]
        [ProducesResponseType(204)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> Delete(int id)
        {
            ProductCategory? existe = await repo.RetrieveAsync(id);
            if (existe == null)
            {
                return NotFound(); //404
            }
            bool? deleted = await repo.DeleteAsync(id);
            if (deleted.HasValue && deleted.Value)
            {
                return new NoContentResult(); //201
            }
            return BadRequest($"Product Category con el id {id} no se pudo borrar");
        }

    }
}
