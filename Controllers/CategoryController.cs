using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TestToken.DTO;
using TestToken.DTO.ProductDto;
using TestToken.Repositories.Interfaces;
using TestToken.Repositories.Services;
using TestToken.UOW;

namespace TestToken.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CategoryController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICloudinaryService _cloudinaryService;
        public CategoryController(IUnitOfWork unitOfWork,ICloudinaryService cloudinaryService)
        {
            _unitOfWork = unitOfWork;
            _cloudinaryService = cloudinaryService;
        }

        [HttpGet("Categories")]
        public async Task<IActionResult> GetAllCategories()
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            var response = await _unitOfWork.Categories.GetAllCategories();
            if (response.IsSucceeded)
                return Ok(response);
            return StatusCode(response.StatusCode, new { response.Message });

        }

        [Authorize(Policy = "Admin")]
        [HttpGet("CategoryById/{id}")]
        public async Task<IActionResult> categoryById(int id)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            var response = await _unitOfWork.Categories.GetCategoryById(id);
            if (response.IsSucceeded)
                return Ok(response);
            return StatusCode(response.StatusCode, new { response.Message });
        }

        [HttpGet("CategoryName")]
        public async Task<IActionResult> CategoryName([FromQuery]string name )
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            var response = await _unitOfWork.Categories.GetCategoryByName(name);
            if (response.IsSucceeded)
                return Ok(response);
            return StatusCode(response.StatusCode, new { response.Message });
        }

        [Authorize(Policy = "Admin")]
        [HttpPost("AddCategory")]
        public async Task<IActionResult> AddCategory(CategoryDto categoryDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            var imageUrl = await _cloudinaryService.UploadImageAsync(categoryDto.ImageFile);
            categoryDto.Image = imageUrl;
            var response = await _unitOfWork.Categories.AddCategory(categoryDto);
            if (response.IsSucceeded)
                return Ok(response);
            return StatusCode(response.StatusCode, new { response.Message });
        }

        [Authorize(Policy = "Admin")]
        [HttpPut("UpdateCategory")]
        public async Task<IActionResult> UpdatedCategory(int id ,CategoryDto categoryDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            var response = await _unitOfWork.Categories.UpdateCategory(id, categoryDto);
            if (response.IsSucceeded)
                return Ok(response);
            return StatusCode(response.StatusCode, new { response.Message });
        }

        [Authorize(Policy = "Admin")]
        [HttpPut("DeleteCategory/{id}")]
        public async Task<IActionResult> DeleteCategory(int id)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            var response = await _unitOfWork.Categories.DeleteCategory(id);
            if (response.IsSucceeded)
                return Ok(response);
            return StatusCode(response.StatusCode, new { response.Message });
        }
    }
}
