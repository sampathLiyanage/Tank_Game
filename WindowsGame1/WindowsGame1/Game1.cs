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
        Texture2D backgroundTexture,tank_up,tank_down,tank_left,tank_right,coins,brick,stone,water;
        int screenWidth;
        int screenHeight;
        int gameScreenSize;
        

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
             cmdr = new Commandor(wf);   //start game

            

            
            graphics.PreferredBackBufferWidth = gameScreenSize;
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
            backgroundTexture = Content.Load<Texture2D>(@"background");

            tank_up = Content.Load<Texture2D>(@"up");
            tank_down = Content.Load<Texture2D>(@"down");
            tank_left = Content.Load<Texture2D>(@"left");
            tank_right = Content.Load<Texture2D>(@"right");
            coins = Content.Load<Texture2D>(@"coins");
            water = Content.Load<Texture2D>(@"water");
            brick = Content.Load<Texture2D>(@"brick");
            stone = Content.Load<Texture2D>(@"stone");

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
            GraphicsDevice.Clear(Color.CornflowerBlue);

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
                }
            }

           
           foreach (KeyValuePair<String,Tank> t in wf.tanks){
               if (t.Value.direction == 0)
               {
                   spriteBatch.Draw(tank_up, new Rectangle(t.Value.tankLoc.x * (gameScreenSize / mapSize), t.Value.tankLoc.y * (gameScreenSize / mapSize), gameScreenSize / mapSize, screenHeight / mapSize), Color.White);
               }
               else if (t.Value.direction == 1)
               {
                   spriteBatch.Draw(tank_right, new Rectangle(t.Value.tankLoc.x * (gameScreenSize / mapSize), t.Value.tankLoc.y * (gameScreenSize / mapSize), gameScreenSize / mapSize, screenHeight / mapSize), Color.White);
               }
               else if (t.Value.direction == 2)
               {
                   spriteBatch.Draw(tank_down, new Rectangle(t.Value.tankLoc.x * (gameScreenSize / mapSize), t.Value.tankLoc.y * (gameScreenSize / mapSize), gameScreenSize / mapSize, screenHeight / mapSize), Color.White);
               }
               else if (t.Value.direction == 3)
               {
                   spriteBatch.Draw(tank_left, new Rectangle(t.Value.tankLoc.x * (gameScreenSize / mapSize), t.Value.tankLoc.y * (gameScreenSize / mapSize), gameScreenSize / mapSize, screenHeight / mapSize), Color.White);
               }
               
           }
           

            spriteBatch.End();

            base.Draw(gameTime);
        }

        private void DrawScenery()
        {
            Rectangle screenRectangle = new Rectangle(0, 0, screenWidth, screenHeight);
            spriteBatch.Draw(backgroundTexture, screenRectangle, Color.White);
         
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
