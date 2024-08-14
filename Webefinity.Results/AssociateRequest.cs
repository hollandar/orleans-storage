using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Webefinity.Results;

public struct AssociateRequest<TAssocitedId, TModel>
{
    public AssociateRequest()
    {
        
    }

    public AssociateRequest(TAssocitedId associtedId, TModel model)
    {
        this.AssociatedId = associtedId;
        this.Model = model;
    }

    public TAssocitedId AssociatedId { get; set; }
    public TModel Model { get; set; }
}
