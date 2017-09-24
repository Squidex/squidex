namespace Squidex.Infrastructure.Actors
{
    public interface IActors
    {
        IActor Get(string id);
    }
}