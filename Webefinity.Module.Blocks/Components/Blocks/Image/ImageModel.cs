using FluentValidation;

namespace Webefinity.Module.Blocks.Components.Blocks.Image
{
    public class ImageModel
    {
        public string Url { get; set; }
        public string Alt { get; set; }
    }

    public class ImageModelValidator : AbstractValidator<ImageModel>
    {
        public ImageModelValidator()
        {
            RuleFor(x => x.Url)
                .NotEmpty()
                .WithMessage("Url is required.");

            RuleFor(x => x.Alt)
                .NotEmpty().
                WithMessage("Alt text is required.");
        }
    }
}
