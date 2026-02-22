using Prompter.Core.Entities;
using Prompter.Core.Enums;

namespace Prompter.Core.Repositories;

public interface IPromptRepository
{
    Task AddRangeAsync(IEnumerable<Prompt> prompts);
    Task<IEnumerable<Prompt>> GetAllAsync();
    Task<Prompt?> GetByIdAsync(int id);
    Task<IEnumerable<Prompt>> GetByStatusAsync(PromptStatus status);
    Task UpdateAsync(Prompt prompt);
    Task<(IEnumerable<Prompt> Items, int TotalCount)> GetPagedAsync(int skip, int take);
}
