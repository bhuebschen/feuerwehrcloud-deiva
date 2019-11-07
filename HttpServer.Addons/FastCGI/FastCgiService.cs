
namespace HttpServer.Addons.FastCgi
{
    using System;
    using System.Diagnostics;
    using System.IO;
    using HttpServer.Addons.CGI;

    /// <summary>
    /// TODO: Update summary.
    /// </summary>
    /// <remarks>
    /// FastCGI specification:
    /// http://www.fastcgi.com/drupal/node/6?q=node/22
    /// </remarks>
    public class FastCgiService : ICgiService
    {
        string _application;
        string _commandline;
        string _extension;

        Process _process;
        int _port;

        string _response;

        int _timeout;
        bool _hasTimedOut;

        FastCgiHandler _handler;

        private int _id = 1;

        public FastCgiService(string application, string extension, string commandline = "-b {port}", int port = 8129)
        {
            if (application == null) throw new ArgumentNullException("application");

            _application = application;
            _extension = extension;
            _commandline = commandline;
            _port = port;
        }

        ~FastCgiService()
        {
            // This won't work if the application crashes. Use FastCgiHelper.CheckProcessExists
            // to check if a FastCGI process is running at a given port.
            this.Stop();
        }

        public string CommandLine
        {
            get { return _commandline; }
            set { _commandline = value; }
        }

        public int Port
        {
            get { return _port; }
            set { _port = value; }
        }

        /// <summary>
        /// Get the path to the CGI executable.
        /// </summary>
        public string Application
        {
            get { return _application; }
        }

        /// <summary>
        /// Get the file extension the CGI service handles.
        /// </summary>
        public string Extension
        {
            get { return _extension; }
        }

        public string StandardOutput
        {
            get { return _response; }
        }

        public string StandardError
        {
            get { throw new NotImplementedException(); }
        }

        public bool HasError
        {
            get { return _handler == null ? false : _handler.HasError; }
        }

        public int TimeOut
        {
            get { return _timeout; }
            set { _timeout = value; }
        }

        public bool HasTimedOut
        {
            get { return _hasTimedOut; }
        }

        public int Execute(IHttpContext context, string path, string scriptName)
        {
            if (_process == null)
            {
                // TODO: try catch
                CreateProcess();
            }

            var request = context.Request;
            var env = CgiHelper.GetEnvironmentVariables(context, path, scriptName);

            var status = _handler.ProcessRequest(GetRequestId(), request, env);

            using (var reader = new StreamReader(_handler.StandardOutput, CgiHelper.Encoding))
            {
                _response = reader.ReadToEnd();
            }

            // TODO: Read error / protocol status

            return status;
        }

        public void Stop()
        {
            if (_process != null && !_process.HasExited)
            {
                try
                {
                    _process.Kill();
                }
                catch (Exception)
                { }

                _process.Close();
                _process = null;
            }
        }

        private void CreateProcess()
        {
            string commandline = _commandline.Replace("{port}", _port.ToString());

            // Create the child process.
            _process = new Process();

            _process.StartInfo.UseShellExecute = false;
            _process.StartInfo.CreateNoWindow = true;
            _process.StartInfo.FileName = _application;
            _process.StartInfo.Arguments = commandline;

            _process.Start();

            // Create handler to connect to FastCGI process
            _handler = new FastCgiHandler(_port, _timeout);
        }

        /// <summary>
        /// Obtains and reserves a request id.
        /// </summary>
        /// <returns>Request id.</returns>
        private int GetRequestId()
        {
            // TODO: need to lock?
            return (this._id++ % 10);
        }
    }
}
