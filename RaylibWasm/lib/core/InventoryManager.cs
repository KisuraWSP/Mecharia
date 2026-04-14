using Raylib_cs;
using System.Numerics;
using Optional;
using Optional.Unsafe;
using System.Collections.Generic;

namespace lib.core;

public class InventoryManager
{
    public const int Cols = 10;
    public const int Rows = 6;
    public const int CellSize = 50;

    private int startX;
    private int startY;

    private Option<InventoryItem>[,] grid = new Option<InventoryItem>[Cols, Rows];
    public List<InventoryItem> Items { get; private set; } = new List<InventoryItem>();

    private Option<InventoryItem> draggedItem = Option.None<InventoryItem>();
    private Vector2 dragOffset;

    public InventoryManager(int screenWidth, int screenHeight)
    {
        startX = (screenWidth - (Cols * CellSize)) / 2;
        startY = (screenHeight - (Rows * CellSize)) / 2;

        for (int x = 0; x < Cols; x++)
        {
            for (int y = 0; y < Rows; y++)
            {
                grid[x, y] = Option.None<InventoryItem>();
            }
        }
    }

    // --- LOGIC: AUTO-ADD ITEM ON PICKUP ---
    public bool TryAddItem(InventoryItem item)
    {
        for (int y = 0; y < Rows; y++)
        {
            for (int x = 0; x < Cols; x++)
            {
                if (CanPlaceItem(item, x, y))
                {
                    PlaceItem(item, x, y);
                    Items.Add(item);
                    return true;
                }
            }
        }
        return false;
    }

    private bool CanPlaceItem(InventoryItem item, int startGridX, int startGridY)
    {
        if (startGridX + item.GridWidth > Cols || startGridY + item.GridHeight > Rows)
            return false;

        for (int x = startGridX; x < startGridX + item.GridWidth; x++)
        {
            for (int y = startGridY; y < startGridY + item.GridHeight; y++)
            {
                if (grid[x, y].HasValue)
                {
                    // Option<T> implements equality, so we can compare directly
                    if (!draggedItem.HasValue || grid[x, y] != draggedItem)
                    {
                        return false;
                    }
                }
            }
        }
        return true;
    }

    private void PlaceItem(InventoryItem item, int x, int y)
    {
        item.GridX = x;
        item.GridY = y;

        for (int ix = x; ix < x + item.GridWidth; ix++)
        {
            for (int iy = y; iy < y + item.GridHeight; iy++)
            {
                grid[ix, iy] = Option.Some(item);
            }
        }
    }

    private void RemoveItemFromGrid(InventoryItem item)
    {
        for (int x = item.GridX; x < item.GridX + item.GridWidth; x++)
        {
            for (int y = item.GridY; y < item.GridY + item.GridHeight; y++)
            {
                grid[x, y] = Option.None<InventoryItem>();
            }
        }
    }

    // --- UI: UPDATE & DRAG-AND-DROP ---
    public void Update()
    {
        Vector2 mousePos = Raylib.GetMousePosition();

        if (Raylib.IsMouseButtonPressed(MouseButton.Left) && !draggedItem.HasValue)
        {
            int gridX = ((int)mousePos.X - startX) / CellSize;
            int gridY = ((int)mousePos.Y - startY) / CellSize;

            if (gridX >= 0 && gridX < Cols && gridY >= 0 && gridY < Rows)
            {
                Option<InventoryItem> clickedSlot = grid[gridX, gridY];

                if (clickedSlot.HasValue)
                {
                    draggedItem = clickedSlot;
                    InventoryItem currentItem = draggedItem.ValueOrFailure();

                    currentItem.OriginalX = currentItem.GridX;
                    currentItem.OriginalY = currentItem.GridY;
                    currentItem.OriginalWidth = currentItem.GridWidth;
                    currentItem.OriginalHeight = currentItem.GridHeight;

                    RemoveItemFromGrid(currentItem);

                    dragOffset = new Vector2(
                        mousePos.X - (startX + currentItem.GridX * CellSize),
                        mousePos.Y - (startY + currentItem.GridY * CellSize)
                    );
                }
            }
        }

        if (draggedItem.HasValue)
        {
            InventoryItem currentItem = draggedItem.ValueOrFailure();

            if (Raylib.IsKeyPressed(KeyboardKey.R))
            {
                currentItem.Rotate();
            }

            if (Raylib.IsMouseButtonReleased(MouseButton.Left))
            {
                int dropX = (int)((mousePos.X - dragOffset.X - startX + (CellSize / 2)) / CellSize);
                int dropY = (int)((mousePos.Y - dragOffset.Y - startY + (CellSize / 2)) / CellSize);

                if (dropX >= 0 && dropY >= 0 && CanPlaceItem(currentItem, dropX, dropY))
                {
                    PlaceItem(currentItem, dropX, dropY);
                }
                else
                {
                    currentItem.GridWidth = currentItem.OriginalWidth;
                    currentItem.GridHeight = currentItem.OriginalHeight;
                    PlaceItem(currentItem, currentItem.OriginalX, currentItem.OriginalY);
                }

                draggedItem = Option.None<InventoryItem>();
            }
        }
    }

    // --- UI: RENDERING ---
    public void Draw()
    {
        Raylib.DrawRectangle(0, 0, Raylib.GetScreenWidth(), Raylib.GetScreenHeight(), new Color(0, 0, 0, 150));

        for (int x = 0; x < Cols; x++)
        {
            for (int y = 0; y < Rows; y++)
            {
                Raylib.DrawRectangleLines(startX + x * CellSize, startY + y * CellSize, CellSize, CellSize, Color.DarkGray);
                Raylib.DrawRectangle(startX + x * CellSize + 1, startY + y * CellSize + 1, CellSize - 2, CellSize - 2, new Color(40, 40, 40, 255));
            }
        }

        foreach (var item in Items)
        {
            if (!draggedItem.HasValue || Option.Some(item) != draggedItem)
            {
                Rectangle destRect = new Rectangle(startX + item.GridX * CellSize, startY + item.GridY * CellSize, item.GridWidth * CellSize, item.GridHeight * CellSize);
                Rectangle sourceRect = new Rectangle(0, 0, item.Sprite.Width, item.Sprite.Height);

                Raylib.DrawRectangleRec(destRect, new Color(20, 20, 20, 200));

                Raylib.DrawTexturePro(item.Sprite, sourceRect, destRect, Vector2.Zero, 0f, Color.White);

                Raylib.DrawRectangleLinesEx(destRect, 2, Color.White);
            }
        }

        if (draggedItem.HasValue)
        {
            InventoryItem currentItem = draggedItem.ValueOrFailure();
            Vector2 mousePos = Raylib.GetMousePosition();
            Rectangle dragRect = new Rectangle(mousePos.X - dragOffset.X, mousePos.Y - dragOffset.Y, currentItem.GridWidth * CellSize, currentItem.GridHeight * CellSize);
            Rectangle sourceRect = new Rectangle(0, 0, currentItem.Sprite.Width, currentItem.Sprite.Height);

            Raylib.DrawTexturePro(currentItem.Sprite, sourceRect, dragRect, Vector2.Zero, 0f, new Color(255, 255, 255, 200));

            Raylib.DrawRectangleLinesEx(dragRect, 2, Color.Yellow);
        }
    }
}