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
        RuleFor(x => x.PageName).NotEmpty();
        RuleFor(x=>x.PageTitle).NotEmpty();
    }
}
