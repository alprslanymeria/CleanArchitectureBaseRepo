namespace Base.Application.Contracts.Infrastructure.Mapping;

public interface IObjectMapper
{
    TDestination Map<TSource, TDestination>(TSource source);
    TDestination Map<TDestination>(object source);
    IEnumerable<TDestination> MapList<TSource, TDestination>(IEnumerable<TSource> source);
}