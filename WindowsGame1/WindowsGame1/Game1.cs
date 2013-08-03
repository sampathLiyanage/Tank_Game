using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using System.Net.Sockets;
using System.IO;
using System.Text;
using System.Threading;
using System.Net;
using System.Text.RegularExpressions;
using System.Collections;
using System.Configuration;

namespace WindowsGame1
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class Game1 : Microsoft.Xna.Framework.Game
    {
        int mapSize;
        WarField wf;
        Commandor cmdr;
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        GraphicsDevice device;
        Texture2D backgroundTexture1, backgroundTexture2, enemyUp, enemyDown, enemyLeft, enemyRight, meUp, meDown, meLeft, meRight, coins, brick, stone, water, lifePack;
        int screenWidth;
        int screenHeight;
        int gameScreenSize;
        Viewport vpRite;               //left side, main view
        Viewport vpLeft;               //right side, tile view
        Viewport separatorViewport;    //line between the 2 views
        Viewport defaultViewPort;
        SpriteFont font;

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            // TODO: Add your initialization logic here

            mapSize = Convert.ToInt16(ConfigurationSettings.AppSettings.Get("mapSize"));
            gameScreenSize = Convert.ToInt16(ConfigurationSettings.AppSettings.Get("screenSize")); 

           wf = WarField.Instance; //prepair game model, join game
             cmdr = Commandor.getInstant(wf);   //start game




             graphics.PreferredBackBufferWidth = gameScreenSize + 200;
             graphics.PreferredBackBufferHeight = gameScreenSize;
             graphics.IsFullScreen = false;
            graphics.ApplyChanges();
            Window.Title = "Tank-Game";
            base.Initialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);
            device = graphics.GraphicsDevice;
            backgroundTexture1 = Content.Load<Texture2D>(@"background");
            backgroundTexture2 = Content.Load<Texture2D>(@"images");

            meUp = Content.Load<Texture2D>(@"meUp");
            meDown = Content.Load<Texture2D>(@"meDown");
            meLeft = Content.Load<Texture2D>(@"meLeft");
            meRight = Content.Load<Texture2D>(@"meRight");

            enemyUp = Content.Load<Texture2D>(@"enemyUp");
            enemyDown = Content.Load<Texture2D>(@"enemyDown");
            enemyLeft = Content.Load<Texture2D>(@"enemyLeft");
            enemyRight = Content.Load<Texture2D>(@"enemyRight");

            coins = Content.Load<Texture2D>(@"coins");
            water = Content.Load<Texture2D>(@"water");
            brick = Content.Load<Texture2D>(@"brick");
            stone = Content.Load<Texture2D>(@"stone");
            lifePack = Content.Load<Texture2D>(@"lifePack");
            font = Content.Load<SpriteFont>("spriteFont1");

            screenWidth = device.PresentationParameters.BackBufferWidth;
            screenHeight = device.PresentationParameters.BackBufferHeight;
            // TODO: use this.Content to load your game content here
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// all content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            // Allows the game to exit
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
                this.Exit();

            // TODO: Add your update logic here

            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.LightSeaGreen);

            // TODO: Add your drawing code here
            spriteBatch.Begin();
            DrawScenery();


            for (int i = 0; i < mapSize; i++)
            {
                for (int j = 0; j < mapSize; j++)
                {
                    if (wf.getField()[i, j].type.Equals("tank"))
                    {

                    }

                    if (wf.getField()[i, j].type.Equals("brick"))
                    {
                        spriteBatch.Draw(brick, new Rectangle(i * (gameScreenSize / mapSize), j * (gameScreenSize / mapSize), gameScreenSize / mapSize, screenHeight / mapSize), Color.White);
                    }

                    if (wf.getField()[i, j].type.Equals("stone"))
                    {
                        spriteBatch.Draw(stone, new Rectangle(i * (gameScreenSize / mapSize), j * (gameScreenSize / mapSize), gameScreenSize / mapSize, screenHeight / mapSize), Color.White);
                    }

                    if (wf.getField()[i, j].type.Equals("water"))
                    {
                        spriteBatch.Draw(water, new Rectangle(i * (gameScreenSize / mapSize), j * (gameScreenSize / mapSize), gameScreenSize / mapSize, screenHeight / mapSize), Color.White);
                    }

                    if (wf.getField()[i, j].type.Equals("coins"))
                    {
                        spriteBatch.Draw(coins, new Rectangle(i * (gameScreenSize / mapSize), j * (gameScreenSize / mapSize), gameScreenSize / mapSize, screenHeight / mapSize), Color.White);
                    }
                    if (wf.getField()[i, j].type.Equals("lifePack"))
                    {
                        spriteBatch.Draw(lifePack, new Rectangle(i * (gameScreenSize / mapSize), j * (gameScreenSize / mapSize), gameScreenSize / mapSize, screenHeight / mapSize), Color.White);
                    }
                }
            }


            foreach (KeyValuePair<String, Tank> t in wf.tanks)
            {

                if (t.Value.getName() == wf.getMyTankName())
                {
                    if (t.Value.direction == 0)
                    {
                        spriteBatch.Draw(meUp, new Rectangle(t.Value.tankLoc.x * (gameScreenSize / mapSize), t.Value.tankLoc.y * (gameScreenSize / mapSize), gameScreenSize / mapSize, screenHeight / mapSize), Color.White);
                    }
                    else if (t.Value.direction == 1)
                    {
                        spriteBatch.Draw(meRight, new Rectangle(t.Value.tankLoc.x * (gameScreenSize / mapSize), t.Value.tankLoc.y * (gameScreenSize / mapSize), gameScreenSize / mapSize, screenHeight / mapSize), Color.White);
                    }
                    else if (t.Value.direction == 2)
                    {
                        spriteBatch.Draw(meDown, new Rectangle(t.Value.tankLoc.x * (gameScreenSize / mapSize), t.Value.tankLoc.y * (gameScreenSize / mapSize), gameScreenSize / mapSize, screenHeight / mapSize), Color.White);
                    }
                    else if (t.Value.direction == 3)
                    {
                        spriteBatch.Draw(meLeft, new Rectangle(t.Value.tankLoc.x * (gameScreenSize / mapSize), t.Value.tankLoc.y * (gameScreenSize / mapSize), gameScreenSize / mapSize, screenHeight / mapSize), Color.White);
                    }
                    this.DrawText(t.Value);

                }

                else
                {
                    if (t.Value.direction == 0)
                    {
                        spriteBatch.Draw(enemyUp, new Rectangle(t.Value.tankLoc.x * (gameScreenSize / mapSize), t.Value.tankLoc.y * (gameScreenSize / mapSize), gameScreenSize / mapSize, screenHeight / mapSize), Color.White);
                    }
                    else if (t.Value.direction == 1)
                    {
                        spriteBatch.Draw(enemyRight, new Rectangle(t.Value.tankLoc.x * (gameScreenSize / mapSize), t.Value.tankLoc.y * (gameScreenSize / mapSize), gameScreenSize / mapSize, screenHeight / mapSize), Color.White);
                    }
                    else if (t.Value.direction == 2)
                    {
                        spriteBatch.Draw(enemyDown, new Rectangle(t.Value.tankLoc.x * (gameScreenSize / mapSize), t.Value.tankLoc.y * (gameScreenSize / mapSize), gameScreenSize / mapSize, screenHeight / mapSize), Color.White);
                    }
                    else if (t.Value.direction == 3)
                    {
                        spriteBatch.Draw(enemyLeft, new Rectangle(t.Value.tankLoc.x * (gameScreenSize / mapSize), t.Value.tankLoc.y * (gameScreenSize / mapSize), gameScreenSize / mapSize, screenHeight / mapSize), Color.White);
                    }
                }

            }


            spriteBatch.End();

            base.Draw(gameTime);
        }

        private void DrawScenery()
        {
            Rectangle screenRectangle1 = new Rectangle(0, 0, screenWidth - 200, screenHeight);
            spriteBatch.Draw(backgroundTexture1, screenRectangle1, Color.White);
            Rectangle screenRectangle2 = new Rectangle(0, screenWidth - 200, 200, screenHeight);
            spriteBatch.Draw(backgroundTexture2, screenRectangle2, Color.White);
         
        }

        private void DrawText(Tank t)
        {

            spriteBatch.DrawString(font, "POINTS:", new Vector2(605, 40), Color.Black);
            spriteBatch.DrawString(font, t.points.ToString(), new Vector2(710, 40), Color.Black);

            spriteBatch.DrawString(font, "COINS:", new Vector2(605, 60), Color.Black);
            spriteBatch.DrawString(font, t.coins.ToString(), new Vector2(710, 60), Color.Black);

            spriteBatch.DrawString(font, "HEALTH:", new Vector2(605, 80), Color.Black);
            spriteBatch.DrawString(font, t.helth.ToString(), new Vector2(710, 80), Color.Black);
        }
    }


    



   

    /*
     * ############################
     * Graphic Controller component (to be implemented)
     * ############################
     * 
     * Easy to implemet
     * Just access the 2D array by calling "WarField.getField()" method
     * access the Location objects of the 2D Array and render the graphic according to the types of locations using XNA framework
     */

    /*
     * ##############
     * GUI component (to be implemented)
     * ##############
     * 
     * Implement if features like a button for start the game, end the game are needed
     * otherwise game will start when program runs
     * 
     */
}
