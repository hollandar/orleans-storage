using FluentValidation;

namespace Webefinity.Module.Blocks.Components.Blocks.Tiles;

public class TilesModel
{
    public List<TileModel> Tiles { get; set; } = new List<TileModel>();
}

public class TilesModelValidator : AbstractValidator<TilesModel>
{
    public TilesModelValidator()
    {
        RuleForEach(x => x.Tiles).SetValidator(new TileModelValidator());
    }
}

public class TileModel
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Title { get; set; } = string.Empty;
    public string Md { get; set; } = string.Empty;
    public string? ImageUri { get; set; } = null;
    public string? IconResource { get; set; } = null;
}

public class TileModelValidator : AbstractValidator<TileModel>
{
    public TileModelValidator()
    {
        RuleFor(r => r.Title).NotEmpty();
        RuleFor(x => x.Md).NotNull();
    }
}
