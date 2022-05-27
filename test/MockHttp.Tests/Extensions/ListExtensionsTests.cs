using FluentAssertions;
using Xunit;

namespace MockHttp.Extensions;

public class ListExtensionsTests
{
    private interface IFoo
    {
    }

    public class Foo : IFoo
    {
        public string Bar { get; set; }
    }

    public class FooBar : IFoo
    {
        public string Baz { get; set; }
    }

    public class Replace
    {
        [Fact]
        public void Given_that_list_does_not_contain_instance_of_type_when_replacing_it_should_just_add()
        {
            var initial = new List<IFoo>()
            {
                new Foo(),
                new Foo()
            };
            var list = new List<IFoo>(initial);
            var replaceWith = new FooBar();

            // Act
            list.Replace(replaceWith);

            // Assert
            list.Should().HaveCount(3);
            list.Should().StartWith(initial);
            list.Should().EndWith(replaceWith);
        }

        [Fact]
        public void Given_that_list_contains_instances_of_type_when_replacing_it_should_replace()
        {
            var list = new List<IFoo>()
            {
                new Foo(),
                new Foo(),
                new Foo()
            };
            var replaceWith = new Foo { Bar = "bar" };

            // Act
            list.Replace(replaceWith);

            // Assert
            list.Should().ContainSingle().Which.Should().BeSameAs(replaceWith);
        }
    }
}
