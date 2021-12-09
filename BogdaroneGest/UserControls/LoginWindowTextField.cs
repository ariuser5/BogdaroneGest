using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace BogdaroneGest.UserControls
{
	class LoginWindowTextField: TextBox
	{

		string _initialText;
		Brush _initialTextColor;

		public LoginWindowTextField()
		{
			this.InteractiveModeSwitchHandler = this.DefaultInteractiveMode;
			this.FreeModeSwitchHandler = this.DefaultFreeMode;
		}



		[Browsable(false)]
		public Action InteractiveModeSwitchHandler { get; set; }

		[Browsable(false)]
		public Action FreeModeSwitchHandler { get; set; }


		protected override void OnInitialized(EventArgs e)
		{
			base.OnInitialized(e);
			this.ReadDefaults();
		}

		private void ReadDefaults()
		{
			this._initialText = this.Text;
			this._initialTextColor = this.Foreground;
		}

		private void DefaultInteractiveMode()
		{
			if(string.Equals(this.Text, this._initialText)) {
				this.Text = string.Empty;
			}

			this.Foreground = Brushes.Black;
		}

		private void DefaultFreeMode()
		{
			if(string.IsNullOrWhiteSpace(this.Text)) {
				this.Text = this._initialText;
			}
			
			this.Foreground = this._initialTextColor;
		}


		public void ToggleInteractiveMode(bool value)
		{
			if(value) {
				this.OnInteractiveModeSwitch();
			} else {
				this.OnFreeModeSwitch();
			}
		}

		protected virtual void OnInteractiveModeSwitch()
		{
			this.InteractiveModeSwitchHandler.Invoke();
		}

		protected virtual void OnFreeModeSwitch()
		{
			this.FreeModeSwitchHandler.Invoke();
		}

		protected override void OnGotFocus(RoutedEventArgs e)
		{
			this.OnInteractiveModeSwitch();
			base.OnGotFocus(e);
		}

		protected override void OnLostFocus(RoutedEventArgs e)
		{
			base.OnLostFocus(e);
			this.OnFreeModeSwitch();
		}


	}
}
