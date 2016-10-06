using System;
using System.Collections;
using System.ComponentModel;
using System.Globalization;

namespace ArcDev.AnotherAttachToAny.Components
{
	// ReSharper disable once InconsistentNaming - we're converting an IList so this type name makes sense
	internal class IListTypeConverter : TypeConverter
	{
		/// <summary>
		/// Returns whether this converter can convert the object to the specified type, using the specified context.
		/// </summary>
		/// <param name="context">An <see cref="T:System.ComponentModel.ITypeDescriptorContext" /> that provides a format context.</param>
		/// <param name="destinationType">A <see cref="T:System.Type" /> that represents the type you want to convert to.</param>
		/// <returns>
		/// true if this converter can perform the conversion; otherwise, false.
		/// </returns>
		public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
		{
			return destinationType == typeof(string);
		}

		public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
		{
			if (destinationType != typeof(string) || value == null)
			{
				return null;
			}
			var count = ((IList) value).Count;
			return $"({count} Item{(count == 1 ? string.Empty : "s")})";
		}
	}
}