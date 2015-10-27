using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace SqlServerSpatialTypes.Toolkit
{
	public class VisualHostContainer : FrameworkElement
	{
		// Create a collection of child visual objects.
		private VisualCollection _children;
		public VisualHostContainer()
		{
			_children = new VisualCollection(this);
			//_children.Add(CreateDrawingVisualRectangle());
			//_children.Add(CreateDrawingVisualText());
			//_children.Add(CreateDrawingVisualEllipses());

			// Add the event handler for MouseLeftButtonUp.
			this.MouseLeftButtonUp += new MouseButtonEventHandler(MyVisualHost_MouseLeftButtonUp);
		}

		// Create a DrawingVisual that contains a rectangle.
		private DrawingVisual CreateDrawingVisualRectangle()
		{
			DrawingVisual drawingVisual = new DrawingVisual();

			// Retrieve the DrawingContext in order to create new drawing content.
			DrawingContext drawingContext = drawingVisual.RenderOpen();

			// Create a rectangle and draw it in the DrawingContext.
			Rect rect = new Rect(new Point(160, 100), new Size(320, 80));
			drawingContext.DrawRectangle(Brushes.LightBlue, new Pen(Brushes.Black, 1), rect);

			// Persist the drawing content.
			drawingContext.Close();

			return drawingVisual;
		}

		public void AddGeometry(Geometry geometry)
		{
			_children.Add(CreateDrawingVisualFromGeometry(geometry));
		}

		// Create a DrawingVisual that contains a rectangle.
		private DrawingVisual CreateDrawingVisualFromGeometry(Geometry geometry)
		{

			DrawingVisual drawingVisual = new DrawingVisual();

			// Retrieve the DrawingContext in order to create new drawing content.
			DrawingContext drawingContext = drawingVisual.RenderOpen();

			// Create a rectangle and draw it in the DrawingContext.
			drawingContext.DrawGeometry(Brushes.LightBlue, new Pen(Brushes.Black, 1) { LineJoin = PenLineJoin.Bevel }, geometry);

			// Persist the drawing content.
			drawingContext.Close();

			return drawingVisual;
		}

		// Provide a required override for the VisualChildrenCount property.
		protected override int VisualChildrenCount
		{
			get { return _children.Count; }
		}

		// Provide a required override for the GetVisualChild method.
		protected override Visual GetVisualChild(int index)
		{
			if (index < 0 || index >= _children.Count)
			{
				throw new ArgumentOutOfRangeException();
			}

			return _children[index];
		}


		// Capture the mouse event and hit test the coordinate point value against
		// the child visual objects.
		void MyVisualHost_MouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
		{
			// Retreive the coordinates of the mouse button event.
			Point pt = e.GetPosition((UIElement)sender);

			// Initiate the hit test by setting up a hit test result callback method.
			VisualTreeHelper.HitTest(this, null, new HitTestResultCallback(myCallback), new PointHitTestParameters(pt));
		}

		// If a child visual object is hit, toggle its opacity to visually indicate a hit.
		public HitTestResultBehavior myCallback(HitTestResult result)
		{
			if (result.VisualHit.GetType() == typeof(DrawingVisual))
			{
				if (((DrawingVisual)result.VisualHit).Opacity == 1.0)
				{
					((DrawingVisual)result.VisualHit).Opacity = 0.4;
				}
				else
				{
					((DrawingVisual)result.VisualHit).Opacity = 1.0;
				}
			}

			// Stop the hit test enumeration of objects in the visual tree.
			return HitTestResultBehavior.Stop;
		}
	}
}
