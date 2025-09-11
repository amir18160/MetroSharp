using Application.Core;
using Domain.Enums;
using MediatR;

namespace Application.Documents.Commands.UpdateTemporaryDocument;

public class Command : IRequest<Result<Unit>>
{
    public Guid Id { get; set; }
    public string ImdbId { get; set; }
    public DocumentType Type { get; set; }
    public int? Season { get; set; }
    public int? Episode { get; set; }
    public string FileName { get; set; }
    public bool? IsSubbed { get; set; }
}