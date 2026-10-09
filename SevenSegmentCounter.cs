using System;
using Godot;

[GlobalClass, Tool]
public partial class SevenSegmentCounter : HBoxContainer
{
	private const string DigitNodePrefix = "___SEVEN_SEGMENT_DISPLAY_";

	private int _value = 0;
	private int _digits = 1;

	[Export]
	public Texture2D Texture { get; set; }

	[Export(PropertyHint.Range, "0,100,1,or_greater")]
	public int Value
	{
		get => _value;
		set
		{
			if ((value >= 0) && (value.ToString().Length <= _digits))
				_value = value;

			Update();
		}
	}

	[Export(PropertyHint.Range, "0,100,1,or_greater")]
	public int Digits
	{
		get => _digits;
		set
		{
			if (value > 0)
				_digits = value;

			_value = Math.Clamp(_value, AllowNegative ? int.MinValue : 0, ((int)Math.Pow(10, _digits)) - 1);

			Update();
		}
	}

	[Export]
	public bool AllowNegative { get; set; } = true;

	public override void _Ready()
	{
		Update();
	}

	internal void Update()
	{
		if (Texture is null)
		{
			GD.PushWarning("Texture not set.");
			return;
		}

		foreach (var digitRect in GetChildren())
		{
			if (digitRect.Name.ToString().StartsWith(DigitNodePrefix))
			{
				digitRect.QueueFree();
			}
		}

		foreach (char digit in _value.ToString().PadLeft(_digits, '0'))
		{
			int digitOffset = (byte)digit - (byte)'0';

			TextureRect digitRect = new()
			{
				Name = DigitNodePrefix + GD.Randi().ToString(),
				Texture = new AtlasTexture()
				{
					Atlas = Texture,
					Region = new Rect2(13 + digitOffset * 13, 0, new(13, 23))
				}
			};
			AddChild(digitRect);
		}
	}
}
