using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;

namespace BogdaroneGest.UserControls
{
	class LoginWindowTextField: TextBox
	{


		public LoginWindowTextField()
		{
			this.InteractiveModeSwitchHandler = () => { return; };
			this.FreeModeSwitchHandler = () => { return; };
		}



		[Browsable(false)]
		public Action InteractiveModeSwitchHandler { get; set; }

		[Browsable(false)]
		public Action FreeModeSwitchHandler { get; set; }



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
