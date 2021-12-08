using System.Windows;
using System.Windows.Media;
using BogdaroneGest.Models;

namespace BogdaroneGest
{
	/// <summary>
	/// Interaction logic for LoginWindow.xaml
	/// </summary>
	public partial class LoginWindow: Window
	{

		internal const string USERNAME_FIELD_NAME = "User Name";
		internal const string PASSWORD_FIELD_NAME = "Password";


		public LoginWindow()
		{
			this.InitializeComponent();
			this.InitializeCustomComponents();
			this.ImportRememberedData();
		}


		void InitializeCustomComponents()
		{
			var presetUserNameFieldTextColor = this.field_userName.Foreground;
			var presetPasswordFieldTextColor = this.field_userName.Foreground;

			var presetPasswordFieldBorderBrush 
				= this.field_password.BorderBrush;


			this.field_userName.InteractiveModeSwitchHandler = () => {
				if(string.Equals(
					a: this.field_userName.Text,
					b: USERNAME_FIELD_NAME))
				{
					this.field_userName.Text = string.Empty;
					this.field_userName.Foreground = Brushes.Black;
				}
			};

			this.field_userName.FreeModeSwitchHandler = () => {
				if(string.IsNullOrWhiteSpace(this.field_userName.Text)) {
					this.field_userName.Text = USERNAME_FIELD_NAME;
					this.field_userName.Foreground = presetUserNameFieldTextColor;
				}
			};

			this.field_password.InteractiveModeSwitchHandler = () => {
				if(string.Equals(
					a: this.field_password.Text,
					b: PASSWORD_FIELD_NAME))
				{
					this.field_password.Text = string.Empty;
					this.field_password.Foreground = Brushes.Black;
				}
			};

			this.field_password.FreeModeSwitchHandler = () => {
				if(string.IsNullOrWhiteSpace(this.field_password.Text)) {
					this.field_password.Text = PASSWORD_FIELD_NAME;
					this.field_password.Foreground = presetPasswordFieldTextColor;
				}
			};

			this.field_password.ValidInputTextHandler = (text) => {
				this.field_password.BorderBrush
					= presetPasswordFieldBorderBrush;
			};

			this.field_password.InvalidInputTextHandler = (text) => {
				this.field_password.BorderBrush = Brushes.Red;
			};
		}

		void ImportRememberedData()
		{
			if(LoginCredentials.IsRememberedLocally()) {
				var remembered = LoginCredentials.ImportRemembered();

				this.field_userName.ToggleInteractiveMode(true);
				this.field_password.ToggleInteractiveMode(true);

				this.field_userName.Text = remembered.UserName;
				this.field_password.Text = remembered.Password;
				this.cbx_remember.IsChecked = true;
			}
		}


		private void btn_login_Click(object sender, RoutedEventArgs e)
		{
			//MessageBox.Show("Passowrd is: " + this.field_password._textValue);
			if(LoginCredentials.TryCreateFrom(this, out var credentials)) {

				if(this.cbx_remember.IsChecked.Value) {
					credentials.StoreLocally();
				} else if(LoginCredentials.IsRememberedLocally()) {
					LoginCredentials.ForgetLocallyStoredCredentials();
				}

				var mainWin = new Views.MainWindow();

				mainWin.Show();
				this.Close();

				//MessageBox.Show($"Hello, {credentials.UserName}!");
			}
		}

		private void btn_cancel_Click(object sender, RoutedEventArgs e)
		{
			Application.Current.Shutdown();
		}


	}
}
