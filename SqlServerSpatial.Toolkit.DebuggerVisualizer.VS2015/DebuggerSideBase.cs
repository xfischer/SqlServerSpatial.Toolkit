using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Windows.Forms.Integration;
using System.Windows.Media;
using Microsoft.SqlServer.Types;
using Microsoft.VisualStudio.DebuggerVisualizers;
using SqlServerSpatial.Toolkit.Viewers;
using System.Windows;

namespace SqlServerSpatial.Toolkit.Visualizer
{
	public abstract class DebuggerSideBase : DialogDebuggerVisualizer
	{

		protected abstract SqlGeometry GetObject(IVisualizerObjectProvider objectProvider);

		protected override void Show(IDialogVisualizerService windowService, IVisualizerObjectProvider objectProvider)
		{
			// Initialize form
			InitializeComponent();
			try
			{
				SqlGeometry geometry = GetObject(objectProvider);

				_spatialViewerControl.SetGeometry(new SqlGeometryStyled(geometry, Color.FromArgb(200, 0, 175, 0), Colors.Black, 1f, null, true));

				_form.Shown += (o, e) => _spatialViewerControl.ResetView();

				// Show the grid with the list
				windowService.ShowDialog(_form);
			}
			catch (Exception e)
			{
				System.Windows.Forms.MessageBox.Show("Error: " + e.ToString());
			}
		}

		#region Visualizer host Form

		protected ElementHost _elementHost1;
		protected ISpatialViewer _spatialViewerControl;
		protected Form _form;
		/// <summary>
		/// Méthode requise pour la prise en charge du concepteur - ne modifiez pas
		/// le contenu de cette méthode avec l'éditeur de code.
		/// </summary>
		protected void InitializeComponent()
		{
			this._elementHost1 = new System.Windows.Forms.Integration.ElementHost();
			this._spatialViewerControl = new SpatialViewer_GDIHost();
			_form = new System.Windows.Forms.Form();
			_form.SuspendLayout();
			// 
			// elementHost1
			// 
			this._elementHost1.Dock = System.Windows.Forms.DockStyle.Fill;
			this._elementHost1.Location = new System.Drawing.Point(0, 0);
			this._elementHost1.Name = "elementHost1";
			this._elementHost1.Size = new System.Drawing.Size(824, 581);
			this._elementHost1.TabIndex = 0;
			this._elementHost1.Text = "elementHost1";
			this._elementHost1.Child = (UIElement)this._spatialViewerControl;
			// 
			// Form1
			// 
			_form.AutoScaleDimensions = new System.Drawing.SizeF(12F, 25F);
			_form.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Inherit;
			_form.ClientSize = new System.Drawing.Size(824, 581);
			_form.Controls.Add(this._elementHost1);
			_form.Name = "Spir Spatial Toolkit";
			_form.Text = "Spir Spatial Toolkit";
			_form.ResumeLayout(false);

		}

		#endregion
	}
}
