using FluentAssertions;

namespace UnitTests.Extensions
{
    public static class ObjectExtensions
    {
        public static void AssertControllerReturn<T>(this object? returnValue, T expectedResponse) where T : class
        {
            returnValue.Should().BeAssignableTo<T>();
            var response = returnValue as T;
            response.Should().NotBeNull();
            response.Should().Be(expectedResponse);
        }
    }
}
