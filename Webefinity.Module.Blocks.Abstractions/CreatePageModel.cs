using FluentValidation;

namespace Webefinity.Module.Blocks.Abstractions;

public class CreatePageModel
{
    public string PageName { get; set; } = string.Empty;
    public string PageTitle { get; set; } = string.Empty;
}

public class CreatePageModelValidator : AbstractValidator<CreatePageModel>
{
    public CreatePageModelValidator()
    {
        RuleFor(x => x.PageName).NotEmpty().Matches("^[a-z0-9-]+$").WithMessage("Page name is required, and must consist of lowercase alphanumeric characters or hyphens.");
        RuleFor(x=>x.PageTitle).NotEmpty().WithMessage("Page title is required.");
    }
}
