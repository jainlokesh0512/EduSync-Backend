using Edu_Sync_Backend.Models;
using EduSyncWebAPI.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Edu_Sync_Backend.Data;
using Microsoft.AspNetCore.Authorization;

namespace EduSyncAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class AssessmentsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public AssessmentsController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/Assessments
        [HttpGet]
        //[Authorize(Policy = "RequireAdminOrInstructorRole")]
        public async Task<ActionResult<IEnumerable<AssessmentDto>>> GetAssessments()
        {
            return await _context.Assessments
                .Select(a => new AssessmentDto
                {
                    AssessmentId = a.AssessmentId,
                    Title = a.Title,
                    MaxScore = a.MaxScore,
                    CourseId = a.CourseId,
                    Question = a.Question
                }).ToListAsync();
        }

        // GET: api/Assessments/5
        [HttpGet("{id}")]
        //[Authorize(Policy = "RequireAdminOrInstructorRole")]
        public async Task<ActionResult<AssessmentDto>> GetAssessment(Guid id)
        {
            var assessment = await _context.Assessments.FindAsync(id);
            if (assessment == null) return NotFound();

            return new AssessmentDto
            {
                AssessmentId = assessment.AssessmentId,
                Title = assessment.Title,
                MaxScore = assessment.MaxScore,
                Question = assessment.Question,
                CourseId = assessment.CourseId
            };
        }

        // POST: api/Assessments
        [HttpPost]
        [Authorize(Policy = "RequireAdminOrInstructorRole")]
        public async Task<ActionResult<AssessmentDto>> PostAssessment(AssessmentDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            if (dto.CourseId != null &&
                !await _context.Courses.AnyAsync(c => c.CourseId == dto.CourseId))
                return Conflict("Course with given ID does not exist.");

            var assessment = new Assessment
            {
                AssessmentId = Guid.NewGuid(),
                Title = dto.Title,
                MaxScore = dto.MaxScore,
                CourseId = dto.CourseId,
                Question = dto.Question
            };

            _context.Assessments.Add(assessment);
            await _context.SaveChangesAsync();

            dto.AssessmentId = assessment.AssessmentId;
            return CreatedAtAction(nameof(GetAssessment), new { id = dto.AssessmentId }, dto);
        }

        // PUT: api/Assessments/5
        [HttpPut("{id}")]
        [Authorize(Policy = "RequireAdminOrInstructorRole")]
        public async Task<IActionResult> PutAssessment(Guid id, AssessmentDto dto)
        {
            if (id != dto.AssessmentId) return BadRequest("ID mismatch.");

            var assessment = await _context.Assessments.FindAsync(id);
            if (assessment == null) return NotFound();

            if (dto.CourseId != null &&
                !await _context.Courses.AnyAsync(c => c.CourseId == dto.CourseId))
                return Conflict("Course with given ID does not exist.");

            assessment.Title = dto.Title;
            assessment.MaxScore = dto.MaxScore;
            assessment.CourseId = dto.CourseId;
            assessment.Question = dto.Question;

            _context.Entry(assessment).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.Assessments.Any(a => a.AssessmentId == id))
                    return NotFound();
                throw;
            }

            return NoContent();
        }

        // DELETE: api/Assessments/5
        [HttpDelete("{id}")]
        [Authorize(Policy = "RequireAdminOrInstructorRole")]
        public async Task<IActionResult> DeleteAssessment(Guid id)
        {
            var assessment = await _context.Assessments.FindAsync(id);
            if (assessment == null) return NotFound();

            _context.Assessments.Remove(assessment);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
