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
			this.InteractiveModeSwitch = () => { return; };
			this.FreeModeSwitch = () => { return; };
		}



		[Browsable(false)]
		public Action InteractiveModeSwitch { get; set; }

		[Browsable(false)]
		public Action FreeModeSwitch { get; set; }


		protected virtual void SwitchToInteractiveMode()
		{
			this.InteractiveModeSwitch.Invoke();
		}

		protected virtual void SwitchToFreeMode()
		{
			this.FreeModeSwitch.Invoke();
		}

		protected override void OnGotFocus(RoutedEventArgs e)
		{
			this.SwitchToInteractiveMode();
			base.OnGotFocus(e);
		}

		protected override void OnLostFocus(RoutedEventArgs e)
		{
			base.OnLostFocus(e);
			this.SwitchToFreeMode();
		}


	}
}
