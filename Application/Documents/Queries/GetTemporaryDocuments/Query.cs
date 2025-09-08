using Application.Core;
using MediatR;

namespace Application.Documents.Queries.GetTemporaryDocuments
{
    public class Query : PagingParams, IRequest<Result<PagedList<DocumentDto>>>
    {
    }
}