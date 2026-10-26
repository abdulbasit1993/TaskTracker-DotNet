using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaskTracker.Api.Data;
using TaskTracker.Api.Models;

namespace TaskTracker.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TasksController : ControllerBase
{
    private readonly TaskTrackerDbContext _context;

    public TasksController(TaskTrackerDbContext context)
    {
        _context = context;
    }

    // GET: api/tasks
    [HttpGet]
    public async Task<ActionResult<IEnumerable<TaskItem>>> GetTasks()
    {
        var tasks = await _context.Tasks
            .AsNoTracking()
            .OrderByDescending(task => task.CreatedAt)
            .ToListAsync();

        return Ok(tasks);
    }

    // GET api/tasks/{id}
    [HttpGet("{id:int}")]
    public async Task<ActionResult<TaskItem>> GetTask(int id)
    {
        var task = await _context.Tasks
            .AsNoTracking()
            .FirstOrDefaultAsync(task => task.Id == id);

        if (task is null)
        {
            return NotFound();
        }

        return Ok(task);
    }

    // POST: api/tasks
    [HttpPost]
    public async Task<ActionResult<TaskItem>> CreateTask(
        [FromBody] CreateTaskRequest request)
    {
        var task = new TaskItem
        {
            Title = request.Title.Trim(),
            Description = request.Description,
            IsCompleted = false,
            CreatedAt = DateTime.UtcNow
        };

        if (string.IsNullOrWhiteSpace(task.Title))
        {
            ModelState.AddModelError(
                nameof(request.Title),
                "Title cannot be empty.");

            return ValidationProblem(ModelState);
        }

        _context.Tasks.Add(task);

        await _context.SaveChangesAsync();

        return Created($"/api/tasks/{task.Id}", task);
    }

    // PUT api/tasks/{id}
    [HttpPut("{id:int}")]
    public async Task<ActionResult<TaskItem>> UpdateTask(
        int id,
        [FromBody] UpdateTaskRequest request)
    {
        // Find the existing task
        var task = await _context.Tasks
            .FirstOrDefaultAsync(task => task.Id == id);

        // Return 404 if it doesn't exist
        if (task is null)
        {
            return NotFound();
        }

        // Validate the title after trimming the whitespace
        var title = request.Title.Trim();

        if (string.IsNullOrWhiteSpace(title))
        {
            ModelState.AddModelError(
                nameof(request.Title),
                "Title cannot be empty."
            );

            return ValidationProblem(ModelState);
        }

        // Update only the fields this endpoint allows
        task.Title = title;
        task.Description = request.Description?.Trim();

        await _context.SaveChangesAsync();

        return Ok(task);
    }

    // DELETE api/tasks/{id}
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteTask(int id)
    {
        // Find the existing task
        var task = await _context.Tasks
            .FirstOrDefaultAsync(task => task.Id == id);

        // Return 404 if it doesn't exist
        if (task is null)
        {
            return NotFound();
        }

        // Mark the task for deletion
        _context.Tasks.Remove(task);

        // Delete it from the database
        await _context.SaveChangesAsync();

        return NoContent();
    }

    // PATCH api/tasks/{id}/complete
    [HttpPatch("{id:int}/complete")]
    public async Task<ActionResult<TaskItem>> UpdateTaskCompletion(
        int id,
        [FromBody] UpdateTaskCompletionRequest request)
    {
        // Find the existing task
        var task = await _context.Tasks
            .FirstOrDefaultAsync(task => task.Id == id);

        // Return 404 if it doesn't exist
        if (task is null)
        {
            return NotFound();
        }

        // Update only the completion status
        task.IsCompleted = request.IsCompleted;

        await _context.SaveChangesAsync();

        return Ok(task);
    }
}