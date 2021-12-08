using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace BogdaroneGest.UserControls
{
	class LoginWindowPasswordField: LoginWindowTextField
	{

		const char DEFAULT_PW_CHAR = '*';

		string _textValue;
		object _textChangedLock;
		bool _interactiveMode;
		bool _skipTextValidation;
		
		public LoginWindowPasswordField() : base()
		{
			this._textChangedLock = new object();
			this._skipTextValidation = false;
			this._interactiveMode = false;
			this._textValue = string.Empty;

			this.PasswordChar = DEFAULT_PW_CHAR;

			this.ValidInputTextHandler = (text) => { return; };

			this.InvalidInputTextHandler = 
				(text) => throw new InvalidOperationException(
					$"Invalid input text '{text}'. " +
					$"White spaces are not allowed.");
		}



		public char PasswordChar { get; set; }


		[Browsable(false)]
		public Action<string> ValidInputTextHandler { get; set; }

		[Browsable(false)]
		public Action<string> InvalidInputTextHandler { get; set; }


		
		protected override void OnTextChanged(TextChangedEventArgs e)
		{
			lock(this._textChangedLock) {

				if(	this._skipTextValidation == false && 
					this._interactiveMode) 
				{
					foreach(var change in e.Changes) {
						this.IntegrateTextChange(change);
					}
				}

				base.OnTextChanged(e);
			}
		}
		
		void IntegrateTextChange(TextChange change)
		{
			var added = this.Text.Substring(change.Offset, change.AddedLength);

			this.ValidateTextChange(added, change);
		}

		void ValidateTextChange(string text, TextChange changeInfo)
		{
			this.RemoveSelectedText(changeInfo);

			if(text.Any(c => char.IsWhiteSpace(c))) {
				var encryptedText = new string(
					c: this.PasswordChar,
					count: this._textValue.Length);

				this.SetTextPropertyWithoutEvent(encryptedText);
				this.InvalidInputTextHandler.Invoke(text);
			} else {
				var encryptedText = new string(
					c: this.PasswordChar, 
					count: this.Text.Length);

				this.ApplyAddedText(text, changeInfo);
				this.SetTextPropertyWithoutEvent(encryptedText);
				this.ValidInputTextHandler.Invoke(text);
			}
		}
 
		void RemoveSelectedText(TextChange changeInfo)
		{
			if(changeInfo.RemovedLength > 0) {
				this._textValue = this._textValue
					.Remove(changeInfo.Offset, changeInfo.RemovedLength);
			}
		}

		void ApplyAddedText(
			string addedText, 
			TextChange changeInfo)
		{
			if(addedText.Any()) {
				if(string.IsNullOrEmpty(this._textValue)) {
					this._textValue = addedText;
				} else {
					this._textValue = this._textValue
						.Insert(changeInfo.Offset, addedText);
				}
			}
		}

		void SetTextPropertyWithoutEvent(string text)
		{
			this._skipTextValidation = true;
			this.Text = text;
			this.CaretIndex = this.Text.Length;
			this._skipTextValidation = false;
		}

		protected override void SwitchToInteractiveMode()
		{
			base.SwitchToInteractiveMode();
			this._interactiveMode = true;
		}

		protected override void SwitchToFreeMode()
		{
			this._interactiveMode = false;
			base.SwitchToFreeMode();
		}


	}
}
