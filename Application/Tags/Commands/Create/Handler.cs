using Application.Core;
using AutoMapper;
using Domain.Entities;
using Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.Tags.Commands.Create
{
    public class Handler : IRequestHandler<Command, Result<Unit>>
    {
        private readonly DataContext _context;
        private readonly IMapper _mapper;

        public Handler(DataContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<Result<Unit>> Handle(Command request, CancellationToken cancellationToken)
        {
            var existingTag = await _context.Tags
                .FirstOrDefaultAsync(t => t.OmdbItemId == request.OmdbItemId && t.Type == request.Type);

            if (existingTag != null)
            {
                _mapper.Map(request, existingTag);
            }
            else
            {
                var newTag = _mapper.Map<Tag>(request);
                newTag.CreatedAt = DateTime.UtcNow;
                _context.Tags.Add(newTag);
            }

            if (request.IsPinned)
            {
                var pinnedTags = await _context.Tags
                    .Where(t => t.IsPinned)
                    .OrderBy(t => t.CreatedAt)
                    .ToListAsync();

                if (pinnedTags.Count >= 10)
                {
                    pinnedTags.First().IsPinned = false;
                }
            }

            if (request.Type == TagType.Recommended)
            {
                var recommendedTags = await _context.Tags
                    .Where(t => t.Type == TagType.Recommended)
                    .OrderBy(t => t.CreatedAt)
                    .ToListAsync();

                if (recommendedTags.Count >= 20)
                {
                    _context.Tags.Remove(recommendedTags.First());
                }
            }

            

            var result = await _context.SaveChangesAsync() > 0;

            if (!result)
            {
                return Result<Unit>.Failure("Failed to create or update tag");
            }

            return Result<Unit>.Success(Unit.Value);
        }
    }
}