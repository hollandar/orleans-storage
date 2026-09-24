using FluentValidation;
using System.Text.RegularExpressions;
using Webefinity.Module.Blog.Data;

namespace Webefinity.Module.Blog.Models;

public class ArticleEditModel
{
    public string? Id { get; set; } = null;
    public string NewId { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string Summary { get; set; } = string.Empty;
    public string? Image { get; set; } = string.Empty;
    public string Tags { get; set; } = string.Empty;
    public DateTimeOffset Date { get; set; } = DateTimeOffset.UtcNow;
    public string Author { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public ArticleState State { get; set; } = ArticleState.Draft;
}

public class ArticleEditModelValidator : AbstractValidator<ArticleEditModel>
{
    public ArticleEditModelValidator()
    {
        RuleFor(x => x.NewId).NotEmpty().WithMessage("NewId is required.");
        RuleFor(x => x.Title).NotEmpty().WithMessage("Title is required.");
        RuleFor(x => x.Author).NotEmpty().WithMessage("Author is required.");
        RuleFor(x => x.Date).NotEmpty().WithMessage("Date is required.");
        RuleFor(x => x.Tags).NotEmpty().Custom((v, c) =>
        {
            var individualTags = v.Split(',');
            var fail = false;
            foreach (var tag in individualTags)
            {
                if (string.IsNullOrWhiteSpace(tag))
                {
                    fail = true;
                }

                if (!Regex.IsMatch(tag, @"^[a-zA-Z0-9-]+$"))
                {
                    fail = true;
                }
            }

            if (fail)
            {
                c.AddFailure("Tags cannot contain empty values or spaces, only alphanumeric characters and hyphens are allowed.");
            }
        });
        RuleFor(x => x.Content).NotEmpty().WithMessage("Content is required.");
        RuleFor(x => x.State).IsInEnum().WithMessage("State must be a valid Article State value.");
    }
}   
