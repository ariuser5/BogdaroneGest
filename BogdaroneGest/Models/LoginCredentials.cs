using System;
using System.IO;
using System.Reflection;
using System.Windows;
using System.Xml;
using System.Xml.Serialization;

namespace BogdaroneGest.Models
{
	public class LoginCredentials
	{

		static string m_appLocalCacheDir;
		static string m_credentialsLocalFile;

		static LoginCredentials()
		{
			var myDocsDir = Environment
				.GetFolderPath(Environment.SpecialFolder.MyDocuments);

			var appName = Assembly.GetExecutingAssembly().GetName().Name;

			m_appLocalCacheDir = Path.Combine(myDocsDir, appName);
			m_credentialsLocalFile = Path.Combine(
				path1: m_appLocalCacheDir,
				path2: nameof(LoginCredentials) + ".xml");
		}


		public static bool IsRememberedLocally()
		{
			return File.Exists(m_credentialsLocalFile);
		}

		public static LoginCredentials ImportRemembered()
		{
			var serializer = new XmlSerializer(typeof(LoginCredentials));
			LoginCredentials instance;

			using(var sr = new StreamReader(m_credentialsLocalFile))
			using(var xmlReader = XmlReader.Create(sr)) {
				instance = (LoginCredentials)serializer.Deserialize(xmlReader);
			}

			return instance;
		}

		public static void ForgetLocallyStoredCredentials()
		{
			File.Delete(m_credentialsLocalFile);
		}


		public static bool TryCreateFrom(
			LoginWindow window,
			out LoginCredentials credentials)
		{
			if(IsValidUserName(window.field_userName.Text)) {
				if(IsValidPassword(window.field_password.Text)) {

					credentials = new LoginCredentials()
					{
						UserName = window.field_userName.Text,
						Password = window.field_password.Text,
					};

					return true;

				} else {
					MessageBox.Show(
						$"Empty '{LoginWindow.PASSWORD_FIELD_NAME}' field. " +
						$"Please fill in the " +
						$"'{LoginWindow.PASSWORD_FIELD_NAME}' field.");
				}
			} else {
				MessageBox.Show(
					$"Empty '{LoginWindow.USERNAME_FIELD_NAME}' field. " +
					$"Please fill in the " +
					$"'{LoginWindow.USERNAME_FIELD_NAME}' field.");
			}

			credentials = null;
			return false;
		}


		static bool IsValidUserName(string userName)
		{
			return
				!string.IsNullOrWhiteSpace(userName) &&
				!string.Equals(userName, LoginWindow.USERNAME_FIELD_NAME);
		}

		static bool IsValidPassword(string password)
		{
			return
				!string.IsNullOrWhiteSpace(password) &&
				!string.Equals(password, LoginWindow.PASSWORD_FIELD_NAME);
		}



		public string UserName { get; set; }
		public string Password { get; set; }


		public void StoreLocally()
		{
			var serializer = new XmlSerializer(this.GetType());
			var namespaces = new[] { new XmlQualifiedName("", "") };
			var xmlSerializerNs = new XmlSerializerNamespaces(namespaces);
			var writerSettings = new XmlWriterSettings() { Indent = true };

			if(!Directory.Exists(m_appLocalCacheDir)) {
				Directory.CreateDirectory(m_appLocalCacheDir);
			}

			using(var sw = new StreamWriter(m_credentialsLocalFile))
			using(var xmlWriter = XmlWriter.Create(sw, writerSettings)) {
				serializer.Serialize(xmlWriter, this, xmlSerializerNs);
			}
		}


	}
}
