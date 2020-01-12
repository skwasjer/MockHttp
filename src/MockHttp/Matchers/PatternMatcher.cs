namespace MockHttp.Matchers
{
	internal abstract class PatternMatcher
	{
		public abstract bool IsMatch(string value);
	}
}
