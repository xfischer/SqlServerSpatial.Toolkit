# SqlServerSpatialTypes.Toolkit
Geometry trace for Sql Spatial data + debugger visualizer for SQL Server data types in Visual Studio

## What's inside ?

 - Debugger visualizer for SqlGeometry and SqlGeography types
 - Custom trace writer with colorful syntax
 - Trace viewer

### How to build

 - Grab the repo
 - Choose the solution matching your Visual Studio version (VS2013 or VS2015)
 - Build. Binaries are generated in Binaries directory

### How to install the debugger visualizer

 - Copy the following files
 
![Files](/img/visfile.png?raw=true "Files (example for VS2015)")
 - in Binaries\Release to either of the following locations: 
	 - VisualStudioInstallPath\Common7\Packages\Debugger\Visualizers
	 - My Documents\VisualStudioVersion\Visualizers
 - More information here [on MSDN](https://msdn.microsoft.com/en-us/library/sb2yca43.aspx).

## Using the toolkit [Work in progress]
### Debugger Visualizer
Hover any `SqlGeometry` or `SqlGeography` variable and click on the lens icon. The visualizer will popup and display your geometry. Use mouse to pan and zoom.

![Screen capture](/img/debugvis.png?raw=true "Screen capture")
 
### Spatial Trace

Very useful when processing geometries. **SpatialTrace** lets you track what is going on along the way.
```csharp
using SqlServerSpatialTypes.Toolkit;

// Enable tracing
SpatialTrace.Enable(); 
// Trace sample geometry instance. 
// Works with SqlGeometry, SqlGeography and IEnumerable<> of those
SpatialTrace.TraceGeometry(geometry, "Sample geometry with default style");

// Change styling
SpatialTrace.SetLineWidth(3); // Current stroke style is 3px wide
SpatialTrace.SetFillColor(Color.FromArgb(128, 255, 0, 0)); // Fills with red

// Style is applied to subsequent traces 
SpatialTrace.TraceGeometry(geometry, "Some text");

// Reset style
SpatialTrace.ResetStyle();
```
This will generate a SpatialTrace.txt file in running assembly directory.
You can directly view this trace by calling
```csharp
SpatialTrace.ShowDialog();
```
### Trace Viewer

Open the viewer. Drag the file on it like a ninja, and there it goes :

 ![Viewer](/img/traceviewer.png?raw=true "Trace Viewer")


