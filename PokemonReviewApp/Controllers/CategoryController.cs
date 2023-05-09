using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ActionConstraints;
using PokemonReviewApp.Cache;
using PokemonReviewApp.Dto;
using PokemonReviewApp.Interfaces;
using PokemonReviewApp.Models;
using PokemonReviewApp.Services;

namespace PokemonReviewApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CategoryController : ControllerBase
    {
        public readonly ICategoryService _categoryService;
        public readonly IMapper _mapper;
        private readonly ICacheService _cacheService;
        public CategoryController(ICategoryService categoryService, IMapper mapper, ICacheService cacheService)
        {
            _categoryService = categoryService;
            _mapper = mapper;
            _cacheService = cacheService;
        }

        [AllowAnonymous]
        [HttpGet]
        [ProducesResponseType(200, Type = typeof(IEnumerable<Category>))]
        public IActionResult GetCategories()
        {
            const string key = "pokemons-all-category";
            var cached = _cacheService.GetValue<IEnumerable<Category>>(key);

            if (cached == null)
            {
                var categories = _mapper.Map<List<CategoryDto>>(_categoryService.GetCategories());

                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                return Ok(categories);
            }
            return Ok(cached);
        }

        [AllowAnonymous]
        [HttpGet("{categoryId}")]
        [ProducesResponseType(200, Type = typeof(Category))]
        [ProducesResponseType(400)]
        public IActionResult GetCategory(int categoryId)
        {
            if (!_categoryService.CategoryExists(categoryId))
                return NotFound();

            var category = _mapper.Map<CategoryDto>(_categoryService.CategoryExists(categoryId));

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            return Ok(category);
        }

        [AllowAnonymous]
        [HttpGet("pokemon/{categoryId}")]
        [ProducesResponseType(200, Type = typeof(IEnumerable<Pokemon>))]
        [ProducesResponseType(400)]
        public IActionResult GetPokemonByCategoryId(int categoryId)
        {
            var pokemons = _mapper.Map<List<PokemonDto>>(
                _categoryService.GetPokemonByCategory(categoryId));

            if (!ModelState.IsValid)
                return BadRequest();

            return Ok(pokemons);
        }

        [Authorize(Roles = "admin")]
        [HttpPost]
        [ProducesResponseType(204)]
        [ProducesResponseType(400)]
        public IActionResult  CreateCategory([FromBody] CategoryDto categoryCreate)
        {
            if (categoryCreate == null)
                return BadRequest(ModelState);

            var category = _categoryService.GetCategories()
                .Where(c => c.Name.Trim().ToUpper() == categoryCreate.Name.TrimEnd().ToUpper())
                .FirstOrDefault();

            if (category != null)
            {
                ModelState.AddModelError("", "Category alredy exists");
                return StatusCode(422, ModelState);
            }

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var categoryMap = _mapper.Map<Category>(categoryCreate);

            if (!_categoryService.CreateCategory(categoryMap))
            {
                ModelState.AddModelError("", "Something went wrong while saving");
                return StatusCode(500, ModelState);
            }

            // drop/update cache for this entity

            return Ok("Successfully created");
        }

        [Authorize(Roles = "admin")]
        [HttpPut("{categoryId}")]
        [ProducesResponseType(400)]
        [ProducesResponseType(204)]
        [ProducesResponseType(404)]
        public IActionResult UpdateCategory(int categoryId, [FromBody]CategoryDto updateCategory)
        {
            if(updateCategory == null) 
                return BadRequest(ModelState);

            if (categoryId != updateCategory.Id)
                return BadRequest(ModelState);

            if (!_categoryService.CategoryExists(categoryId))
                return NotFound();

            if(!ModelState.IsValid)
                return BadRequest();

            var categoryMap = _mapper.Map<Category>(updateCategory);

            if (!_categoryService.UpdateCategory(categoryMap))
            {
                ModelState.AddModelError("", "Something went wrong updating category");
                return StatusCode(500, ModelState);
            }
            return NoContent();
        }

        [Authorize(Roles = "admin")]
        [HttpDelete("{categoryId}")]
        [ProducesResponseType(400)]
        [ProducesResponseType(204)]
        [ProducesResponseType(404)]
        public IActionResult DeleteCategory(int categoryId)
        {
            if(!_categoryService.CategoryExists(categoryId))
            {
                return NotFound();
            }

            var categoryToDelete = _categoryService.GetCategory(categoryId);

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (!_categoryService.DeleteCategory(categoryToDelete))
            {
                ModelState.AddModelError("", "Something went wrong deleting category");
            }

            return NoContent();
        }
    }
}
