using FluentValidation;

namespace Webefinity.Module.Blocks.Components.Blocks.Hero;

public class HeroModel
{
    public string Url { get; set; } = string.Empty;
    public string Left { get; set; } = String.Empty;
    public string Right { get; set; } = String.Empty;
    public string Text { set => this.Left = value; }
    public string TextColor { get; set; }
}

public class HeroModelValidator : AbstractValidator<HeroModel>
{
    public HeroModelValidator()
    {
        RuleFor(x => x.Url)
            .NotEmpty()
            .WithMessage("Url is required.");
    }
}
