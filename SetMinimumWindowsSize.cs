using Godot;

public partial class SetMinimumWindowsSize : Node
{
	public override void _Ready()
	{
		GetWindow().MinSize = new(164, 207);
	}
}
