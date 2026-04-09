using Microsoft.AspNetCore.Mvc;
using Ube.Application.DTOs.Category;      // for CreateCategoryDto, UpdateCategoryDto, CategoryDto
using Ube.Application.Interfaces;         // for ICategoryService

namespace Ube.Api.Controllers
{

    [ApiController]
    [Route("api/[controller]")]
    public class CategoryController : ControllerBase
    {
        private readonly ICategoryService _service;

        public CategoryController(ICategoryService service)
        {
            _service = service;
        }

        // GET ALL (for cards UI)
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            return Ok(await _service.GetAllAsync());
        }

        // CREATE
        [HttpPost]
        public async Task<IActionResult> Create(CreateCategoryDto dto)
        {
            return Ok(await _service.CreateAsync(dto));
        }

        // UPDATE
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, UpdateCategoryDto dto)
        {
            var result = await _service.UpdateAsync(id, dto);
            if (!result) return NotFound();

            return Ok();
        }

        // DELETE
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _service.DeleteAsync(id);
            if (!result) return NotFound();

            return Ok();
        }

        // 🔥 TOGGLE (for switch button)
        [HttpPatch("{id}/toggle")]
        public async Task<IActionResult> Toggle(int id)
        {
            var result = await _service.ToggleStatusAsync(id);
            if (!result) return NotFound();

            return Ok();
        }
    }
}