namespace Application.Interfaces
{
    public interface IProwlarr
    {
        Task<List<Domain.Models.Prowlarr.Indexer>> GetIndexersAsync();
        Task<List<Domain.Models.Prowlarr.SearchResult>> Search(Domain.Models.Prowlarr.SearchOptions options);
    }
}