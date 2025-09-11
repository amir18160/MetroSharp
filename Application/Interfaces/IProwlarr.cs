using Domain.Models.Prowlarr;

namespace Application.Interfaces;

public interface IProwlarr
{
    Task<List<Domain.Models.Prowlarr.Indexer>> GetIndexersAsync();
    Task<List<SearchResult>> Search(SearchOptions options);
}