using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace BogdaroneGest.Utilities
{
	static class Utils
	{

		public static string HashPassword(string password)
		{
			var sha1 = new SHA1CryptoServiceProvider();

			var pwBytes = Encoding.ASCII.GetBytes(password);
			var encryptedBytes = sha1.ComputeHash(pwBytes);

			return Convert.ToBase64String(encryptedBytes);
		}


	}
}
