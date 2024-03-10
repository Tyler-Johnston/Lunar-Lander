using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using CS5410; // Assuming HighScoreManager is in the same namespace, adjust if necessary

namespace CS5410
{
    public class HighScoresView : GameStateView
    {
        private SpriteFont m_font;
        private const string MESSAGE = "High Scores:";

        public override void loadContent(ContentManager contentManager)
        {
            m_font = contentManager.Load<SpriteFont>("Fonts/menu");
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
            Vector2 position = new Vector2(m_graphics.PreferredBackBufferWidth / 3 - stringSize.X / 2, 100);

            m_spriteBatch.DrawString(m_font, MESSAGE, position, Color.Yellow);
            position.Y += 40;

            foreach (var score in HighScoreManager.HighScores)
            {
                string scoreText = $"Score: {score.Score}, Date: {score.TimeStamp.ToShortDateString()}";
                m_spriteBatch.DrawString(m_font, scoreText, position, Color.White);
                position.Y += 50;
            }

            m_spriteBatch.End();
        }

        public override void update(GameTime gameTime)
        {

        }
    }
}
