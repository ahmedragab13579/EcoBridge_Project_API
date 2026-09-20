using System.Text.Json.Serialization;

namespace EcoBridge.Application.Results;

public class PagedResult<T>
{
    [JsonPropertyName("data")]
    public List<T> Items { get; set; } = new();

    [JsonPropertyName("pageNumber")]
    public int PageNumber { get; set; }

    [JsonIgnore]
    public int Page { get => PageNumber; set => PageNumber = value; }

    [JsonPropertyName("pageSize")]
    public int PageSize { get; set; }

    [JsonPropertyName("totalCount")]
    public int TotalCount { get; set; }

    [JsonPropertyName("totalPages")]
    public int TotalPages => PageSize > 0 ? (int)Math.Ceiling(TotalCount / (double)PageSize) : 0;

    [JsonPropertyName("hasNextPage")]
    public bool HasNextPage => PageNumber < TotalPages;

    [JsonPropertyName("hasPreviousPage")]
    public bool HasPreviousPage => PageNumber > 1;

    public PagedResult() { }

    public PagedResult(List<T> items, int totalCount, int pageNumber, int pageSize)
    {
        Items = items ?? new List<T>();
        TotalCount = totalCount;
        PageNumber = pageNumber;
        PageSize = pageSize;
    }
}
