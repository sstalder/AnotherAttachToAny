using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Design;
using System.Windows.Forms;
using System.Windows.Forms.Design;

namespace ArcDev.AttachToAny.Components
{
	/// <summary>
	/// A dropdown that allows a user to enter multiple lines of strings, each line is then converted in to a collection.
	/// </summary>
	public class StringListUIEditor : UITypeEditor
	{
		private IWindowsFormsEditorService _frmsvr;

		/// <summary>
		/// Edits the specified object's value using the editor style indicated by the <see cref="M:System.Drawing.Design.UITypeEditor.GetEditStyle"></see> method.
		/// </summary>
		/// <param name="context">An <see cref="T:System.ComponentModel.ITypeDescriptorContext"></see> that can be used to gain additional context information.</param>
		/// <param name="provider">An <see cref="T:System.IServiceProvider"></see> that this editor can use to obtain services.</param>
		/// <param name="value">The object to edit.</param>
		/// <returns>
		/// The new value of the object. If the value of the object has not changed, this should return the same object it was passed.
		/// </returns>
		public override object EditValue(ITypeDescriptorContext context, IServiceProvider provider, object value)
		{
			if (provider == null)
			{
				return value;
			}
			_frmsvr = (IWindowsFormsEditorService) provider.GetService(typeof(IWindowsFormsEditorService));
			if (_frmsvr == null)
			{
				return value;
			}
			var panel = new Panel
			            {
				            Dock = DockStyle.Fill
			            };
			var lbl = new Label
			          {
				          Text = "Enter Strings; 1 item per line:",
				          ForeColor = Color.DarkGray,
				          AutoSize = true,
				          Dock = DockStyle.Top
			          };

			var tb = new TextBox
			         {
				         Multiline = true,
				         ScrollBars = ScrollBars.Both,
				         AcceptsReturn = true,
				         Dock = DockStyle.Fill
			         };
			panel.Controls.Add(tb);
			panel.Controls.Add(lbl);

			if (value != null)
			{
				var lst = (IEnumerable<string>) value;
				foreach (var s in lst)
				{
					tb.AppendText($"{s}{Environment.NewLine}");
				}
			}


			_frmsvr.DropDownControl(panel);

			var result = tb.Text.Trim();
			if (string.IsNullOrEmpty(result))
			{
				return value;
			}
			var array = result.Split(new[] {Environment.NewLine}, StringSplitOptions.RemoveEmptyEntries);
			return new List<string>(array);
		}

		/// <summary>
		/// Gets the editor style used by the <see cref="M:System.Drawing.Design.UITypeEditor.EditValue(System.IServiceProvider,System.Object)"></see> method.
		/// </summary>
		/// <param name="context">An <see cref="T:System.ComponentModel.ITypeDescriptorContext"></see> that can be used to gain additional context information.</param>
		/// <returns>
		/// A <see cref="T:System.Drawing.Design.UITypeEditorEditStyle"></see> value that indicates the style of editor used by the <see cref="M:System.Drawing.Design.UITypeEditor.EditValue(System.IServiceProvider,System.Object)"></see> method. If the <see cref="T:System.Drawing.Design.UITypeEditor"></see> does not support this method, then <see cref="M:System.Drawing.Design.UITypeEditor.GetEditStyle"></see> will return <see cref="F:System.Drawing.Design.UITypeEditorEditStyle.None"></see>.
		/// </returns>
		public override UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext context)
		{
			return UITypeEditorEditStyle.DropDown;
		}
	}
}