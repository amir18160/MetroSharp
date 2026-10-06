using Application.Core;
using Application.Documents.DTOs;
using MediatR;

namespace Application.Documents.Queries.GetTemporaryDocuments;

public class Query : PagingParams, IRequest<Result<PagedList<DocumentDto>>>
{
}