using System;
using System.Linq;
using System.Reflection;

namespace ArcDev.AttachToAny.Extensions
{
	public static partial class AttachToAnyExtensions
	{
		/// <summary>
		/// Gets the custom attribute.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="type">The type.</param>
		/// <returns></returns>
		public static T GetCustomAttribute<T>(this Type type) where T : Attribute
		{
			var attr = type.GetCustomAttributes<T>().FirstOrDefault();
			return attr;
		}

		/// <summary>
		/// Gets the custom attribute value.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <typeparam name="TExpected">The type of the Expected.</typeparam>
		/// <param name="type">The type.</param>
		/// <param name="expression">The expression.</param>
		/// <returns></returns>
		public static TExpected GetCustomAttributeValue<T, TExpected>(this Type type, Func<T, TExpected> expression) where T : Attribute
		{
			var attribute = type.GetCustomAttribute<T>();
			return attribute == null ? default(TExpected) : expression(attribute);
		}
	}
}