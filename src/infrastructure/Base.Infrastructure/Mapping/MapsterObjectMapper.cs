using Base.Application.Contracts.Infrastructure.Mapping;
using MapsterMapper;

namespace Base.Infrastructure.Mapping;

public class MapsterObjectMapper(IMapper mapper) : IObjectMapper
{
    private readonly IMapper _mapper = mapper;

    public TDestination Map<TSource, TDestination>(TSource source)
    {
        return source is null
                    ? default!
                    : _mapper.Map<TDestination>(source);
    }

    public TDestination Map<TDestination>(object source)
    {
        return source is null
                    ? default!
                    : _mapper.Map<TDestination>(source);
    }

    public IEnumerable<TDestination> MapList<TSource, TDestination>(IEnumerable<TSource> source)
    {
        return source is null
                    ? []
                    : source.Select(s => _mapper.Map<TDestination>(s!));
    }
}