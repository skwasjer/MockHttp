using System;
using System.Linq;
using FluentAssertions;
using MockHttp.FluentAssertions;

namespace MockHttp
{
	public static class NullArgumentTest
	{
		public static void Execute(params object[] testArgs)
		{
			if (testArgs is null)
			{
				throw new ArgumentNullException(nameof(testArgs));
			}

			string testCase = (string)testArgs[0];
			var func = (Delegate)testArgs[1];
			string expectedParamName = (string)testArgs[2];
			object[] args = testArgs.Skip(3).ToArray();
			if (args.Length == 0)
			{
				// args can be empty for single param, so in that case provide array with 1 item.
				args = new object[1];
			}

			func.Should()
				.Throw<ArgumentNullException>(args)
				.Which.ParamName.Should()
				.Be(expectedParamName, "no {0} was provided for {1}", expectedParamName, testCase);
		}
	}
}
