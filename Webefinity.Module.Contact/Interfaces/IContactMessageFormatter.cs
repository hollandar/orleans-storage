using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Webefinity.Module.Contact.Models;

namespace Webefinity.Module.Contact.Interfaces;


public interface IContactMessageFormatter
{
    MailMessageModel FormatMessage(IEnumerable<ContactModel> model);
    bool HandlesType(string type);
}
