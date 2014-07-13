namespace Kayateia.Climoo {
using System;
using System.IO;

/// <summary>
/// Implements a simplistic logging mechanism that anything in CliMOO can use.
/// </summary>
public class Log {
	Log(string path) {
		_basePath = path;
		if (_basePath == null)
			_basePath = Path.Combine(Path.GetDirectoryName(typeof(Log).Assembly.Location), "Log");
		if (!Directory.Exists(_basePath))
			Directory.CreateDirectory(_basePath);

		debug( "Debug log started" );
		info( "Info log started" );
		error( "Error log started" );
	}

	/// <summary>
	/// Write out a line of logging info the debug log.
	/// </summary>
	static public void Debug( string text )
	{
		Global.debug( text );
	}

	/// <summary>
	/// Write out a line of logging info to the debug log.
	/// </summary>
	static public void Debug( string fmt, params object[] p )
	{
		Global.debug( String.Format( CultureFree.Culture, fmt, p ) );
	}

	/// <summary>
	/// Write out a line of logging info to the information log.
	/// </summary>
	static public void Info(string text) {
		Global.info(text);
	}

	/// <summary>
	/// Write out a line of logging info to the information log.
	/// </summary>
	static public void Info(string fmt, params object[] p) {
		Global.info(String.Format(CultureFree.Culture, fmt, p));
	}

	/// <summary>
	/// Write out a line of logging info to the error log.
	/// </summary>
	static public void Error(string text) {
		Global.error(text);
	}

	/// <summary>
	/// Write out a line of logging info to the error log.
	/// </summary>
	static public void Error(string fmt, params object[] p) {
		Global.error(String.Format(CultureFree.Culture, fmt, p));
	}

	public void debug( string text )
	{
		// Write out to the trace log too, so it shows up in the debugger.
		System.Diagnostics.Trace.WriteLine( "DEBUG: " + text );

		writeOut( debugPath, text );
	}

	public void info(string text) {
		// Write out to the trace log too, so it shows up in the debugger.
		System.Diagnostics.Trace.WriteLine( "INFO: " + text );

		writeOut(infoPath, text);
	}

	public void error(string text) {
		// Write out to the trace log too, so it shows up in the debugger.
		System.Diagnostics.Trace.WriteLine( "ERROR: " + text );

		writeOut(errPath, text);
	}

	void writeOut(string logFile, string text) {
		// An exception during log writing should never crash whatever the app was doing.
		// We do want to try a few times though, in case it was a transient locking issue.
		for (int i=0; i<5; ++i) {
			try {
				writeOutInner(logFile, text);
				return;
			} catch (System.Exception /*ex*/) {
				System.Threading.Thread.Sleep(10);
			}
		}
	}

	void writeOutInner(string logFile, string text) {
		using (FileStream f = File.Open(logFile, FileMode.Append))
			using (StreamWriter sw = new StreamWriter(f)) {
				sw.WriteLine("{0}: {1}", DateTimeOffset.Now, text );
			}
	}

	string debugPath
	{
		get
		{
			return Path.Combine( _basePath, "debug.log" );
		}
	}

	string infoPath {
		get {
			return Path.Combine(_basePath, "info.log");
		}
	}

	string errPath {
		get {
			return Path.Combine(_basePath, "error.log");
		}
	}

	string _basePath;

	// Create this only when it's first used, because the environment may not
	// have been set up at class load time.
	static Log Global {
		get {
			if (s_global != null)
				return s_global;

			// A better mechanism should be found here. (Web.config or whatever.)
			s_global = new Log(System.Web.Hosting.HostingEnvironment.ApplicationPhysicalPath);
			return s_global;
		}
	}

	static Log s_global;
}

}

