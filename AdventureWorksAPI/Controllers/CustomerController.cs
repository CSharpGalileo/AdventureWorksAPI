using Microsoft.AspNetCore.Mvc;
using AdventureWorksNS.Data;
using AdventureWorksAPI.Repositories;

namespace AdventureWorksAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CustomerController : ControllerBase
    {
        private readonly ICustomerRepository repo;

        public CustomerController(ICustomerRepository repo)
        {
            this.repo = repo;
        }

        [HttpGet]
        [ProducesResponseType(200, Type = typeof(IEnumerable<Customer>))]
        public async Task<IEnumerable<Customer>> GetCustomers(string? companyName) 
        { 
            if (string.IsNullOrEmpty(companyName))
            {
                return await repo.RetrieveAllAsync();
            }
            else
            {
                return (await repo.RetrieveAllAsync())
                        .Where(customer => customer.CompanyName == companyName);
            }
        }

        [HttpGet("{id}", Name = nameof(GetCustomer))] //Ruta
        [ProducesResponseType(200, Type = typeof(Customer))]
        [ProducesResponseType(404)]
        public async Task<IActionResult> GetCustomer(int id)
        {
            Customer? c = await repo.RetrieveAsync(id);
            if (c == null)
            {
                return NotFound(); //Error 404
            }
            return Ok(c);
        }

        [HttpPost]
        [ProducesResponseType(201, Type = typeof(Customer))]
        [ProducesResponseType(400)]
        public async Task<IActionResult> Create([FromBody] Customer c)
        {
            if (c == null)
            {
                return BadRequest(); //400
            }
            Customer? addCustomer = await repo.CreateAsync(c);
            if (addCustomer == null)
            {
                return BadRequest("El repositorio fallo al crear el customer");
            }
            else
            {
                return CreatedAtRoute(
                        routeName: nameof(GetCustomer),
                        routeValues: new {id = addCustomer.CustomerId},
                        value: addCustomer);
            }
        }

        [HttpPut("{id}")]
        [ProducesResponseType(204)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> Update(int id, [FromBody] Customer c)
        {
            if (c == null || c.CustomerId != id)
            {
                return BadRequest(); //400
            }
            Customer? existe = await repo.RetrieveAsync(id);
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
            Customer? existe = await repo.RetrieveAsync(id);
            if (existe == null)
            {
                return NotFound(); //404
            }
            bool? deleted = await repo.DeleteAsync(id);
            if (deleted.HasValue && deleted.Value)
            {
                return new NoContentResult(); //201
            }
            return BadRequest($"Cliente con el id {id} no se pudo borrar");
        }

    }
}
