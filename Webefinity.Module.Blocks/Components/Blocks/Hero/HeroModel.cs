using FluentValidation;

namespace Webefinity.Module.Blocks.Components.Blocks.Hero;

public class HeroModel
{
    public string Url { get; set; } = string.Empty;
    public string Text { get; set; } = String.Empty;
}

public class HeroModelValidator : AbstractValidator<HeroModel>
{
    public HeroModelValidator()
    {
        RuleFor(x => x.Url)
            .NotEmpty()
            .WithMessage("Url is required.");

        RuleFor(x => x.Text)
            .NotEmpty().
            WithMessage("Text is required.");
    }
}
