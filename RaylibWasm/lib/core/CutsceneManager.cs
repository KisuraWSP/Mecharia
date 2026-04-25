using Raylib_cs;
using System.Collections.Generic;

namespace lib.core;

public class CutsceneManager
{
    private List<string> dialogueLines = new List<string>();
    private int currentLineIndex = 0;
    
    // Typewriter effect variables
    private int charsRevealed = 0;
    private float timer = 0f;
    private float typeSpeed = 0.05f; // Time in seconds between each letter

    public bool IsFinished { get; private set; } = false;

    public void StartCutscene(List<string> lines)
    {
        dialogueLines = lines;
        currentLineIndex = 0;
        charsRevealed = 0;
        timer = 0f;
        IsFinished = false;
    }

    public void Update()
    {
        if (IsFinished) return;

        string currentLine = dialogueLines[currentLineIndex];

        // 1. Reveal characters one by one
        if (charsRevealed < currentLine.Length)
        {
            timer += Raylib.GetFrameTime();
            if (timer >= typeSpeed)
            {
                charsRevealed++;
                timer = 0f;
            }
        }

        // 2. Handle Input to skip or proceed
        if (Raylib.IsKeyPressed(KeyboardKey.Space) || Raylib.IsKeyPressed(KeyboardKey.Enter))
        {
            if (charsRevealed < currentLine.Length)
            {
                // If currently typing, instantly reveal the whole line
                charsRevealed = currentLine.Length;
            }
            else
            {
                // If done typing, move to the next line
                currentLineIndex++;
                charsRevealed = 0;
                timer = 0f;

                // Check if the cutscene is entirely over
                if (currentLineIndex >= dialogueLines.Count)
                {
                    IsFinished = true;
                }
            }
        }
    }

    public void Draw()
    {
        if (IsFinished || dialogueLines.Count == 0) return;

        Raylib.ClearBackground(Color.Black);

        string currentLine = dialogueLines[currentLineIndex];
        string visibleText = currentLine.Substring(0, charsRevealed);

        int fontSize = 30;
        int textWidth = Raylib.MeasureText(visibleText, fontSize);
        int textX = (Game.width / 2) - (textWidth / 2);
        int textY = Game.height / 2;

        // --- CORRUPTED GLITCH EFFECT ---
        int glitchOffsetX = 0;
        int glitchOffsetY = 0;
        if (Raylib.GetRandomValue(0, 10) > 8) // 20% chance to glitch per frame
        {
            glitchOffsetX = Raylib.GetRandomValue(-3, 3);
            glitchOffsetY = Raylib.GetRandomValue(-3, 3);
        }

        Raylib.DrawText(visibleText, textX + glitchOffsetX + 2, textY + glitchOffsetY, fontSize, new Color(0, 255, 255, 200));
        Raylib.DrawText(visibleText, textX - glitchOffsetX - 2, textY - glitchOffsetY, fontSize, new Color(255, 0, 255, 200));
        Raylib.DrawText(visibleText, textX, textY, fontSize, Color.White);

        // --- PROMPT TO CONTINUE ---
        if (charsRevealed >= currentLine.Length)
        {
            string prompt = "[PRESS SPACE]";
            int promptWidth = Raylib.MeasureText(prompt, 20);
            
            // Blink the prompt text
            if ((int)(Raylib.GetTime() * 2) % 2 == 0)
            {
                Raylib.DrawText(prompt, (Game.width / 2) - (promptWidth / 2), Game.height - 50, 20, Color.DarkGray);
            }
        }
    }
}