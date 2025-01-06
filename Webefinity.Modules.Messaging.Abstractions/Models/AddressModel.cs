using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Webefinity.Module.Messaging.Abstractions.Models;

public class AddressModel
{
    public Guid Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Name { get; set; }= string.Empty;
    public AddressType Type { get; set; }
}
