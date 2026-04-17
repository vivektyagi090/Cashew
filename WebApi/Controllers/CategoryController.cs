using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApi.Models;
using WebApi.Repository.Interface;

namespace WebApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CategoryController : ControllerBase
    {
        private readonly ICategoryRepository _repo;
        public CategoryController(ICategoryRepository repo) { _repo = repo; }

        [HttpGet]
        public async Task<IActionResult> GetAll() => Ok(await _repo.GetAllCategoriesAsync());

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var cat = await _repo.GetCategoryByIdAsync(id);
            return cat == null ? NotFound() : Ok(cat);
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Create([FromBody] CategoryModel model)
        {
            var id = await _repo.CreateCategoryAsync(model);
            return CreatedAtAction(nameof(GetById), new { id }, new { CategoryId = id });
        }

        [HttpPut("{id}")]
        [Authorize]
        public async Task<IActionResult> Update(int id, [FromBody] CategoryModel model)
        {
            model.CategoryId = id;
            return await _repo.UpdateCategoryAsync(model) ? Ok() : NotFound();
        }

        [HttpDelete("{id}")]
        [Authorize]
        public async Task<IActionResult> Delete(int id)
            => await _repo.DeleteCategoryAsync(id) ? Ok() : NotFound();
    }
}
