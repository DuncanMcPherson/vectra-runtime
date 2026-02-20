using FluentAssertions;

namespace VectraRuntime.Executor.Tests;

[TestFixture]
public class StackValueTests
{
    [Test]
    public void Null_Kind_IsNull() =>
        StackValue.Null.Kind.Should().Be(StackValueKind.Null);

    [Test]
    public void True_Kind_IsBool() =>
        StackValue.True.Kind.Should().Be(StackValueKind.Bool);

    [Test]
    public void True_AsBoolean_ReturnsTrue() =>
        StackValue.True.AsBoolean().Should().BeTrue();

    [Test]
    public void False_AsBoolean_ReturnsFalse() =>
        StackValue.False.AsBoolean().Should().BeFalse();

    [Test]
    public void FromNumber_RoundTrips() =>
        StackValue.FromNumber(3.14).AsNumber().Should().Be(3.14);

    [Test]
    public void FromString_RoundTrips() =>
        StackValue.FromString("hello").AsString().Should().Be("hello");

    [Test]
    public void AsNumber_WrongKind_Throws() =>
        StackValue.FromString("oops").Invoking(v => v.AsNumber())
            .Should().Throw<InvalidCastException>();

    [Test]
    public void AsString_WrongKind_Throws() =>
        StackValue.FromNumber(1).Invoking(v => v.AsString())
            .Should().Throw<InvalidCastException>();

    [Test]
    public void ToString_Null_ReturnsNullString() =>
        StackValue.Null.ToString().Should().Be("null");

    [Test]
    public void ToString_Number_UsesInvariantCulture() =>
        StackValue.FromNumber(1.5).ToString().Should().Be("1.5");
}