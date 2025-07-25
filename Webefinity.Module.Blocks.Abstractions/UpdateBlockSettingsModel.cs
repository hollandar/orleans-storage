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
}

public class UpdateBlockSettingsRequestValidator : AbstractValidator<UpdateBlockSettingsRequest>
{
    public UpdateBlockSettingsRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty().WithMessage("Name is required.");
        RuleFor(x => x.Title).NotEmpty().WithMessage("Title is required.");
        RuleFor(x => x.State).IsInEnum().WithMessage("Invalid publish state.");
    }
}