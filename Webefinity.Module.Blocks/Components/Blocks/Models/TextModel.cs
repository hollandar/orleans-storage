using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Webefinity.Module.Blocks.Components.Blocks;

public class TextModel
{
    public string Text { get; set; } = string.Empty;
}

public class TextModelValidator : AbstractValidator<TextModel>
{
    public TextModelValidator()
    {
        RuleFor(x => x.Text).NotEmpty();
    }
}
