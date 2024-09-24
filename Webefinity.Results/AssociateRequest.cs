using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Webefinity.Results;

public struct AssociateRequest<TAssociated, TValue>
{
    public AssociateRequest()
    {
        
    }

    public AssociateRequest(TAssociated associted, TValue value)
    {
        this.Associated = associted;
        this.Value = value;
    }

    public TAssociated Associated { get; set; } = default!;
    public TValue Value { get; set; } = default!;
}
