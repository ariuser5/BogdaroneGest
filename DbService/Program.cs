/* This assembly is meant to execute as a bridge between a master process, which represents 
 * the requester, and another process which does the database querying and responds to requests. 
 * The purpose of it is to ensure that when the master process is terminated, all the connections,
 * opened streams or unmanaged code exections are terminated safely. To achieve this, the entry point 
 * receives the master process id as argument which is used to constantly monitor if the process
 * is still active. If the master process is terminated, then a safe shutdown command is passed
 * to the responder process and proper clean-up is ensured before full termination.
 * This process is designed to be started as child process.
 */

using System;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;

namespace DbService
{
	class Program
	{

		const string DEFAULT_QUIT_COMMAND = "quit";
		const int MAX_WAIT_TIME = 5000;


		/// <summary>
		/// args[0] - master process id
		/// args[1] - quit command for connection process
		/// args[2] - refresh rate(between 0 and 1000)
		/// </summary>
		/// <param name="args"></param>
		static void Main(string[] args)
		{
			Console.WriteLine($"Inside {nameof(DbService)}");

			var masterProcessId = ParseProcessId(args);
			var quitCommand = ParseQuitCommandArg(args);
			var refreshRate = ParseRefreshRateArg(args);

			var master = Process.GetProcessById(masterProcessId);

			var instance = new Program(
				master: master, 
				quitCommand: quitCommand,
				refreshRate: refreshRate);

			instance.Run();
		}

		static int ParseProcessId(string[] args)
		{
			if(args.Length >= 1) {
				var pid = int.Parse(args[0]);

				return pid;
			}

			return Process.GetCurrentProcess().Id;
		}

		static string ParseQuitCommandArg(string[] args)
		{
			if(args.Length >= 2) {
				return args[1];
			}

			return DEFAULT_QUIT_COMMAND;
		}

		static RefreshRate ParseRefreshRateArg(string[] args)
		{
			if(args.Length >= 3) {
				var value = RefreshRate.Parse(args[2]);

				return value;
			}

			return new RefreshRate();
		}



		readonly StringBuilder _buffer;

		readonly Process _master;
		readonly string _quitCommand;
		readonly RefreshRate _refreshRate;

		public Program(
			Process master, 
			string quitCommand,
			RefreshRate refreshRate)
		{
			this._master = master;
			this._quitCommand = quitCommand;
			this._refreshRate = refreshRate;

			this._buffer = new StringBuilder();
		}



		void Run()
		{
			using(var connection = this.StartSqlConnection()) {
				this.Talk(connection);
				this.CloseConnection(connection);
			}
		}

		Process StartSqlConnection()
		{
			const string SYNC_FLAG = "READY";

			var startInfo = new ProcessStartInfo()
			{
				FileName = "SqlDbQuery.exe",
				Arguments = SYNC_FLAG,
				UseShellExecute = false,
				RedirectStandardInput = true,
				RedirectStandardOutput = true,
				CreateNoWindow = false
			};

			var process = new Process()
			{
				StartInfo = startInfo,
				EnableRaisingEvents = true
			};


			this.StartAndSyncConnection(process, SYNC_FLAG);

			return process;
		}

		void StartAndSyncConnection(Process process, string syncFlag)
		{
			var isInit = false;
			var waiter = Task.Run(WaitForSyncMessage);

			process.OutputDataReceived += ReadOutputAsync;

			process.Start();
			process.BeginOutputReadLine();

			waiter.Wait(MAX_WAIT_TIME);
			process.OutputDataReceived -= ReadOutputAsync;

			if(isInit == false) {
				throw new ApplicationException(
					"Could not initialize sql connection process");
			}

			void WaitForSyncMessage()
			{ while(isInit == false) { /* wait */ } }

			void ReadOutputAsync(object sender, DataReceivedEventArgs e)
			{
				this._buffer.AppendLine(e.Data);

				if(e.Data == syncFlag) {
					isInit = true;
				}
			}
		}

		void Talk(Process connection)
		{
			var cmd = (string)null;
			var readCommandTask = new Task(ReadCommand);

			while(this._master.HasExited == false) {

				if(readCommandTask.Status == TaskStatus.RanToCompletion) {
					
					this.RunCommand(connection, cmd);

					if(connection.HasExited) {
						Console.WriteLine("Sql connection is closed. Exiting...");
						break;
					} else {
						readCommandTask = new Task(ReadCommand);
						cmd = null;
					}

				} else if(readCommandTask.Status == TaskStatus.Created) {
					var drain = this.DrainConnectionProgramOutput();

					Console.Write(drain);
					readCommandTask.Start();
				}

				System.Threading.Thread.Sleep(this._refreshRate);
			}

			void ReadCommand() { cmd = Console.ReadLine(); }
		}

		void RunCommand(Process connection, string command)
		{
			var isResponded = false;
			var response = string.Empty;
			var waiter = Task.Run(WaitForResponse);

			connection.OutputDataReceived += ReadResponseAsync;
			connection.StandardInput.WriteLine(command);

			waiter.Wait(MAX_WAIT_TIME);
			connection.OutputDataReceived -= ReadResponseAsync;

			if(isResponded) {
				Console.WriteLine(response);
			} else {
				throw new TimeoutException(
					"SqlConnection failed to respond to request");
			}
			
			void WaitForResponse()
			{ while(isResponded == false) { /* wait */ } }

			void ReadResponseAsync(object sender, DataReceivedEventArgs e)
			{
				isResponded = true;
				response = e.Data;
			}
		}

		string DrainConnectionProgramOutput()
		{
			var output = this._buffer.ToString();
			this._buffer.Clear();
			return output;
		}

		void CloseConnection(Process connection)
		{
			connection.StandardInput.WriteLine(this._quitCommand);
			connection.WaitForExit();
		}

	}
}
