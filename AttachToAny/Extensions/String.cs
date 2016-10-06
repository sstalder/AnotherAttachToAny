namespace RyanConrad.AttachToAny.Extensions
{
	public static partial class AttachToAnyExtensions
	{
		public static string With(this string format, params object[] args)
		{
			return string.Format(format, args);
		}
	}
}