using FluentValidation;

namespace Webefinity.Module.Blocks.Abstractions;

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
