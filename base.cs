// Services/BaseSharePointService.cs
using Microsoft.Graph;
using System.Text.Json;

public abstract class BaseSharePointService<T>
{
    protected readonly GraphServiceClient _graphClient;
    protected readonly string _siteId;
    protected readonly string _listId;

    public BaseSharePointService(GraphServiceClient graphClient, string siteId, string listId)
    {
        _graphClient = graphClient;
        _siteId = siteId;
        _listId = listId;
    }

    public async Task<List<T>> GetAllAsync(Func<ListItem, T> map)
    {
        var items = await _graphClient.Sites[_siteId].Lists[_listId].Items
            .Request()
            .Expand("fields")
            .GetAsync();

        return items.CurrentPage.Select(map).ToList();
    }

    public async Task<T?> GetByIdAsync(string id, Func<ListItem, T> map)
    {
        var item = await _graphClient.Sites[_siteId].Lists[_listId].Items[id]
            .Request()
            .Expand("fields")
            .GetAsync();

        return item == null ? default : map(item);
    }

    public async Task<string> CreateAsync(T model, Dictionary<string, object> fieldMap)
    {
        var newItem = new ListItem
        {
            Fields = new FieldValueSet { AdditionalData = fieldMap }
        };

        var created = await _graphClient.Sites[_siteId].Lists[_listId].Items
            .Request()
            .AddAsync(newItem);

        return created.Id;
    }

    public async Task UpdateAsync(string id, Dictionary<string, object> fieldMap)
    {
        var updatedItem = new FieldValueSet { AdditionalData = fieldMap };

        await _graphClient.Sites[_siteId].Lists[_listId].Items[id].Fields
            .Request()
            .UpdateAsync(updatedItem);
    }

    public async Task DeleteAsync(string id)
    {
        await _graphClient.Sites[_siteId].Lists[_listId].Items[id]
            .Request()
            .DeleteAsync();
    }
}
