using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApi.Models;
using WebApi.Repository.Interface;

namespace WebApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductController : ControllerBase
    {
        private readonly IProductRepository _repo;
        public ProductController(IProductRepository repo) { _repo = repo; }

        [HttpGet]
        public async Task<IActionResult> GetProducts([FromQuery] ProductListRequest request)
            => Ok(await _repo.GetProductsAsync(request));

        [HttpGet("featured")]
        public async Task<IActionResult> GetFeatured()
            => Ok(await _repo.GetFeaturedProductsAsync());

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var product = await _repo.GetProductByIdAsync(id);
            return product == null ? NotFound() : Ok(product);
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Create([FromBody] ProductModel model)
        {
            var id = await _repo.CreateProductAsync(model);
            return CreatedAtAction(nameof(GetById), new { id }, new { ProductId = id });
        }

        [HttpPut("{id}")]
        [Authorize]
        public async Task<IActionResult> Update(int id, [FromBody] ProductModel model)
        {
            model.ProductId = id;
            return await _repo.UpdateProductAsync(model) ? Ok() : NotFound();
        }

        [HttpDelete("{id}")]
        [Authorize]
        public async Task<IActionResult> Delete(int id)
            => await _repo.DeleteProductAsync(id) ? Ok() : NotFound();
    }
}
