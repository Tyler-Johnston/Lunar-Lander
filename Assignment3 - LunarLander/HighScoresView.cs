using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.IsolatedStorage;
using System.Runtime.Serialization.Json;

namespace CS5410
{
    public class HighScoresView : GameStateView
    {
        private SpriteFont m_font;
        private List<HighScore> highScores = new List<HighScore>(); // Store high scores here
        private const string MESSAGE = "High Scores: ";

        public override void loadContent(ContentManager contentManager)
        {
            m_font = contentManager.Load<SpriteFont>("Fonts/menu");
            LoadHighScores(); // Load high scores when content is loaded
        }

        private void LoadHighScores()
        {
            using (IsolatedStorageFile storage = IsolatedStorageFile.GetUserStoreForApplication())
            {
                if (storage.FileExists("HighScores.json"))
                {
                    using (IsolatedStorageFileStream fs = storage.OpenFile("HighScores.json", FileMode.Open))
                    {
                        try
                        {
                            if (fs.Length > 0)
                            {
                                DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(List<HighScore>));
                                highScores = (List<HighScore>)serializer.ReadObject(fs);
                            }
                        }
                        catch (Exception e) // Catch more specific exceptions as necessary
                        {
                            // Handle exceptions or errors as needed
                            Console.WriteLine($"Error loading high scores: {e.Message}");
                        }
                    }
                }
            }
        }

        public override GameStateEnum processInput(GameTime gameTime)
        {
            if (Keyboard.GetState().IsKeyDown(Keys.Escape))
            {
                return GameStateEnum.MainMenu;
            }

            return GameStateEnum.HighScores;
        }

        public override void render(GameTime gameTime)
        {
            m_spriteBatch.Begin();

            Vector2 stringSize = m_font.MeasureString(MESSAGE);
            Vector2 position = new Vector2(m_graphics.PreferredBackBufferWidth / 3 - stringSize.X / 2, 100); // Start a bit lower to leave space for the message

            // Draw the message
            m_spriteBatch.DrawString(m_font, MESSAGE, position, Color.Yellow);
            position.Y += 30; // Add some spacing after the message

            // Iterate and draw each high score
            foreach (var score in highScores)
            {
                string scoreText = $"{score.Name} - Level: {score.Level}, Score: {score.Score}, Date: {score.TimeStamp.ToShortDateString()}";
                m_spriteBatch.DrawString(m_font, scoreText, position, Color.White);
                position.Y += 50; // Move to the next line for the next score
            }

            m_spriteBatch.End();
        }

        public override void update(GameTime gameTime)
        {
            // Update logic, if necessary
        }
    }
}
