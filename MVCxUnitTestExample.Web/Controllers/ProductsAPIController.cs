using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MVCxUnitTestExample.Web.Models;
using MVCxUnitTestExample.Web.Repository;

namespace MVCxUnitTestExample.Web.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductsAPIController : ControllerBase
    {
        private readonly IRepository<Product> _repository;

        public ProductsAPIController(IRepository<Product> repository)
        {
            this._repository = repository;
        }

        // GET: api/ProductsAPI
        [HttpGet]
        //Task<ActionResult<IEnumerable<Product>>>
        public async Task<IActionResult> GetProducts()
        {
            var products = await _repository.GetAll();
            return Ok(products);
        }

        // GET: api/ProductsAPI/5
        [HttpGet("{id}")]
        //Task<ActionResult<Product>>
        public async Task<IActionResult> GetProduct(int id)
        {
            var product = await _repository.GetById(id);

            if (product == null)
            {
                return NotFound();
            }

            return Ok(product);
        }

        // PUT: api/ProductsAPI/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for
        // more details see https://aka.ms/RazorPagesCRUD.
        [HttpPut("{id}")]
        public IActionResult PutProduct(int id, Product product)
        {
            if (id != product.Id)
            {
                return BadRequest();
            }

            //_repository.Entry(product).State = EntityState.Modified; it's done on Repository side

            _repository.Update(product);

            return NoContent();
        }

        // POST: api/ProductsAPI
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for
        // more details see https://aka.ms/RazorPagesCRUD.
        [HttpPost]
        //Task<ActionResult<Product>>
        public async Task<IActionResult> PostProduct(Product product)
        {
            //_repository.Products.Add(product);
            //await _repository.SaveChangesAsync();

            await _repository.Create(product);

            return CreatedAtAction("GetProduct", new { id = product.Id }, product);
        }

        // DELETE: api/ProductsAPI/5
        [HttpDelete("{id}")]
        //Task<ActionResult<Product>>
        public async Task<IActionResult> DeleteProduct(int id)
        {
            var product = await _repository.GetById(id);
            if (product == null)
            {
                return NotFound();
            }

            //_repository.Products.Remove(product);
            //await _repository.SaveChangesAsync();
            _repository.Delete(product);

            return NoContent();
        }

        public bool ProductExists(int id)
        {
            return _repository.GetById(id)?.Result != null;
        }
    }
}
