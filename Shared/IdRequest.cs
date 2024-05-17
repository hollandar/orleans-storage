namespace Shared;

public class IdRequest<T0>
{
    public T0 Id { get; set; } = default(T0)!;
}

public class IdRequest<T0, T1> : IdRequest<T0>
{
    public T1 Id1 { get; set; } = default(T1)!;
}

public class IdRequest<T0, T1, T2> : IdRequest<T0, T1>
{
    public T2 Id2 { get; set; } = default(T2)!;
}

public class IdRequest<T0, T1, T2, T3> : IdRequest<T0, T1, T2>
{
    public T3 Id3 { get; set; } = default(T3)!;
}

