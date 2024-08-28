using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Webefinity.Module.Contact.Services;

namespace Webefinity.Module.Contact.Models;

public enum BodyFormat
{
    Text,
    Html,
    Md
}

public class MailMessageModel
{
    public string From { get; set; }
    public string To { get; set; }
    public string Subject { get; set; }
    public string Body { get; set; }
    public BodyFormat Format { get; set; } = BodyFormat.Text;
}

public class MailMessageModelValidator : AbstractValidator<MailMessageModel>
{
    public MailMessageModelValidator()
    {
        RuleFor(r => r.From).NotEmpty().EmailAddress();
        RuleFor(r => r.To).NotEmpty().EmailAddress();
        RuleFor(r => r.Subject).NotEmpty();
        RuleFor(r => r.Body).NotEmpty();
    }
}