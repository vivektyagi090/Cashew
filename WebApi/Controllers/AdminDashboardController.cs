using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApi.Models;
using WebApi.Repository.Interface;

namespace WebApi.Controllers
{
    [Authorize(Roles = "Admin")]
    [ApiController]
    [Route("api/[controller]")]
    public class AdminDashboardController : ControllerBase
    {
        private readonly IAdminRepository _adminRepo;
        private readonly IInventoryRepository _inventoryRepo;
        private readonly ILogisticsRepository _logisticsRepo;
        private readonly IProductRepository _productRepo;
        private readonly ICategoryRepository _categoryRepo;

        public AdminDashboardController(
            IAdminRepository adminRepo, 
            IInventoryRepository inventoryRepo, 
            ILogisticsRepository logisticsRepo,
            IProductRepository productRepo,
            ICategoryRepository categoryRepo)
        {
            _adminRepo = adminRepo;
            _inventoryRepo = inventoryRepo;
            _logisticsRepo = logisticsRepo;
            _productRepo = productRepo;
            _categoryRepo = categoryRepo;
        }

        [HttpGet("stats")]
        public async Task<IActionResult> GetStats()
        {
            var stats = await _adminRepo.GetDashboardStatsAsync();
            return Ok(stats);
        }

        [HttpGet("inventory-master")]
        public async Task<IActionResult> GetInventoryMaster()
        {
            var master = await _adminRepo.GetInventoryMasterAsync();
            return Ok(master);
        }

        [HttpGet("movements")]
        public async Task<IActionResult> GetMovements()
        {
            var movements = await _inventoryRepo.GetMovementsAsync();
            return Ok(movements);
        }

        [HttpPost("movement")]
        public async Task<IActionResult> LogMovement([FromBody] InventoryModel movement)
        {
            var result = await _inventoryRepo.AddMovementAsync(movement);
            return result ? Ok(new { message = "Stock movement logged successfully" }) : BadRequest();
        }

        [HttpPost("damage")]
        public async Task<IActionResult> LogDamage([FromBody] DamageModel damage)
        {
            // Set current admin ID from claims if needed
            var result = await _inventoryRepo.LogDamageAsync(damage);
            return result ? Ok(new { message = "Damage logged successfully" }) : BadRequest();
        }

        [HttpGet("returns")]
        public async Task<IActionResult> GetReturns()
        {
            var returns = await _inventoryRepo.GetReturnsAsync();
            return Ok(returns);
        }

        [HttpPost("return")]
        public async Task<IActionResult> LogReturn([FromBody] ReturnModel returnItem)
        {
            // Manual return logging from master
            var result = await _inventoryRepo.ProcessReturnAsync(returnItem);
            return result ? Ok(new { message = "Manual return logged successfully" }) : BadRequest();
        }

        [HttpPut("returns/process")]
        public async Task<IActionResult> ProcessReturn([FromBody] ReturnModel returnItem)
        {
            var result = await _inventoryRepo.ProcessReturnAsync(returnItem);
            return result ? Ok(new { message = "Return processed successfully" }) : BadRequest();
        }

        [HttpGet("logistics")]
        public async Task<IActionResult> GetLogistics()
        {
            var logistics = await _logisticsRepo.GetAllTrackingAsync();
            return Ok(logistics);
        }

        [HttpPut("logistics/update")]
        public async Task<IActionResult> UpdateLogistics([FromBody] LogisticsModel logistics)
        {
            var result = await _logisticsRepo.UpdateLogisticsAsync(logistics);
            return result ? Ok(new { message = "Logistics updated successfully" }) : BadRequest();
        }

        // --- Master CRUD Operations ---

        [HttpGet("categories")]
        public async Task<IActionResult> GetCategories()
        {
            var categories = await _categoryRepo.GetAllCategoriesAsync();
            return Ok(categories);
        }

        [HttpPost("category")]
        public async Task<IActionResult> CreateCategory([FromBody] CategoryModel category)
        {
            var id = await _categoryRepo.CreateCategoryAsync(category);
            return id > 0 ? Ok(new { id, message = "Category created" }) : BadRequest();
        }

        [HttpPut("category")]
        public async Task<IActionResult> UpdateCategory([FromBody] CategoryModel category)
        {
            var result = await _categoryRepo.UpdateCategoryAsync(category);
            return result ? Ok(new { message = "Category updated" }) : BadRequest();
        }

        [HttpDelete("category/{id}")]
        public async Task<IActionResult> DeleteCategory(int id)
        {
            var result = await _categoryRepo.DeleteCategoryAsync(id);
            return result ? Ok(new { message = "Category deleted" }) : BadRequest();
        }

        [HttpPost("product")]
        public async Task<IActionResult> CreateProduct([FromBody] ProductModel product)
        {
            var id = await _productRepo.CreateProductAsync(product);
            return id > 0 ? Ok(new { id, message = "Product created" }) : BadRequest();
        }

        [HttpPut("product")]
        public async Task<IActionResult> UpdateProduct([FromBody] ProductModel product)
        {
            var result = await _productRepo.UpdateProductAsync(product);
            return result ? Ok(new { message = "Product updated" }) : BadRequest();
        }

        [HttpDelete("product/{id}")]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            var result = await _productRepo.DeleteProductAsync(id);
            return result ? Ok(new { message = "Product deleted" }) : BadRequest();
        }
    }
}
