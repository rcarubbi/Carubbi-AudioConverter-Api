using System;
using System.IO;
using System.Linq;

namespace Carubbi.AudioConverter.Api.Utilities
{
    public class EnvironmentVariablesConfig
    {
        private bool _loaded;

        public void CheckAddBinPath()
        {
            if (_loaded) return;

            // find path to 'bin' folder
            var binPath = AppDomain.CurrentDomain.BaseDirectory;
            // get current search path from environment
            var path = Environment.GetEnvironmentVariable("PATH") ?? "";

            // add 'bin' folder to search path if not already present
            if (path.Split(Path.PathSeparator).Any(part => part.Equals(binPath, StringComparison.CurrentCultureIgnoreCase)))
                return;

            path = string.Join(Path.PathSeparator.ToString(), new string[] { path, binPath });
            Environment.SetEnvironmentVariable("PATH", path);
            _loaded = true;
        }
    }
}
