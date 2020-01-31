using BaseLibrary;
using BaseLibrary.Input;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Runtime.InteropServices;
using TChromiumBackend;
using Terraria;
using Terraria.ModLoader;

namespace TChromium
{
	public class TLayer : Layer
	{
		private BrowserAPI browser;

		public override bool Enabled => Main.instance.IsActive && Main.hasFocus;

		public TLayer()
		{
			Dispatcher.Dispatch(() =>
			{
				browser = new BrowserAPI();
				browser.Paint += Paint;
				browser.OnPageLoaded += PageLoaded;
			});

			Main.instance.Exiting += Instance_Exiting;
			On.Terraria.Main.DrawMenu += Draw;
		}

		private void Draw(On.Terraria.Main.orig_DrawMenu orig, Main self, GameTime gameTime)
		{
			orig(self, gameTime);

			OnDraw(Main.spriteBatch);
		}

		private void PageLoaded()
		{
			if (browser.URL.Contains("discordapp.com"))
			{
				browser.ExecuteJavascript(@"
												var x = document.getElementsByClassName('wrapper-1BJsBx');
												var s='';
												for(var i = 0; i < x.length; i++)
												{
													s+=x[i].getAttribute('aria-label')+' ';
												}

											");
			}
		}

		public override void OnMouseMove(MouseMoveEventArgs args)
		{
			browser.Focus();

			browser.MouseMove((int)(args.Position.X - 10), (int)(args.Position.Y - 10));

			args.Handled = true;
		}

		public override void OnMouseScroll(MouseScrollEventArgs args)
		{
			browser.MouseScroll((int)(args.Position.X - 10), (int)(args.Position.Y - 10), (int)args.OffsetX, (int)args.OffsetY);

			args.Handled = true;
		}

		public override void OnMouseDown(MouseButtonEventArgs args)
		{
			posX = (int)(args.Position.X - 10);
			posY = (int)(args.Position.Y - 10);
			browser.MouseDown(posX, posY, (int)args.Button);

			args.Handled = true;
		}

		private int posX;
		private int posY;

		public override void OnMouseUp(MouseButtonEventArgs args)
		{
			browser.MouseUp(posX, posY, (int)args.Button);

			args.Handled = true;
		}

		private bool block;

		public override void OnKeyPressed(KeyboardEventArgs args)
		{
			block = true;
			browser.KeyDown((int)args.Key);

			args.Handled = true;
		}

		public override void OnKeyReleased(KeyboardEventArgs args)
		{
			browser.KeyUp((int)args.Key);

			args.Handled = true;
		}

		public override void OnKeyTyped(KeyboardEventArgs args)
		{
			if (args.Key == Keys.Left || args.Key == Keys.Right || args.Key == Keys.Back || args.Key == Keys.Delete)
			{
				if (block)
				{
					block = false;
					return;
				}

				browser.KeyDown((int)args.Key);
			}
			else
			{
				if (args.Character != null) browser.KeyTyped(args.Character.Value);
			}

			args.Handled = true;
		}

		public override void OnDraw(SpriteBatch spriteBatch)
		{
			if (texture != null && !texture.IsDisposed)
			{
				Main.spriteBatch.End();
				Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.AnisotropicClamp, DepthStencilState.None, null, TChromium.effect);
				Main.spriteBatch.Draw(texture, new Vector2(10), Color.White);
				Main.spriteBatch.End();
				Main.spriteBatch.Begin();
			}
		}

		public override void OnWindowResize(WindowResizedEventArgs args)
		{
			browser.SetSize((int)args.Width / 2, (int)args.Height / 2);
		}

		private void Instance_Exiting(object sender, EventArgs e)
		{
			browser.Shutdown();
		}

		private Texture2D texture;

		private byte[] arr;

		private void Paint(IntPtr buffer, int width, int height)
		{
			try
			{
				//Debug.WriteLine($"Redraw - width: {width}, height: {height}, buffer: {buffer}");

				int bytes = width * height * 4;

				if (arr == null || bytes != arr.Length)
				{
					arr = new byte[bytes];
					texture?.Dispose();
				}

				Marshal.Copy(buffer, arr, 0, bytes);

				if (texture == null || texture.IsDisposed) texture = new Texture2D(Main.graphics.GraphicsDevice, width, height);

				Main.graphics.GraphicsDevice.Textures[0] = null;
				texture.SetData(arr);
			}
			catch
			{
			}
		}
	}

	public class TChromium : Mod
	{
		internal static Effect effect;

		public override void Load()
		{
			effect = GetEffect("Effects/BGRtoRGB");

			BaseLibrary.BaseLibrary.Layers.PushOverlay(new TLayer());
		}
	}
}