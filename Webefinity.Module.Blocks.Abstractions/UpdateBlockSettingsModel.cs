using System;
using FluentValidation;
using Webefinity.Module.Blocks.Abstractions;

namespace Webefinity.Module.Blocks.Abstractions;

public class UpdateBlockSettingsRequest
{
    public Guid Id { get; set; } = Guid.Empty;
    public string Name { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public PublishState State { get; set; } = PublishState.Draft;
    public string LinkText { get; set; } = string.Empty;
    public int LinkOrder { get; set; } = 0;
}

public class UpdateBlockSettingsRequestValidator : AbstractValidator<UpdateBlockSettingsRequest>
{
    public UpdateBlockSettingsRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200).WithMessage("Name is required and is limited to 200 characters.");
        RuleFor(x => x.Title).NotEmpty().MaximumLength(200).WithMessage("Title is required and is limited to 200 characters.");
        RuleFor(x => x.State).IsInEnum().WithMessage("Invalid publish state.");
        RuleFor(x => x.LinkText).MaximumLength(200).WithMessage("Link text cannot exceed 200 characters.");
    }
}