using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Webefinity.Module.Contact.Models
{
    public class ContactModel
    {
        public Guid Id { get; set; }
        public string Body { get; set; }
        public string Type { get; internal set; }
    }
}
