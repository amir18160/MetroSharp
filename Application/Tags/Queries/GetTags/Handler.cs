using Application.Core;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.Tags.Queries.GetTags
{
    public class Handler : IRequestHandler<Query, Result<TagsFeedDto>>
    {
        private readonly DataContext _context;
        private readonly IMapper _mapper;

        public Handler(DataContext context, IMapper mapper)
        {
            _mapper = mapper;
            _context = context;
        }

        public async Task<Result<TagsFeedDto>> Handle(Query request, CancellationToken cancellationToken)
        {
            var pinned = await _context.Tags
                .Where(t => t.IsPinned)
                .OrderByDescending(t => t.CreatedAt)
                .ProjectTo<TagDto>(_mapper.ConfigurationProvider)
                .ToListAsync(cancellationToken);


            var recommended = await _context.Tags
                .Where(t => t.Type == TagType.Recommended)
                .OrderByDescending(t => t.CreatedAt)
                .Take(30)
                .ProjectTo<TagDto>(_mapper.ConfigurationProvider)
                .ToListAsync(cancellationToken);

            var newTags = await _context.Tags
                .Where(t => t.Type == TagType.New)
                .OrderByDescending(t => t.CreatedAt)
                .Take(30)
                .ProjectTo<TagDto>(_mapper.ConfigurationProvider)
                .ToListAsync(cancellationToken);

            var newTopRated = await _context.Tags
                .Where(t => t.Type == TagType.NewTopRated)
                .OrderByDescending(t => t.CreatedAt)
                .Take(30)
                .ProjectTo<TagDto>(_mapper.ConfigurationProvider)
                .ToListAsync(cancellationToken);

            var updatedMovies = await _context.Tags
                .Where(t => t.Type == TagType.UpdatedMovie)
                .OrderByDescending(t => t.CreatedAt)
                .Take(50)
                .ProjectTo<TagDto>(_mapper.ConfigurationProvider)
                .ToListAsync(cancellationToken);

            var updatedSeries = await _context.Tags
                .Where(t => t.Type == TagType.UpdatedSeries)
                .OrderByDescending(t => t.CreatedAt)
                .Take(50)
                .ProjectTo<TagDto>(_mapper.ConfigurationProvider)
                .ToListAsync(cancellationToken);

            var result = new TagsFeedDto
            {
                Pinned = pinned, 
                Recommended = recommended,
                New = newTags,
                NewTopRated = newTopRated,
                UpdatedMovies = updatedMovies,
                UpdatedSeries = updatedSeries
            };

            return Result<TagsFeedDto>.Success(result);
        }
    }
}