using System;
using System.Linq.Expressions;
using Moq;

namespace Scope.Tests
{
  public static class MockExtensions
  {
    public static Mock<T> Mock<T>(this T mock) where T : class
    {
      return Moq.Mock.Get(mock);
    }

    public static T ReturnsOn<T, TResult>(this T mock,
                                          Expression<Func<T, TResult>> expression,
                                          TResult value) where T : class
    {
      mock.Mock()
          .Setup(expression)
          .Returns(value);

      return mock;
    }    
  }
}
