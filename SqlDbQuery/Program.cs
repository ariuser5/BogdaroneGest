using System;
using System.Configuration;
using System.Data.SqlClient;
using Newtonsoft.Json.Linq;

namespace SqlDbQuery
{
	class Program
	{
		static void Main(string[] args)
		{
			//System.Diagnostics.Debugger.Launch();
			Console.WriteLine($"Inside {nameof(SqlDbQuery)}");
			Console.WriteLine("ready");

			using(var connection = GetConnection()) {
				var program = new Program(connection);

				program.Execute();
			}
		}

		static SqlConnection GetConnection()
		{
			var connectionString = ConfigurationManager
				.ConnectionStrings["DefaultConnection"]
				.ToString();

			var connection = new SqlConnection(connectionString);

			connection.Open();

			return connection;
		}




		//readonly string _dbConnectionString;
		//readonly string _pipeHandle;

		//public Program(string dbConnectionString, string pipeHandle)
		//{
		//	this._dbConnectionString = dbConnectionString;
		//	this._pipeHandle = pipeHandle;
		//}


		readonly SqlConnection _connection;

		public Program(SqlConnection connection)
		{
			this._connection = connection;
		}


		public void Execute()
		{
			while(true) {
				var cmd = Console.ReadLine();

				if(cmd.ToLower() == "quit") {
					break;
				} else {
					this.ExecuteCommand(cmd);
				}
			}

		}


		void ExecuteCommand(string cmd)
		{
			var result = this.RunQuery(cmd);

			System.IO.File.WriteAllText("result.txt", result);
			Console.WriteLine(result);
		}

		string RunQuery(string cmd)
		{
			try {
				using(var command = new SqlCommand(cmd, this._connection))
				using(var reader = command.ExecuteReader()) {
					var result = this.ReadAsJson(reader);
					return result;
				}
			} catch(Exception ex) {
				var msg = "Failed to run query";

				Console.WriteLine(msg);
				Console.WriteLine(ex.Message);
				System.IO.File.WriteAllText("result.txt", ex.ToString() + Environment.NewLine + ex.Message);
				return null;
			}
		}

		string ReadAsJson(SqlDataReader reader)
		{
			var jArr = new JArray();

			while(reader.Read()) {
				var jObj = new JObject();

				for(var i = 0; i < reader.FieldCount; i++) {
					var fieldName = reader.GetName(i);
					var jProp = new JProperty(
						name: fieldName,
						content: reader[fieldName]);

					jObj.Add(jProp);
				}

				jArr.Add(jObj);
			}

			return jArr.ToString(Newtonsoft.Json.Formatting.None);
		}

		//void WriteToPipe(string value)
		//{
		//	using(var pipeClient = new AnonymousPipeClientStream(
		//		direction: PipeDirection.Out,
		//		pipeHandleAsString: this._pipeHandle)) 
		//	{
		//		Console.WriteLine(
		//			format: "[CLIENT] Current TransmissionMode: {0}.",
		//			arg0: pipeClient.TransmissionMode);

		//		using(var sw = new StreamWriter(pipeClient)) {
		//			sw.AutoFlush = true;
		//			sw.WriteLine(value);
		//		}
		//	}
		//}


	}
}
