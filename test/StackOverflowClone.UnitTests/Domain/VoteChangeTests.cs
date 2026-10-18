using StackOverflowClone.Domain.Votes;

namespace StackOverflowClone.UnitTests.Domain;

public sealed class VoteChangeTests
{
    [Theory]
    [InlineData(null, 1, VoteAction.Add, 1)]
    [InlineData(null, -1, VoteAction.Add, -1)]
    [InlineData(null, 0, VoteAction.None, 0)]
    [InlineData(1, 1, VoteAction.None, 0)]
    [InlineData(-1, -1, VoteAction.None, 0)]
    [InlineData(1, -1, VoteAction.Update, -2)]
    [InlineData(-1, 1, VoteAction.Update, 2)]
    [InlineData(1, 0, VoteAction.Remove, -1)]
    [InlineData(-1, 0, VoteAction.Remove, 1)]
    public void Calculate_ReturnsExpectedActionAndScoreDelta(int? currentValue, int newValue, VoteAction expectecAction, int expectedDelta)
    {
        var change = VoteChange.Calculate(currentValue, newValue);

        Assert.Equal(new VoteChange(expectecAction, expectedDelta), change);
    }

    [Theory]
    [InlineData(2)]
    [InlineData(-2)]
    [InlineData(5)]
    public void Calculate_WithValueOutsideAllowedRange_Throws(int newValue)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => VoteChange.Calculate(null, newValue));
    }
}
