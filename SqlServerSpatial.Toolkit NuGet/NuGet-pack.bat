cd ..\SqlServerSpatial.Toolkit
nuget pack SqlServerSpatial.Toolkit.core.csproj -Prop Configuration=Release
rem "Remove any Microsoft.SqlServer.Types content directory from package file"
pause