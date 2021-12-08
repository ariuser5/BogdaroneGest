using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;

namespace BogdaroneGest.UserControls
{
	/// <summary>
	/// Interaction logic for LoginWindowField.xaml
	/// </summary>
	public partial class LoginWindowField: UserControl
	{


		//static LoginWindowField()
		//{
		//	NameProperty.OverrideMetadata(
		//		forType: typeof(LoginWindowField), 
		//		typeMetadata: 
		//			new FrameworkPropertyMetadata("asd"));
		//}



		public LoginWindowField()
		{
			InitializeComponent();

			this.SwitchToFocusedState = () => { return; };
			this.SwitchToUnfocusedState = () => { return; };
		}


		[Category("Common")]
		public string Text {
			get => this.textBox.Text;
			set => this.textBox.Text = value;
		}

		[Browsable(false)]
		public Action SwitchToFocusedState { get; set; }

		[Browsable(false)]
		public Action SwitchToUnfocusedState { get; set; }


		protected override void OnGotFocus(RoutedEventArgs e)
		{
			this.SwitchToFocusedState.Invoke();
			base.OnGotFocus(e);
		}

		protected override void OnLostFocus(RoutedEventArgs e)
		{
			base.OnLostFocus(e);
			this.SwitchToUnfocusedState.Invoke();
		}


	}
}
