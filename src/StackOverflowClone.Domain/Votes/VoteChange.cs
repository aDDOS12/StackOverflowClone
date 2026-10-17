namespace StackOverflowClone.Domain.Votes;

public enum VoteAction
{
    None,
    Add,
    Update,
    Remove
}

public readonly record struct VoteChange(VoteAction Action, int ScoreDelta)
{
    public static VoteChange Calculate(int? currentValue, int newValue)
    {
        if (newValue is not (-1 or 0 or 1))
        {
            throw new ArgumentOutOfRangeException(nameof(newValue), "Vote value must be -1, 0 or 1.");
        }

        var delta = newValue - (currentValue ?? 0);

        if (delta == 0)
        {
            return new VoteChange(VoteAction.None, 0);
        }

        var action = (currentValue, newValue) switch
        {
            (null, _) => VoteAction.Add,
            (_, 0) => VoteAction.Remove,
            _ => VoteAction.Update
        };

        return new VoteChange(action, delta);
    }
}
