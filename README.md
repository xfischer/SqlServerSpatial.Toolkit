# SqlServerSpatial.Toolkit [![Build status](https://ci.appveyor.com/api/projects/status/github/xfischer/SqlServerSpatial.Toolkit)](https://ci.appveyor.com/project/xfischer/SqlServerSpatial-Toolkit)
Geometry trace for Sql Spatial data + debugger visualizer for SQL Server data types in Visual Studio

 ![Viewer](/img/traceviewer.png?raw=true "Trace Viewer")

 - Debugger visualizer for SqlGeometry and SqlGeography types
 - Extensions methods
 - Custom trace writer with colorful syntax
 - Trace viewer

## Install
Via NuGet: `Install-Package SqlServerSpatial.Toolkit`

## Using the toolkit
### Spatial Trace

Very useful when processing geometries. **SpatialTrace** lets you track what is going on along the way.

**Important: Trace will actually write only if a debugger is attached.**
This is by design, to avoid tracing in a production environment.

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

Open the viewer. Drag the file on it like a ninja and drop it like a samuraï, and the trace viewer will show what you haved logged through the SpatialTrace.Trace...

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

### Extension methods

#### 1. Point enumerator
Enumerate points in a more natural manner. Here's a look of how it is without the toolkit:
```csharp
for (int i = 1; i<=geom.STNumPoints(); i++)
{
	SqlGeometry point = geom.STPointN(i);
	// ... do something with point
}
```
Now, with the toolkit, you can iterate point with a *foreach* syntax:
```csharp
foreach(SqlGeometry point in geom.Points())
{
	// ... do something with point
}
```
#### 2. Geometry parts enumerator
Enumerate parts of a geometry. Here's a look of how it is without the toolkit:
```csharp
for (int i = 1; i <= geom.STNumGeometries(); i++)
{
	SqlGeometry geometryPart = geom.STGeometryN(i);
	// ... do something with geometryPart
}
```
Now, with the toolkit:
```csharp
foreach (SqlGeometry geometryPart in geom.Geometries())
{
	// ... do something with geometryPart
}
```
#### 3. Polygon helpers
You can handle polygon interior rings easily:
```csharp
bool hasInteriorRings = polygon.HasInteriorRings();
foreach(SqlGeometry ring in geom.InteriorRings())
{
	// ... do something with ring
}
```
#### 4. `MakeValidIfInvalid()` helper
`MakeValid()` can create strange artefacts and awkward geometries. The `MakeValidIfInvalid()` method will help you.
It takes two parameters:
- **retainDimension** : guarantees that every geometry returned will be at least of the specified dimension. For example, `MakeValid()` or `Reduce()` sometimes returns a geometry collection object with lines, points and polygons. Calling `MakeValidIfInvalid(2)` will guarantee that lines and points are removed.
- **minimumRatio** : guarantees that every geometry under this ratio will not be returned. For example, if you have a geometry collection with a 10000m² polygon and 0.5m² negligible polygon, you can call `MakeValidIfInvalid(2, 0.00001)` and this polygon will be removed.

#### 5. Serialization helpers
You can save and load SqlGeometry from disk:
```csharp
// Loads SqlGeometry from disk
SqlGeometry geom = SqlTypesExtensions.Read("file.bin");

// Save SqlGeometry to disk
geom.Save("file.bin");

// Loads a list of SqlGeometry from disk
List<SqlGeometry> geometries = SqlTypesExtensions.ReadList("file.bin");

// Saves a list of SqlGeometry to disk
geometries.Save("file.bin");
```
## How to build the toolkit from source

 - Grab the repo
 - Choose the solution matching your Visual Studio version (VS2013 or VS2015)
 - Restore NuGet packages (Microsoft.SqlServer.Types and DotSpatial.Projections)
 - Build. Binaries are generated in Binaries directory
