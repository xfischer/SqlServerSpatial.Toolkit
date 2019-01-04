using System;
using System.Collections.Generic;
using System.Configuration;
using System.Drawing;
using System.IO;
using System.Runtime.CompilerServices;
using GeoAPI.Geometries;

namespace NetTopologySuite.Diagnostics.Tracing
{
    public static class SpatialTrace
    {
        private static ISpatialTrace _trace; // trace singleton
        private static ISpatialTrace _dummyTrace;
        private static bool _isEnabled;
        private static string _outputBaseDirectory = null;
        private const string TRACE_DATA_DIR = "SpatialTraceData";
        private const string TRACE_DATA_FILE = "SpatialTrace.txt";

        private static ISpatialTrace Current
        {
            get
            {
                if (_isEnabled)
                {
                    if (_trace == null)
                    {
                        if (string.IsNullOrWhiteSpace(_outputBaseDirectory))
                        {
                            _outputBaseDirectory = AppDomain.CurrentDomain.BaseDirectory;
                        }
                        if (Path.IsPathRooted(_outputBaseDirectory) == false)
                        {
                            _outputBaseDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, _outputBaseDirectory);
                        }
                        _trace = new SpatialTraceInternal(_outputBaseDirectory);
                    }

                    return _trace;
                }
                else
                {
                    return _dummyTrace;
                }
            }
        }

        static SpatialTrace()
        {
            try
            {
                _dummyTrace = new DummySpatialTrace();

                _isEnabled = false;
                //Boolean.TryParse(ConfigurationManager.AppSettings["EnableSpatialTrace"], out _isEnabled);
                _outputBaseDirectory = AppDomain.CurrentDomain.BaseDirectory;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public static void TraceGeometry(IGeometry geom, string message, string label = null, [CallerMemberName] string memberName = "", [CallerFilePath] string sourceFilePath = "", [CallerLineNumber] int sourceLineNumber = 0)
        {
            SpatialTrace.Current.TraceGeometry(geom, message, label, memberName, sourceFilePath, sourceLineNumber);
        }
        public static void TraceGeometry(IEnumerable<IGeometry> geomList, string message, string label = null, [CallerMemberName] string memberName = "", [CallerFilePath] string sourceFilePath = "", [CallerLineNumber] int sourceLineNumber = 0)
        {
            SpatialTrace.Current.TraceGeometry(geomList, message, label, memberName, sourceFilePath, sourceLineNumber);
        }
        public static void TraceText(string message, [CallerMemberName] string memberName = "", [CallerFilePath] string sourceFilePath = "", [CallerLineNumber] int sourceLineNumber = 0)
        {
            SpatialTrace.Current.TraceText(message, memberName, sourceFilePath, sourceLineNumber);
        }
        public static void SetFillColor(Color color)
        {
            SpatialTrace.Current.SetFillColor(color);
        }
        public static void SetLineColor(Color color)
        {
            SpatialTrace.Current.SetLineColor(color);
        }
        public static void SetLineWidth(float width)
        {
            SpatialTrace.Current.SetLineWidth(width);
        }
        public static void ResetStyle()
        {
            SpatialTrace.Current.ResetStyle();
        }

        public static void Indent(string groupName = null)
        {
            SpatialTrace.Current.Indent(groupName);
        }

        public static void Unindent()
        {
            SpatialTrace.Current.Unindent();
        }

        //[Conditional("DEBUG")]
        public static void Enable()
        {
            _isEnabled = IsTracingAllowed;
        }

        private static bool IsTracingAllowed
        {
            get
            {
                bool v_isAllowed = false;
                bool enabledInConfig = false;
                Boolean.TryParse(ConfigurationManager.AppSettings["EnableSpatialTrace"], out enabledInConfig);

                if (System.Diagnostics.Debugger.IsAttached)
                {
                    v_isAllowed = true;
                }
                else
                {
                    v_isAllowed = enabledInConfig;
                }

                return v_isAllowed;
            }
        }

        public static void Disable()
        {
            _isEnabled = false;
        }

        public static void Clear()
        {
            SpatialTrace.Current.Clear();
        }

        /// <summary>
        /// Displays the trace viewer with the current trace opened.
        /// Warning: do not use this in production code
        /// 
        /// Important : the _isEnabled is not useful there because user can call Disable()
        /// Better to recheck debugger attachment or enablement in AppSettings
        /// 
        /// </summary>
        //[Conditional("DEBUG")]
        //public static void ShowDialog()
        //{

        //    if (!IsTracingAllowed)
        //        return;

        //    using (FrmTraceViewer frm = new FrmTraceViewer())
        //    {
        //        frm.Initialize(SpatialTrace.TraceFilePath);
        //        frm.ShowDialog();
        //    }
        //}

        public static string TraceFilePath
        {
            get
            {
                if (_trace != null)
                    return _trace.TraceFilePath;
                else
                    return SpatialTrace.Current.TraceFilePath;
            }
        }

        public static string TraceDataDirectoryName
        {
            get { return TRACE_DATA_DIR; }
        }
        public static string TraceFileName
        {
            get { return TRACE_DATA_FILE; }
        }

        public static string OutputDirectory
        {
            get
            {
                return _outputBaseDirectory;
            }
            set
            {
                if (_outputBaseDirectory != value)
                {
                    _outputBaseDirectory = value;
                    if (_trace != null)
                    {
                        _trace.Dispose();
                        _trace = null;
                    }
                }
            }
        }


    }
}
