using System;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Threading;
using System.Threading.Tasks;

namespace BogdaroneAccess
{

	class DbService : IDisposable
	{

		static readonly DbService m_instance;
		static readonly Task m_serviceTask;

		static DbService()
		{
			var thisProcess = Process.GetCurrentProcess();

			var pipeStream = new AnonymousPipeServerStream(
				direction: PipeDirection.InOut,
				inheritability: HandleInheritability.Inheritable);

			var serviceProcess = StartDatabaseService(pipeStream);

			var superviserProcess = StartSupervisorService(
				thisProcess: thisProcess, 
				serviceProcess: serviceProcess);

			m_instance = new DbService(thisProcess, serviceProcess, pipeStream);
			m_serviceTask = Task.Run(m_instance.Execute);
		}

		public static DbService Instance => m_instance;


		private static Process StartDatabaseService(
			AnonymousPipeServerStream pipeServer)
		{
			var handle = pipeServer.GetClientHandleAsString();

			var startInfo = new ProcessStartInfo()
			{
				FileName = "BogdaroneDBService.exe",
				Arguments = handle,
				UseShellExecute = false,
				RedirectStandardInput = true,
				RedirectStandardOutput = false,
				CreateNoWindow = false
			};

			var process = new Process() { StartInfo = startInfo };

			process.Start();

			pipeServer.DisposeLocalCopyOfClientHandle();
			process.WaitForExit();

			try {
				using(var sr = new StreamReader(pipeServer)) {
					var output = sr.ReadToEnd();

					return output;
				}
			} catch(Exception ex) {
				// Do nothing
			} finally {
				process.Close();
			}
		}

		private static Process StartSupervisorService(
			Process thisProcess,
			Process serviceProcess)
		{
			var args = string.Format("{0} {1} {2}", 
				arg0: thisProcess.Id,
				arg1: serviceProcess.Id,
				arg2: "exit");

			var processStartInfo = new ProcessStartInfo()
			{
				FileName = "DbServiceSupervisor.exe",
				Arguments = args,
				UseShellExecute = false,
				RedirectStandardInput = false,
				RedirectStandardOutput = false,
				CreateNoWindow = true
			};

			var process = new Process()
			{
				StartInfo = processStartInfo,
				EnableRaisingEvents = false
			};

			process.Start();

			return process;
		}



		internal static string RunQuery(string query)
		{

		}


		readonly int _masterProcessId;
		readonly string _masterProcessName;
		readonly Process _service;
		readonly AnonymousPipeServerStream _pipeStream;

		private DbService(
			Process masterProcess,
			Process serviceProcess,
			AnonymousPipeServerStream pipeStream)
		{
			this._masterProcessId = masterProcess.Id;
			this._masterProcessName = masterProcess.ProcessName;
			this._service = serviceProcess;
			this._pipeStream = pipeStream;
		}


		private void Execute()
		{
			try {
				this.ExecuteRoutine();
			} catch {
				// Do nothing.
				// Mandatory statement to catch exception before executing finally block.
			} finally {

				using(var sw = new StreamWriter(this._pipeStream, leaveOpen: true)) {
					sw.WriteLine("exit");
				}

				this._pipeStream.Dispose();
				this._service.Dispose();
			}
		}

		private void ExecuteRoutine()
		{

		}


		public void Dispose()
		{

		}

	}
}
