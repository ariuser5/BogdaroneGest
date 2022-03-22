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

		//static string m_output = string.Empty;

		/// <summary>
		/// args[0] - master process id
		/// args[1] - quit command for connection process
		/// args[2] - refresh rate(between 0 and 1000)
		/// </summary>
		/// <param name="args"></param>
		static void Main(string[] args)
		{
			Console.WriteLine($"Inside {nameof(DbService)}");

			var masterProcessId = int.Parse(args[0]);
			var quitCommand = ParseQuitCommandArg(args);
			var refreshRate = ParseRefreshRateArg(args);

			var master = Process.GetProcessById(masterProcessId);

			var instance = new Program(
				master: master, 
				quitCommand: quitCommand,
				refreshRate: refreshRate);

			instance.Run();
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




		//readonly MemoryStream _;


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
			var startInfo = new ProcessStartInfo()
			{
				FileName = "SqlDbQuery.exe",
				Arguments = "",
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


			this.StartAndSyncConnection(process);

			return process;
		}

		void StartAndSyncConnection(Process process)
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

				if(e.Data == "ready") {
					isInit = true;
				}
			}
		}

		void Talk(Process connection)
		{
			var cmd = (string)null;
			var readCommandTask = new Task(ReadCommand);

			while(this._master.HasExited == false) {

				switch(readCommandTask.Status) {
					case TaskStatus.Created:
						var drain = this.DrainConnectionProgramOutput();

						Console.Write(drain);
						readCommandTask.Start();
						break;

					case TaskStatus.RanToCompletion:
						this.RunCommand(connection, cmd);
						readCommandTask = new Task(ReadCommand);
						break;
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
