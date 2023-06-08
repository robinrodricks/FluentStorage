namespace NetBox {
    using System;
    using System.IO;

	/// <summary>
	/// Represents a temporary file that is deleted on dispose. The files are created in user's temp directory.
	/// </summary>
	public class TempFile : IDisposable {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="ext">Optional extension, defaults to .tmp</param>
        public TempFile(string? ext = null) {
            if(ext == null)
                ext = ".tmp";

            if(!ext.StartsWith("."))
                ext = "." + ext;

            string name = Guid.NewGuid().ToString() + ext;

            FullPath = Path.Combine(Path.GetTempPath(), name);
        }

        /// <summary>
        /// Full path to the temp file. It's not created by this class.
        /// </summary>
        public string FullPath { get; }

        /// <summary>
        /// Implicit conversion to string (full path).
        /// </summary>
        /// <param name="tf"></param>
        public static implicit operator string(TempFile tf) => tf.FullPath;

        /// <summary>
        /// Returns full path value
        /// </summary>
        /// <returns></returns>
        public override string ToString() => FullPath;

        public void Dispose() {
            if(File.Exists(FullPath)) {
                File.Delete(FullPath);
            }
        }
    }
}