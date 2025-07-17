// Services/ProjectService.cs
public class ProjectService : BaseSharePointService<ProjectModel>
{
    public ProjectService(GraphServiceClient client, string siteId, string listId)
        : base(client, siteId, listId) { }

    private static ProjectModel Map(ListItem item)
    {
        var fields = item.Fields.AdditionalData;
        return new ProjectModel
        {
            Title = fields["Title"]?.ToString(),
            Description = fields["Description"]?.ToString(),
            Status = fields["Status"]?.ToString()
        };
    }

    private static Dictionary<string, object> MapFields(ProjectModel model) => new()
    {
        { "Title", model.Title },
        { "Description", model.Description },
        { "Status", model.Status }
    };

    public Task<List<ProjectModel>> GetAll() => GetAllAsync(Map);
    public Task<ProjectModel?> Get(string id) => GetByIdAsync(id, Map);
    public Task<string> Create(ProjectModel model) => CreateAsync(model, MapFields(model));
    public Task Update(string id, ProjectModel model) => UpdateAsync(id, MapFields(model));
    public Task Delete(string id) => DeleteAsync(id);
}

// Services/TaskService.cs
public class TaskService : BaseSharePointService<TaskModel>
{
    public TaskService(GraphServiceClient client, string siteId, string listId)
        : base(client, siteId, listId) { }

    private static TaskModel Map(ListItem item)
    {
        var fields = item.Fields.AdditionalData;
        return new TaskModel
        {
            Title = fields["Title"]?.ToString(),
            DueDate = DateTime.Parse(fields["DueDate"]?.ToString() ?? DateTime.MinValue.ToString())
        };
    }

    private static Dictionary<string, object> MapFields(TaskModel model) => new()
    {
        { "Title", model.Title },
        { "DueDate", model.DueDate }
    };

    public Task<List<TaskModel>> GetAll() => GetAllAsync(Map);
    public Task<TaskModel?> Get(string id) => GetByIdAsync(id, Map);
    public Task<string> Create(TaskModel model) => CreateAsync(model, MapFields(model));
    public Task Update(string id, TaskModel model) => UpdateAsync(id, MapFields(model));
    public Task Delete(string id) => DeleteAsync(id);
}
