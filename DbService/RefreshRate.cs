using System;

namespace DbService
{
	struct RefreshRate
	{

		const int DEFAULT = 250;
		const int MIN = 0;
		const int MAX = 1000;


		public static implicit operator int(RefreshRate value)
			=> value.Value;

		public static implicit operator RefreshRate(int value)
			=> new(value);

		public static RefreshRate Parse(string value)
		{
			var valueInt = int.Parse(value);

			return new RefreshRate(valueInt);
		}

		public static RefreshRate Parse(int value)
		{
			return new RefreshRate(value);
		}


		readonly int? _value;

		public RefreshRate(int value)
		{
			if(value >= MIN && value <= MAX) {
				this._value = value;
			} else {
				throw new InvalidOperationException(
					$"Invalid value. Instances of {nameof(RefreshRate)} " +
					$"can have only values between {MIN} and {MAX}.");
			}
		}


		public int Value => this._value ?? DEFAULT;


		public override string ToString()
		{
			return this.Value.ToString();
		}

	}
}
