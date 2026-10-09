using System;
using Godot;

namespace Sweeper;

[Flags]
public enum TileFlags : byte
{
	None = 0,
	Mine = 1 << 0,
	Flagged = 1 << 1,
	Revealed = 1 << 2,
}

// TODO: pack into a single byte?
public record struct Tile(byte NearbyMines = 0, TileFlags Flags = TileFlags.None);

public partial class GameManager : Node
{
	private bool _mousePressed = false;

	private TileMapLayer _tileMap;
	private ResetButton _resetButton;

	private Tile[,] _tiles;

	private Vector2I MouseTilePos => _tileMap.LocalToMap(_tileMap.GetLocalMousePosition());

	GameManager() : base()
	{
		Reset();
	}

	public void Reset()
	{
		_tiles = new Tile[9, 9];

		const int MinesToSpawn = 10;
		int spawnedAlready = 0;

		while (spawnedAlready <= MinesToSpawn)
		{
			ref Tile tile = ref _tiles[GD.RandRange(0, 8), GD.RandRange(0, 8)];
			if (tile.Flags.HasFlag(TileFlags.Mine))
				continue;

			tile.Flags |= TileFlags.Mine;

			spawnedAlready++;
		}

		UpdateNearbys();
	}

	internal void UpdateNearbys()
	{
		for (var x = 0; x < 9; x++)
		{
			for (var y = 0; y < 9; y++)
			{
				ref Tile tile = ref _tiles[x, y];

				for (var xx = -1; xx <= 1; xx++)
				{
					for (var yy = -1; yy <= 1; yy++)
					{
						if (!IsTilePossible(new(x + xx, y + yy))) continue;
						if (xx == 0 && yy == 0) continue;

						ref Tile surroundingTile = ref _tiles[x + xx, y + yy];

						if (surroundingTile.Flags.HasFlag(TileFlags.Mine))
							tile.NearbyMines++;
					}
				}
			}
		}
	}

	internal static bool IsTilePossible(Vector2I tilePos)
	{
		return (tilePos.X >= 0) && (tilePos.X < 9) && (tilePos.Y >= 0) && (tilePos.Y < 9);
	}

	internal void UpdateTileMap()
	{
		for (var x = 0; x < 9; x++)
		{
			for (var y = 0; y < 9; y++)
			{
				Vector2I tilePos = new(x, y);

				ref Tile tile = ref _tiles[x, y];
				bool isPressed = _mousePressed && (MouseTilePos == tilePos);

				Vector2I atlasCoords = new(0, 5);

				if (tile.Flags.HasFlag(TileFlags.Flagged))
					atlasCoords = new(4, 5);
				else if (tile.Flags.HasFlag(TileFlags.Revealed))
					if (tile.Flags.HasFlag(TileFlags.Mine))
						atlasCoords = new(2, 5);
					else if (tile.NearbyMines <= 0)
						atlasCoords = new(1, 5);
					else
						atlasCoords = new(tile.NearbyMines - 1, 6);
				else if (isPressed)
					atlasCoords = new(1, 5);

				_tileMap.SetCell(tilePos, 0, atlasCoords);
			}
		}
	}

	public override void _Ready()
	{
		_resetButton = GetNode<ResetButton>("%ResetButton");
		_resetButton.Pressed += Reset;

		_tileMap = GetNode<TileMapLayer>("%TileMap");
		_tileMap.GetParent<Control>().GuiInput += (@event) =>
		{
			// if (@event is InputEventMouseMotion mouseMotion)
			// {

			// }

			if (@event is InputEventMouseButton mouseButton)
			{
				_mousePressed = mouseButton.Pressed;
				if (mouseButton.Pressed) return;

				if (!IsTilePossible(MouseTilePos))
					return;

				ref Tile tile = ref _tiles[MouseTilePos.X, MouseTilePos.Y];
				if (tile.Flags.HasFlag(TileFlags.Revealed)) return;

				tile.Flags |= TileFlags.Revealed;

				// _tiles[tilePos.X, tilePos.Y].Flags = _tiles[tilePos.X, tilePos.Y].Flags & TileFlags.Pressed;
			}
		};
	}

	public override void _Process(double delta)
	{
		UpdateTileMap();
	}
}
