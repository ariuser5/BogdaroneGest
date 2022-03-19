using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace BogdaroneGest.UserControls
{
	class UITextField : TextBox
	{

		private string _defaultText;
		private Brush _defaultTextColor;

		private string _inputText;
		private bool _isInitialized;

		public UITextField()
		{
			this._defaultText = "Insert Text";
			this._defaultTextColor = Brushes.Gray;
			this._inputText = string.Empty;
			this._isInitialized = false;	

			this.ActiveStateColor = Brushes.Black;
		}


		public string DefaultText => this._defaultText;
		public Brush DefaultStateColor => this._defaultTextColor;
		public Brush ActiveStateColor { get; set; }



		protected override void OnInitialized(EventArgs e)
		{
			base.OnInitialized(e);
			this.ReadDefaults();
			this._isInitialized = true;
		}

		private void ReadDefaults()
		{
			this._defaultText = this.Text;
			this._defaultTextColor = this.Foreground;
		}

		protected override void OnGotFocus(RoutedEventArgs e)
		{
			this.OnGotFocusSwitch();
			base.OnGotFocus(e);
		}

		protected override void OnLostFocus(RoutedEventArgs e)
		{
			base.OnLostFocus(e);
			this.OnLostFocusSwitch();
		}


		protected virtual void OnGotFocusSwitch()
		{
			if(this._inputText == string.Empty) {
				this.Text = string.Empty;
			}

			this.Foreground = this.ActiveStateColor;
		}
		

		protected virtual void OnLostFocusSwitch()
		{
			if(string.IsNullOrWhiteSpace(this._inputText)) {
				this.Text = this._defaultText;
				this.Foreground = this._defaultTextColor;
				this._inputText = string.Empty;
			}
		}

		protected override void OnTextChanged(TextChangedEventArgs e)
		{
			base.OnTextChanged(e);

			if(this._isInitialized && this.IsKeyboardFocusWithin) {
				this.UpdateInputText();
			}
		}

		void UpdateInputText()
		{
			this._inputText = this.Text;
		}


	}
}
