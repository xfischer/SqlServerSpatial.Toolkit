using System;
using System.Collections.Generic;
using System.Drawing;
using GeoAPI.Geometries;

namespace NetTopologySuite.Diagnostics.Tracing
{
    internal interface ISpatialTrace : IDisposable
    {
        void Indent(string groupName = null);
        void TraceGeometry(IGeometry geom, string message, string label, string memberName, string sourceFilePath, int sourceLineNumber);
        void TraceGeometry(IEnumerable<IGeometry> geom, string message, string label, string memberName, string sourceFilePath, int sourceLineNumber);
        void TraceText(string message, string memberName, string sourceFilePath, int sourceLineNumber);
        void SetFillColor(Color color);
        void SetLineColor(Color color);
        void SetLineWidth(float width);
        void ResetStyle();
        void Unindent();
        string TraceFilePath { get; }
        void Clear();
    }
}
