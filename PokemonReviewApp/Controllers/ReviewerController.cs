using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PokemonReviewApp.Dto;
using PokemonReviewApp.Interfaces;
using PokemonReviewApp.Models;
using PokemonReviewApp.Services;

namespace PokemonReviewApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReviewerController : Controller
    {
        private readonly IReviewerService _reviewerService;
        private readonly IMapper _mapper;
        public ReviewerController(IReviewerService reviewerService, IMapper mapper)
        {
            _reviewerService = reviewerService;
            _mapper = mapper;
        }

        [AllowAnonymous]
        [HttpGet]
        [ProducesResponseType(200, Type = typeof(IEnumerable<Reviewer>))]
        public IActionResult GetReviewers()
        {
            var reviewers = _mapper.Map<List<ReviewerDto>>(_reviewerService.GetReviewers());

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            return Ok(reviewers);
        }

        [AllowAnonymous]
        [HttpGet("{reviewerId}")]
        [ProducesResponseType(200, Type = typeof(Reviewer))]
        [ProducesResponseType(400)]
        public IActionResult GetPokemon(int reviewerId)
        {
            if (!_reviewerService.ReviewerExists(reviewerId))
                return NotFound();

            var reviewer = _mapper.Map<ReviewDto>(
                _reviewerService.GetReviewer(reviewerId));

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            return Ok(reviewer);
        }

        [AllowAnonymous]
        [HttpGet("{reviewerId}/reviews")]
        [ProducesResponseType(200, Type = typeof(Reviewer))]
        [ProducesResponseType(400)]
        public IActionResult GetReviewsByReviewer(int reviewerId)
        {
            if (!_reviewerService.ReviewerExists(reviewerId))
                return NotFound();

            var reviews = _mapper.Map<List<ReviewerDto>>(
                _reviewerService.GetReviewsByReviewer(reviewerId));

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            return Ok(reviews);
        }

        [Authorize(Roles = "admin")]
        [HttpPost]
        [ProducesResponseType(204)]
        [ProducesResponseType(400)]
        public IActionResult CreateReviewer([FromBody] ReviewerDto reviewerCreate)
        {
            if (reviewerCreate == null)
                return BadRequest(ModelState);

            var reviewers = _reviewerService.GetReviewers()
                .Where(c => c.LastName.Trim().ToUpper() == reviewerCreate.LastName.TrimEnd().ToUpper())
                .FirstOrDefault();

            if (reviewers != null)
            {
                ModelState.AddModelError("", "Reviewer alredy exists");
                return StatusCode(422, ModelState);
            }

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var reviewerMap = _mapper.Map<Reviewer>(reviewerCreate);

            if (!_reviewerService.CreateReviewer(reviewerMap))
            {
                ModelState.AddModelError("", "Something went wrong while saving");
                return StatusCode(500, ModelState);
            }
            return Ok("Successfully created");
        }

        [Authorize(Roles = "admin")]
        [HttpPut("{reviewerId}")]
        [ProducesResponseType(400)]
        [ProducesResponseType(204)]
        [ProducesResponseType(404)]
        public IActionResult UpdateReviewer(int reviewerId, [FromBody] ReviewerDto updateReviewer)
        {
            if (updateReviewer == null)
                return BadRequest(ModelState);

            if (reviewerId != updateReviewer.Id)
                return BadRequest(ModelState);

            if (!_reviewerService.ReviewerExists(reviewerId))
                return NotFound();

            if (!ModelState.IsValid)
                return BadRequest();

            var reviewerMap = _mapper.Map<Reviewer>(updateReviewer);

            if (!_reviewerService.UpdateReviewer(reviewerMap))
            {
                ModelState.AddModelError("", "Something went wrong updating reviewer");
                return StatusCode(500, ModelState);
            }
            return NoContent();
        }

        [Authorize(Roles = "admin")]
        [HttpDelete("{reviewerId}")]
        [ProducesResponseType(400)]
        [ProducesResponseType(204)]
        [ProducesResponseType(404)]
        public IActionResult DeleteReviewer(int reviewerId)
        {
            if (!_reviewerService.ReviewerExists(reviewerId))
            {
                return NotFound();
            }

            var reviewerToDelete = _reviewerService.GetReviewer(reviewerId);

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (!_reviewerService.DeleteReviewer(reviewerToDelete))
            {
                ModelState.AddModelError("", "Something went wrong deleting reviewer");
            }

            return NoContent();
        }
    }
}
