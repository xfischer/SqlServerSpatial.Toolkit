# SqlServerSpatialTypes.Toolkit
Visual Trace and debugger visualizer for SQL Server types in Visual Studio

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
Hover any `SqlGeometry`,`IEnumerable<SqlGeometry>`, `SqlGeography` or `IEnumerable<SqlGeography>` variable and click on the lens icon. The visualizer will popup and display your geometry. Use mouse to pan and zoom.

![Screen capture](/img/debugvis.png?raw=true "Screen capture")
 
### Trace [work in progress]

 ![Trace](/img/trace.png?raw=true "Trace")


### Trace Viewer  [work in progress]

 ![Viewer](/img/traceviewer.png?raw=true "Trace Viewer")


