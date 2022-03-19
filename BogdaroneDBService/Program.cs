using System;
using System.Configuration;
using System.Data.SqlClient;
using System.IO;
using System.IO.Pipes;
using System.Text;
using Newtonsoft.Json.Linq;

namespace BogdaroneDBService
{
	class Program
	{
		static void Main(string[] args)
		{
			System.Diagnostics.Debugger.Launch();

			var connectionString = ConfigurationManager
				.ConnectionStrings["DefaultConnection"]
				.ToString();

			var program = new Program(args[0], connectionString);

			program.Execute();
		}




		readonly string _pipeHandle;
		readonly string _dbConnectionString;

		public Program(string pipeHandle, string dbConnectionString)
		{
			this._pipeHandle = pipeHandle;
			this._dbConnectionString = dbConnectionString;
		}


		public void Execute()
		{
			using(var connection = new SqlConnection(this._dbConnectionString)) {
				connection.Open();
				this.ExecuteQuery(connection);
			}
		}

		void ExecuteQuery(SqlConnection connection)
		{
			try {
				var command = new SqlCommand("SELECT * FROM dbo.Users", connection);
				var reader = command.ExecuteReader();
				var result = this.ReadAsJson(reader);

				this.WriteToPipe(result);
			} catch(Exception ex) {
				Console.WriteLine(ex);
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

			return jArr.ToString(Newtonsoft.Json.Formatting.Indented);
		}

		void WriteToPipe(string value)
		{
			using(var pipeClient = new AnonymousPipeClientStream(
				direction: PipeDirection.Out,
				pipeHandleAsString: this._pipeHandle)) 
			{
				Console.WriteLine(
					format: "[CLIENT] Current TransmissionMode: {0}.",
				   arg0: pipeClient.TransmissionMode);

				using(var sw = new StreamWriter(pipeClient)) {
					sw.AutoFlush = true;
					sw.WriteLine(value);
				}
			}
		}


	}
}
