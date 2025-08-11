namespace Webefinity.Module.Blocks.Abstractions;

public enum PublishState
{
    Draft,
    Published,
    Archived
}

public static class PublishStateHelpers
{
    private static IDictionary<PublishState, PublishState[]> PublishStateTransitions = new Dictionary<PublishState, PublishState[]>
    {
        { PublishState.Draft, [ PublishState.Published, PublishState.Archived ] },
        { PublishState.Published, [PublishState.Draft, PublishState.Archived ] },
        { PublishState.Archived, [PublishState.Draft] }
    };

    public static PublishState[] GetAllowedTransitions(this PublishState state)
    {
        if (PublishStateTransitions.TryGetValue(state, out var allowedStates))
        {
            return allowedStates;
        }
        return Array.Empty<PublishState>();
    }

    public static string ToDisplayString(this PublishState state)
    {
        return state switch
        {
            PublishState.Draft => "Draft",
            PublishState.Published => "Published",
            PublishState.Archived => "Archived",
            _ => "Unknown"
        };
    }

    public static IEnumerable<KeyValuePair<PublishState, string>> ToKeyValuePairs()
    {
        return Enum.GetValues(typeof(PublishState)).Cast<PublishState>()
            .Select(state => new KeyValuePair<PublishState, string>(state, state.ToDisplayString()));
    }
}
