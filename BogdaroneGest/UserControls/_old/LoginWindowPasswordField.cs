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

		object _textChangedLock;
		bool _interactiveMode;
		bool _skipTextValidation;
		
		public LoginWindowPasswordField() : base()
		{
			this._textChangedLock = new object();
			this._skipTextValidation = false;
			this._interactiveMode = false;

			this.PasswordChar = DEFAULT_PW_CHAR;
			this.HiddentTextValue = string.Empty;
			this.ValidInputTextHandler = (text) => { return; };

			this.InvalidInputTextHandler = 
				(text) => throw new InvalidOperationException(
					$"Invalid input text '{text}'. " +
					$"White spaces are not allowed.");
		}



		public char PasswordChar { get; set; }

		[Browsable(false)]
		public string HiddentTextValue { get; set; }

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
			var newCaretPosition = changeInfo.Offset + changeInfo.AddedLength;

			this.RemoveOverwrittenText(changeInfo);

			if(text.Any(c => char.IsWhiteSpace(c))) {
				var encryptedText = new string(
					c: this.PasswordChar,
					count: this.HiddentTextValue.Length);

				this.UpdateTextUIWithoutRaisingEvent(
					value: encryptedText,
					caretPosition: newCaretPosition);

				this.InvalidInputTextHandler.Invoke(text);
			} else {
				var encryptedText = new string(
					c: this.PasswordChar, 
					count: this.Text.Length);

				this.ApplyAddedText(text, changeInfo);
				
				this.UpdateTextUIWithoutRaisingEvent(
					value: encryptedText,
					caretPosition: newCaretPosition);

				this.ValidInputTextHandler.Invoke(text);
			}
		}
 
		void RemoveOverwrittenText(TextChange changeInfo)
		{
			if(changeInfo.RemovedLength > 0) {
				this.HiddentTextValue = this.HiddentTextValue
					.Remove(changeInfo.Offset, changeInfo.RemovedLength);
			}
		}

		void ApplyAddedText(string addedText, TextChange changeInfo)
		{
			if(addedText.Any()) {
				if(string.IsNullOrEmpty(this.HiddentTextValue)) {
					this.HiddentTextValue = addedText;
				} else {
					this.HiddentTextValue = this.HiddentTextValue
						.Insert(changeInfo.Offset, addedText);
				}
			}
		}

		void UpdateTextUIWithoutRaisingEvent(string value, int caretPosition)
		{
			lock(this._textChangedLock) { 
				this._skipTextValidation = true;
				this.Text = value;
				this.CaretIndex = caretPosition;
				this._skipTextValidation = false;
			}
		}

		protected override void OnInteractiveModeSwitch()
		{
			base.OnInteractiveModeSwitch();
			this._interactiveMode = true;
		}

		protected override void OnFreeModeSwitch()
		{
			this._interactiveMode = false;
			base.OnFreeModeSwitch();
		}


	}
}
