using System;

namespace Webefinity.Module.Blocks.Abstractions;

public record PageControlLink(string icon, string alt, string link, int order = 0, string? text = null);
