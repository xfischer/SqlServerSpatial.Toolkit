# SqlServerSpatial.Toolkit
Geometry trace for Sql Spatial data + debugger visualizer for SQL Server data types in Visual Studio

 - Debugger visualizer for SqlGeometry and SqlGeography types
 - Custom trace writer with colorful syntax
 - Trace viewer

## Install
Via NuGet
```csharp
PM> Install-Package GeoJSON.Net
```

### How to build

 - Grab the repo
 - Choose the solution matching your Visual Studio version (VS2013 or VS2015)
 - Build. Binaries are generated in Binaries directory

## Using the toolkit
### Spatial Trace

Very useful when processing geometries. **SpatialTrace** lets you track what is going on along the way.
```csharp
using SqlServerSpatial.Toolkit;

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

Open the viewer. Drag the file on it like a ninja and drop it like a samuraï, and there it goes :

 ![Viewer](/img/traceviewer.png?raw=true "Trace Viewer")

### Debugger Visualizer
#### Installation

 - Copy the following files
 	- SqlServerSpatial.Toolkit.dll
 	- SqlServerSpatial.Toolkit.DebuggerVisualizer.VS2013.dll
 	- Microsoft.SqlServer.Types.dll and the SqlServerTypes directory
 - in Binaries\Release to either of the following locations: 
	 - VisualStudioInstallPath\Common7\Packages\Debugger\Visualizers
	 - My Documents\VisualStudioVersion\Visualizers
 - Restart your debugging session
 - More information here [on MSDN](https://msdn.microsoft.com/en-us/library/sb2yca43.aspx).

#### Usage

Hover any `SqlGeometry` or `SqlGeography` variable and click on the lens icon. The visualizer will popup and display your geometry. Use mouse to pan and zoom.

![Screen capture](/img/debugvis.png?raw=true "Screen capture")
 

