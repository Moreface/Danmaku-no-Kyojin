﻿using Danmaku_no_Kyojin.BulletEngine;
using Danmaku_no_Kyojin.BulletML;
using Danmaku_no_Kyojin.Camera;
using Danmaku_no_Kyojin.Controls;
using Danmaku_no_Kyojin.Entities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Danmaku_no_Kyojin.Screens
{
    public class GameplayScreen : BaseGameState
    {
        private List<Texture2D> _logos;
        private Texture2D _bulletSprite;

        public static Ship Ship;
        private Enemy _enemy;

        // Audio
        AudioEngine _audioEngine;
        WaveBank _waveBank;
        SoundBank _soundBank;

        Cue music = null;

        // Random
        public static Random Rand = new Random();

        // Bullet engine
        static public BulletMLParser parser = new BulletMLParser();
        int timer = 0;
        Mover mover;

        // Bullet
        private List<BaseBullet> _bullets; 

        public GameplayScreen(Game game, GameStateManager manager)
            : base(game, manager)
        {
            _bullets = new List<BaseBullet>();

            Ship = new Ship(GameRef, ref _bullets, new Vector2(GameRef.Graphics.GraphicsDevice.Viewport.Width / 2, GameRef.Graphics.GraphicsDevice.Viewport.Height - 150));
            _enemy = new Enemy(GameRef);
        }

        public override void Initialize()
        {
            Ship.Initialize();
            _enemy.Initialize();

            _audioEngine = new AudioEngine("Content\\Audio\\DnK.xgs");
            _waveBank = new WaveBank(_audioEngine, "Content\\Audio\\Wave Bank.xwb");
            _soundBank = new SoundBank(_audioEngine, "Content\\Audio\\Sound Bank.xsb");

            base.Initialize();
        }

        protected override void LoadContent()
        {
            _logos = new List<Texture2D>();

            for (int i = 0; i < 1; i++)
            {
                _logos.Add(GameRef.Content.Load<Texture2D>(@"Graphics/Pictures/logo"));
            }

            _bulletSprite = GameRef.Content.Load<Texture2D>(@"Graphics/Sprites/ball");
            parser.ParseXML(@"Content/XML/sample.xml");
            //parser.ParseXML(@"Content/XML/3way.xml");
            //parser.ParseXML(@"Content/XML/test.xml");

            BulletMLManager.Init(new BulletFunctions());

            if (music == null)
            {
                music = _soundBank.GetCue("Background");
                //music.Play();
            }

            base.LoadContent();
        }

        protected override void UnloadContent()
        {

        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            /*
            timer++;
            if (timer > 1)
            {
                timer = 0;
                if (mover.used == false)
                {
                    mover = MoverManager.CreateMover();
                    mover.pos = new Vector2(40 + (800 * (float)Rand.NextDouble()), 40 + (600 * (float)Rand.NextDouble()));
                    mover.SetBullet(parser.tree);
                }
            }
            */

            if (MoverManager.movers.Count < 1)
            {
                mover = MoverManager.CreateMover();
                mover.pos = new Vector2(401, 82);
                mover.SetBullet(parser.tree);
            }

            if (Ship.BulletTime)
            {
                GameTime newGameTime = new GameTime(gameTime.TotalGameTime,
                    new TimeSpan((long)(gameTime.ElapsedGameTime.Ticks / Config.DesiredTimeModifier)));
                gameTime = newGameTime;
            }

            MoverManager.Update(gameTime);
            MoverManager.FreeMovers();

            if (!Ship.IsInvincible)
            {
                foreach (Mover mover in MoverManager.movers)
                {
                    Ship.CheckCollision(mover.pos, new Point(_bulletSprite.Width, _bulletSprite.Height));
                }
            }

            foreach (var bullet in _bullets)
            {
                bullet.Update(gameTime);
            }

            // Adjust zoom if the mouse wheel has moved
            if (InputHandler.ScrollUp())
                GameRef.Camera.Zoom += 0.1f;
            else if (InputHandler.ScrollDown())
                GameRef.Camera.Zoom -= 0.1f;

            // Move the camera when the arrow keys are pressed
            Vector2 movement = Vector2.Zero;
            Viewport vp = GameRef.GraphicsDevice.Viewport;

            if (InputHandler.KeyDown(Keys.Left))
                movement.X -= 0.1f;
            if (InputHandler.KeyDown(Keys.Right))
                movement.X += 0.1f;
            if (InputHandler.KeyDown(Keys.Up))
                movement.Y -= 0.1f;
            if (InputHandler.KeyDown(Keys.Down))
                movement.Y += 0.1f;

            GameRef.Camera.Pos += movement * 20;

            Ship.Update(gameTime);
            _enemy.Update(gameTime);
        }

        public override void Draw(GameTime gameTime)
        {
            ControlManager.Draw(GameRef.SpriteBatch);

            GameRef.SpriteBatch.Begin(SpriteSortMode.Deferred,
                BlendState.AlphaBlend,
                SamplerState.PointClamp,
                null,
                null,
                null,
                GameRef.Camera.GetTransformation());

            /*
            Random rand = new Random();

            foreach (Texture2D texture in _logos)
            {
                GameRef.SpriteBatch.Draw(texture, new Vector2(
                    rand.Next(50, (GameRef.Graphics.GraphicsDevice.Viewport.Width) - (texture.Width / 2)),
                    rand.Next(50, (GameRef.Graphics.GraphicsDevice.Viewport.Height) - (texture.Height / 2))), Color.White);
            }
            */
            /*
            GameRef.SpriteBatch.Draw(_logos[0], new Vector2(
                    (GameRef.Graphics.GraphicsDevice.Viewport.Width / 2) - (_logos[0].Width / 2),
                    0), Color.White);
            */

            _enemy.Draw(gameTime);
            Ship.Draw(gameTime);

            foreach (var bullet in _bullets)
            {
                bullet.Draw(gameTime);
            }

            foreach (Mover mover in MoverManager.movers)
            {
                GameRef.SpriteBatch.Draw(_bulletSprite,
                    new Vector2(
                        mover.pos.X - _bulletSprite.Width / 2,
                        mover.pos.Y - _bulletSprite.Height / 2),
                        Color.White);

                if (Config.DisplayCollisionBoxes)
                {
                    Rectangle bulletRectangle = new Rectangle(
                        (int)mover.pos.X - _bulletSprite.Width / 2,
                        (int)mover.pos.Y - _bulletSprite.Height / 2,
                        _bulletSprite.Width,
                        _bulletSprite.Height);
                    GameRef.SpriteBatch.Draw(DnK._pixel, bulletRectangle, Color.White);
                }
            }

            // Text
            GameRef.SpriteBatch.DrawString(ControlManager.SpriteFont, "Bullets: " + MoverManager.movers.Count.ToString(), new Vector2(1, 21), Color.Black);
            GameRef.SpriteBatch.DrawString(ControlManager.SpriteFont, "Bullets: " + MoverManager.movers.Count.ToString(), new Vector2(0, 20), Color.White);

            GameRef.SpriteBatch.End();

            base.Draw(gameTime);
        }
    }
}
