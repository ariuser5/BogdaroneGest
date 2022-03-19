using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using DBEntities;

namespace BogdaroneAccess
{
	public class UsersDao : IUserDao
	{

		public static string PingService()
		{
			using(var pipeServer = new AnonymousPipeServerStream(
				direction: PipeDirection.In,
				inheritability: HandleInheritability.Inheritable))
			{
				Console.WriteLine(
					format: "[SERVER] Current TransmissionMode: {0}.",
					arg0: pipeServer.TransmissionMode);

				var result = ReadClientProcessOutout(pipeServer);

				return result;
			}
		}

		static string ReadClientProcessOutout(AnonymousPipeServerStream pipeServer)
		{
			var handle = pipeServer.GetClientHandleAsString();

			var startInfo = new ProcessStartInfo()
			{
				FileName = "BogdaroneDBService.exe",
				Arguments = handle,
				UseShellExecute = false,
				RedirectStandardOutput = false,
				CreateNoWindow = false
			};

			var process = new Process()
			{
				StartInfo = startInfo,
				EnableRaisingEvents = true
			};

			//var output = string.Empty;

			//process.Exited += (s, e) => output = process.StandardOutput.ReadToEnd();

			process.Start();

			pipeServer.DisposeLocalCopyOfClientHandle();
			process.WaitForExit();

			try {
				using(var sr = new StreamReader(pipeServer)) {
					var output = sr.ReadToEnd();
					
					return output;
				}	 
			} catch(Exception ex) {
				throw;
			} finally {
				process.Close();
			}
		}



		public void CreateUser(User user)
		{
			throw new NotImplementedException();
		}

		public bool DeleteUser(string userName)
		{
			throw new NotImplementedException();
		}

		public User GetUser(string userName)
		{
			throw new NotImplementedException();
		}

		public IEnumerable<User> GetUsers()
		{
			throw new NotImplementedException();
		}

		public void UpdateUser(User user)
		{
			throw new NotImplementedException();
		}
	}
}
