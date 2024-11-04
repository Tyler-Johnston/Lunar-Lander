using System;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace CS5410.Controls
{
    public class CustomizeControlsView : GameStateView
    {
        private SpriteFont m_fontMenu;
        private SpriteFont m_fontMenuSelect;
        private bool m_waitForKeyRelease = false;
        private bool m_waitingForKeyPress = false;
        private const string MESSAGE = "Select 'enter' followed by your new desired key binding";
        private enum MenuState
        {
            ThrustControl,
            LeftControl,
            RightControl
        }
        private MenuState m_currentSelection = MenuState.ThrustControl;

        public override void loadContent(ContentManager contentManager)
        {
            m_fontMenu = contentManager.Load<SpriteFont>("Fonts/menu");
            m_fontMenuSelect = contentManager.Load<SpriteFont>("Fonts/menu-select");
        }
        public override GameStateEnum processInput(GameTime gameTime)
        {
            KeyboardState keyboardState = Keyboard.GetState();
            Keys[] pressedKeys = keyboardState.GetPressedKeys();

            if (!m_waitForKeyRelease && !m_waitingForKeyPress)
            {
                if (keyboardState.IsKeyDown(Keys.Down))
                {
                    int maxValue = Enum.GetValues(typeof(MenuState)).Cast<int>().Max();
                    if ((int)m_currentSelection < maxValue)
                    {
                        m_currentSelection += 1;
                        m_waitForKeyRelease = true;
                    }
                }
                else if (keyboardState.IsKeyDown(Keys.Up))
                {
                    int minValue = 0;
                    if ((int)m_currentSelection > minValue)
                    {
                        m_currentSelection -= 1;
                        m_waitForKeyRelease = true;
                    }
                }
                else if (keyboardState.IsKeyDown(Keys.Enter))
                {
                    m_waitingForKeyPress = true;
                    m_waitForKeyRelease = true;
                }
            }

            if (m_waitingForKeyPress && pressedKeys.Length > 0 && !keyboardState.IsKeyDown(Keys.Enter))
            {
                Keys newKey = pressedKeys.FirstOrDefault(k => k != Keys.Enter && k != Keys.Escape && k != Keys.Y);
                if (newKey != Keys.None)
                {
                    switch (m_currentSelection)
                    {
                        case MenuState.ThrustControl:
                            ControlsManager.SetControl("Thrust", newKey);
                            break;
                        case MenuState.LeftControl:
                            ControlsManager.SetControl("RotateLeft", newKey);
                            break;
                        case MenuState.RightControl:
                            ControlsManager.SetControl("RotateRight", newKey);
                            break;
                    }
                    m_waitingForKeyPress = false;
                    m_waitForKeyRelease = true;
                }
            }

            if (m_waitForKeyRelease && pressedKeys.Length == 0)
            {
                m_waitForKeyRelease = false;
            }

            if (keyboardState.IsKeyDown(Keys.Escape))
            {
                return GameStateEnum.MainMenu;
            }

            return GameStateEnum.CustomizeControls;
        }

        public override void render(GameTime gameTime)
        {
            m_spriteBatch.Begin();

            // Draw the message at the top
            Vector2 messageSize = m_fontMenu.MeasureString(MESSAGE);
            m_spriteBatch.DrawString(m_fontMenu, MESSAGE, new Vector2(m_graphics.PreferredBackBufferWidth / 2 - messageSize.X / 2, 100), Color.White);

            // Increment the starting position for menu items
            float bottom = 150 + messageSize.Y;

            // Drawing menu items
            bottom = drawMenuItem(
                m_currentSelection == MenuState.ThrustControl ? m_fontMenuSelect : m_fontMenu, 
                $"Thrust Controls: {ControlsManager.Controls["Thrust"]}",
                bottom, 
                m_currentSelection == MenuState.ThrustControl ? Color.Yellow : Color.Blue);
            bottom = drawMenuItem(
                m_currentSelection == MenuState.LeftControl ? m_fontMenuSelect : m_fontMenu, 
                $"Left Rotate: {ControlsManager.Controls["RotateLeft"]}",
                bottom, 
                m_currentSelection == MenuState.LeftControl ? Color.Yellow : Color.Blue);
            bottom = drawMenuItem(
                m_currentSelection == MenuState.RightControl ? m_fontMenuSelect : m_fontMenu, 
                $"Right Rotate: {ControlsManager.Controls["RotateRight"]}",
                bottom, 
                m_currentSelection == MenuState.RightControl ? Color.Yellow : Color.Blue);

            // Visual feedback for setting a new key
            if (m_waitingForKeyPress)
            {
                string waitingMessage = "Press a new key...";
                Vector2 waitingMessageSize = m_fontMenu.MeasureString(waitingMessage);
                m_spriteBatch.DrawString(
                    m_fontMenu, 
                    waitingMessage, 
                    new Vector2(m_graphics.PreferredBackBufferWidth / 2 - waitingMessageSize.X / 2, bottom + 20), 
                    Color.Red);
            }
            m_spriteBatch.End();
        }

        private float drawMenuItem(SpriteFont font, string text, float y, Color color)
        {
            Vector2 stringSize = font.MeasureString(text);
            m_spriteBatch.DrawString(
                font,
                text,
                new Vector2(m_graphics.PreferredBackBufferWidth / 2 - stringSize.X / 2, y),
                color);

            return y + stringSize.Y;
        }

        public override void update(GameTime gameTime)
        {
        }
    }
}
