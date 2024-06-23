namespace Domain.Primitives;

// https://www.youtube.com/watch?v=P5CRea21R2E&list=PLYpjLpq5ZDGstQ5afRz-34o_0dexr1RGa
public abstract class ValueObject
{
    public abstract IEnumerable<object> GetAtomicValues();
}
