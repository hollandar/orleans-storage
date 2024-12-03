using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Webefinity.Module.Messaging.Abstractions.Models;

public class AddressModel
{
    public Guid Id { get; set; }
    public string Email { get; set; }
    public string Phone { get; set; }
    public string Name { get; set; }
    public AddressType Type { get; set; }
}
