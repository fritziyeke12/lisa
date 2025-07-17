// Controllers/ProjectsController.cs
[ApiController]
[Route("api/[controller]")]
public class ProjectsController : ControllerBase
{
    private readonly ProjectService _service;

    public ProjectsController(ProjectService service) => _service = service;

    [HttpGet]
    public async Task<IActionResult> Get() => Ok(await _service.GetAll());

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(string id)
    {
        var item = await _service.Get(id);
        return item == null ? NotFound() : Ok(item);
    }

    [HttpPost]
    public async Task<IActionResult> Create(ProjectModel model)
    {
        var id = await _service.Create(model);
        return CreatedAtAction(nameof(GetById), new { id }, model);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(string id, ProjectModel model)
    {
        await _service.Update(id, model);
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(string id)
    {
        await _service.Delete(id);
        return NoContent();
    }
}
