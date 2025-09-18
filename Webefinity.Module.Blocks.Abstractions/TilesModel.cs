using FluentValidation;

namespace Webefinity.Module.Blocks.Abstractions;

public class TilesModel
{
    public TilesDetailModel Detail { get; set; } = new TilesDetailModel();
    public List<TileModel> Tiles { get; set; } = new List<TileModel>();

}

public class TilesModelValidator : AbstractValidator<TilesModel>
{
    public TilesModelValidator()
    {
        RuleFor(x => x.Detail).SetValidator(new TilesDetailModelValidator());
        RuleForEach(x => x.Tiles).SetValidator(new TileModelValidator());
    }
}

public class TilesDetailModel
{
    public string TextureUri { get; set; } = string.Empty;
    public int TextureSize { get; set; } = 512;
    public int TileSize { get; set; } = 256;
    public int X { get; set; } = 2;
    public int Y { get; set; } = 2;

}

public class TilesDetailModelValidator : AbstractValidator<TilesDetailModel>
{
    public TilesDetailModelValidator()
    {
        RuleFor(x => x.TextureSize).GreaterThan(0);
        RuleFor(x => x.TileSize).GreaterThan(0);
        RuleFor(x => x.X).GreaterThan(0);
        RuleFor(x => x.Y).GreaterThan(0);
        RuleFor(x => new { x.X, x.Y }).Must(t => t.X * t.Y > 0).WithMessage("X * Y must be greater than 0");
    }
}

public class TileModel
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public int TextureIndex { get; set; } = 0;
    public string Title { get; set; } = string.Empty;
    public string Md { get; set; } = string.Empty;
}

public class TileModelValidator : AbstractValidator<TileModel>
{
    public TileModelValidator()
    {
        RuleFor(x => x.TextureIndex).GreaterThanOrEqualTo(0);
        RuleFor(x => x.Md).NotNull();
    }
}
