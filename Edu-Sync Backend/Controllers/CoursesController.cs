using Edu_Sync_Backend.Models;
using Edu_Sync_Backend.Data;
using EduSyncWebAPI.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;

namespace EduSyncWebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class CourseController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly ILogger<CourseController> _logger;

        public CourseController(AppDbContext context, ILogger<CourseController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: api/Course
        [HttpGet]
        //[Authorize(Policy = "RequireAdminOrInstructorRole")]
        public async Task<ActionResult<IEnumerable<CourseReadDTO>>> GetCourses()
        {
            var courses = await _context.Courses
                .Select(c => new CourseReadDTO
                {
                    CourseId = c.CourseId,
                    Title = c.Title,
                    Description = c.Description,
                    InstructorId = c.InstructorId,
                    MediaUrl = c.MediaUrl
                }).ToListAsync();

            return Ok(courses);
        }

        // GET: api/Course/{id}
        [HttpGet("{id}")]
        //[Authorize(Policy = "RequireAdminOrInstructorRole")]
        public async Task<ActionResult<CourseDetailDTO>> GetCourse(Guid id)
        {
            var course = await _context.Courses
                .Include(c => c.Assessments)
                .Include(c => c.Instructor)
                .FirstOrDefaultAsync(c => c.CourseId == id);

            if (course == null)
            {
                return NotFound();
            }

            var result = new CourseDetailDTO
            {
                CourseId = course.CourseId,
                Title = course.Title,
                Description = course.Description,
                InstructorId = course.InstructorId,
                MediaUrl = course.MediaUrl,
                Assessments = course.Assessments?
                    .Select(a => new AssessmentSummaryDTO
                    {
                        AssessmentId = a.AssessmentId,
                        Title = a.Title,
                        MaxScore = a.MaxScore
                    }).ToList(),
                Instructor = course.Instructor == null ? null : new UserDto
                {
                    UserId = course.Instructor.UserId,
                    FullName = course.Instructor.Name,
                    Email = course.Instructor.Email
                }
            };

            return Ok(result);
        }

        // POST: api/Course
        [HttpPost]
        [Authorize(Policy = "RequireAdminOrInstructorRole")]
        public async Task<ActionResult<CourseReadDTO>> CreateCourse([FromBody] CourseCreateDTO courseDto)
        {
            var logger = _logger ?? throw new ArgumentNullException(nameof(_logger));
            
            try
            {
                logger.LogInformation("Starting course creation with data: {CourseData}", 
                    new { courseDto.Title, courseDto.InstructorId });

                // Validate model state
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage)
                        .ToList();
                    
                    logger.LogWarning("Validation failed: {Errors}", string.Join(", ", errors));
                    return BadRequest(new { 
                        message = "Validation failed", 
                        errors = errors 
                    });
                }


                // Validate instructor exists if provided
                if (courseDto.InstructorId.HasValue)
                {
                    var instructorExists = await _context.UserModels
                        .AnyAsync(u => u.UserId == courseDto.InstructorId.Value);
                        
                    if (!instructorExists)
                    {
                        logger.LogWarning("Instructor not found with ID: {InstructorId}", courseDto.InstructorId);
                        return BadRequest(new { 
                            message = "Instructor with given ID does not exist.",
                            field = "instructorId"
                        });
                    }
                }


                // Create new course
                var course = new Course
                {
                    CourseId = Guid.NewGuid(),
                    Title = courseDto.Title?.Trim(),
                    Description = courseDto.Description?.Trim(),
                    InstructorId = courseDto.InstructorId,
                    MediaUrl = courseDto.MediaUrl?.Trim()
                };

                // Log the course being created
                logger.LogInformation("Creating course: {CourseData}", new { 
                    course.CourseId, 
                    course.Title,
                    course.InstructorId 
                });

                _context.Courses.Add(course);
                await _context.SaveChangesAsync();

                var courseReadDto = new CourseReadDTO
                {
                    CourseId = course.CourseId,
                    Title = course.Title,
                    Description = course.Description,
                    InstructorId = course.InstructorId,
                    MediaUrl = course.MediaUrl
                };

                logger.LogInformation("Successfully created course with ID: {CourseId}", course.CourseId);
                return CreatedAtAction(nameof(GetCourse), new { id = course.CourseId }, courseReadDto);
            }
            catch (DbUpdateException dbEx)
            {
                logger.LogError(dbEx, "Database error while creating course");
                return StatusCode(500, new { 
                    message = "A database error occurred while creating the course.",
                    error = dbEx.InnerException?.Message ?? dbEx.Message
                });
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An unexpected error occurred while creating course");
                return StatusCode(500, new { 
                    message = "An unexpected error occurred while creating the course.",
                    error = ex.Message
                });
            }
        }

        // PUT: api/Course/{id}
        [HttpPut("{id}")]
        [Authorize(Policy = "RequireAdminOrInstructorRole")]
        public async Task<IActionResult> UpdateCourse(Guid id, CourseCreateDTO dto)
        {
            var existingCourse = await _context.Courses.FindAsync(id);
            if (existingCourse == null)
            {
                return NotFound();
            }

            if (dto.InstructorId.HasValue &&
                !await _context.UserModels.AnyAsync(u => u.UserId == dto.InstructorId.Value))
            {
                return Conflict("Instructor with given ID does not exist.");
            }

            existingCourse.Title = dto.Title;
            existingCourse.Description = dto.Description;
            existingCourse.InstructorId = dto.InstructorId;
            existingCourse.MediaUrl = dto.MediaUrl;

            await _context.SaveChangesAsync();
            return NoContent();
        }

        // DELETE: api/Course/{id}
        [HttpDelete("{id}")]
        [Authorize(Policy = "RequireAdminOrInstructorRole")]
        public async Task<IActionResult> DeleteCourse(Guid id)
        {
            var course = await _context.Courses.FindAsync(id);
            if (course == null)
            {
                return NotFound();
            }

            // Check foreign key constraint (Assessments)
            bool hasAssessments = await _context.Assessments.AnyAsync(a => a.CourseId == id);
            if (hasAssessments)
            {
                return Conflict("Cannot delete course with associated assessments.");
            }

            _context.Courses.Remove(course);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
