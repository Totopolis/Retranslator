namespace Domain.Primitives;

public abstract class Entity<T>
    where T : struct
{
    protected Entity(T id) => Id = id;

    protected Entity()
    {
    }

    public T Id { get; private set; }
}
