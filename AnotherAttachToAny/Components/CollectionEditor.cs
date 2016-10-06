using System;
using System.ComponentModel;
using System.Diagnostics;
using ArcDev.AnotherAttachToAny.Extensions;

namespace ArcDev.AnotherAttachToAny.Components
{
	public class CollectionEditor : System.ComponentModel.Design.CollectionEditor
	{
		public CollectionEditor(Type type)
			: base(type)
		{
			Debug.WriteLine("Create Collection Editor");
		}

		private CollectionForm EditorForm { get; set; }


		/// <summary>
		/// Creates a new form to display and edit the current collection.
		/// </summary>
		/// <returns>
		/// A <see cref="T:System.ComponentModel.Design.CollectionEditor.CollectionForm"></see> to provide as the user interface for editing the collection.
		/// </returns>
		protected override CollectionForm CreateCollectionForm()
		{
			EditorForm = base.CreateCollectionForm();
			EditorForm.Text = CollectionItemType.GetCustomAttributeValue<DisplayNameAttribute, string>(x => x.DisplayName);
			return EditorForm;
		}

		protected override Type[] CreateNewItemTypes()
		{
			return new[] {CollectionItemType};
		}
	}

	public class CollectionEditor<T> : CollectionEditor
	{
		public CollectionEditor()
			: base(typeof(T))
		{
		}
	}
}