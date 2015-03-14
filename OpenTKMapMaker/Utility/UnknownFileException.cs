using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace OpenTKMapMaker.Utility
{
    /// <summary>
    /// Wraps a System.IO.FileNotFoundException.
    /// </summary>
    class UnknownFileException : FileNotFoundException
    {
        /// <summary>
        /// Constructs an UnknownFileException.
        /// </summary>
        /// <param name="filename">The name of the unknown file</param>
        public UnknownFileException(string filename)
            : base("file not found", filename)
        {
        }
    }
}
