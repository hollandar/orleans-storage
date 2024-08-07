using System.Formats.Tar;

namespace Webefinity.Results;

public class IdRequest<T0>
{
    public IdRequest()
    {
        
    }
    public IdRequest(T0 id0)
    {
        this.Id = id0;
    }

    public T0 Id { get; set; } = default(T0)!;

    public static implicit operator IdRequest<T0>(T0 id)
    {
        return new IdRequest<T0>(id);
    }

    public static implicit operator T0(IdRequest<T0> request)
    {
        return request.Id;
    }
}

public class IdRequest<T0, T1> : IdRequest<T0>
{
    public IdRequest():base()
    {
        
    }

    public IdRequest(T0 id0, T1 id1): base(id0)
    {
        this.Id1 = id1;
    }

    public T1 Id1 { get; set; } = default(T1)!;
}

public class IdRequest<T0, T1, T2> : IdRequest<T0, T1>
{
    public IdRequest():base()
    {
        
    }

    public IdRequest(T0 id0, T1 id1, T2 id2): base(id0, id1)
    {
        this.Id2 = id2;
    }

    public T2 Id2 { get; set; } = default(T2)!;
}

public class IdRequest<T0, T1, T2, T3> : IdRequest<T0, T1, T2>
{
    public IdRequest() : base()
    {

    }

    public IdRequest(T0 id0, T1 id1, T2 id2, T3 id3) : base(id0, id1, id2)
    {
        this.Id3 = id3;
    }
    public T3 Id3 { get; set; } = default(T3)!;
}

