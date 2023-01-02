namespace MockHttp.Extensions;

public static class ListExtensionsTests
{
    internal interface IFoo
    {
    }

    internal class Foo : IFoo
    {
        public string Bar { get; set; } = default!;
    }

    internal class FooBar : IFoo
    {
        public string Baz { get; set; } = default!;
    }

    public sealed class IndexOf
    {
        [Theory]
        [MemberData(nameof(NullArgTestCases))]
        public void Given_that_required_arg_is_null_when_replacing_it_should_throw(IList<object> list, Type type, string expectedParamName)
        {
            Func<int> act = () => list.IndexOf<object>(type);

            act.Should()
                .ThrowExactly<ArgumentNullException>()
                .WithParameterName(expectedParamName);
        }

        public static IEnumerable<object?[]> NullArgTestCases()
        {
            IList<object> list = new List<object>();
            Type? type = null;

            yield return new object?[] { null, type, nameof(list) };
            yield return new object?[] { list, null, nameof(type) };
        }

        [Theory]
        [InlineData(typeof(Foo), 2)]
        [InlineData(typeof(FooBar), 5)]
        public void Given_that_list_contains_instances_of_type_when_getting_index_it_should_return_index_of_first_match(Type type, int expectedIndex)
        {
            var list = new List<IFoo?>
            {
                Mock.Of<IFoo>(),
                null,
                new Foo(),
                new Foo(),
                Mock.Of<IFoo>(),
                new FooBar(),
                new Foo(),
                new FooBar()
            };

            list.IndexOf(type).Should().Be(expectedIndex);
        }

        [Fact]
        public void Given_that_list_is_empty_when_getting_index_it_should_return_expected()
        {
            var list = new List<IFoo>();

            list.IndexOf(typeof(FooBar)).Should().Be(-1);
        }
    }

    public sealed class Replace
    {
        [Theory]
        [MemberData(nameof(NullArgTestCases))]
        public void Given_that_required_arg_is_null_when_replacing_it_should_throw(IList<object> list, object instance, string expectedParamName)
        {
            Action act = () => list.Replace(instance);

            act.Should()
                .ThrowExactly<ArgumentNullException>()
                .WithParameterName(expectedParamName);
        }

        public static IEnumerable<object?[]> NullArgTestCases()
        {
            IList<object> list = new List<object>();
            object? instance = null;

            yield return new[] { null, instance, nameof(list) };
            yield return new object?[] { list, null, nameof(instance) };
        }

        [Fact]
        public void Given_that_list_contains_instances_of_type_when_replacing_it_should_replace()
        {
            var list = new List<IFoo>
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

        [Fact]
        public void Given_that_list_contains_null_instance_when_replacing_it_should_ignore()
        {
            var list = new List<IFoo?>
            {
                new Foo { Bar = "1" },
                null,
                new FooBar { Baz = "2" }
            };
            var replaceWith = new Foo { Bar = "bar" };

            // Act
            list.Replace(replaceWith);

            // Assert
            list.Should()
                .BeEquivalentTo(new object?[] { replaceWith, null, new FooBar { Baz = "2" } },
                    opts => opts.RespectingRuntimeTypes()
                );
        }

        [Fact]
        public void Given_that_list_does_not_contain_instance_of_type_when_replacing_it_should_just_add()
        {
            var initial = new List<IFoo> { new Foo(), new Foo() };
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
        public void Given_that_list_is_empty_when_replacing_it_should_just_add()
        {
            var list = new List<IFoo>();
            var replaceWith = new FooBar();

            // Act
            list.Replace(replaceWith);

            // Assert
            list.Should()
                .ContainSingle()
                .Which.Should()
                .BeSameAs(replaceWith);
        }
    }
}
