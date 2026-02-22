namespace Prompter.Web.DTOs;

public record PagedResponse<T>(IEnumerable<T> Items, int TotalCount);
