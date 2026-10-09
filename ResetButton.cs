using Godot;

namespace Sweeper;

// TODO:
// - allow to cancel press if mouse not on the button on release
public partial class ResetButton : TextureRect
{
	public enum ButtonState
	{
		Idle,
		Pressed,
		Ooh,
		Dead,
	}

	[Signal] public delegate void PressedEventHandler();

	[Export] public ButtonState State { get => _state; set { _state = value; UpdateTexture(); } }

	[Export] public Rect2 IdleTextureRegion { get; set; } = new Rect2();
	[Export] public Rect2 PressedTextureRegion { get; set; } = new Rect2();
	[Export] public Rect2 OohTextureRegion { get; set; } = new Rect2();
	[Export] public Rect2 DeadTextureRegion { get; set; } = new Rect2();

	private ButtonState _state = ButtonState.Idle;
	private ButtonState _stateBeforePressed;

	private Rect2 GetRectForState(ButtonState state)
	{
		return state switch
		{
			ButtonState.Idle => IdleTextureRegion,
			ButtonState.Pressed => PressedTextureRegion,
			ButtonState.Ooh => OohTextureRegion,
			ButtonState.Dead => DeadTextureRegion,
			_ => throw new System.NotImplementedException(),
		};
	}

	private void UpdateTexture()
	{
		(Texture as AtlasTexture).Region = GetRectForState(_state);
	}

	public override void _GuiInput(InputEvent @event)
	{
		if ((@event is InputEventMouseButton mouseButton) && (mouseButton.ButtonIndex == MouseButton.Left))
		{
			if (mouseButton.Pressed)
			{
				_stateBeforePressed = State;
				State = ButtonState.Pressed;
			}
			else
			{
				State = _stateBeforePressed;
				EmitSignal(SignalName.Pressed);
			}

			UpdateTexture();
		}
	}
}
