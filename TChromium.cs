using BaseLibrary;
using BaseLibrary.Input;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Terraria;
using Terraria.ModLoader;
using test;

namespace TChromium
{
	public class TLayer : Layer
	{
		private CefSharp browser;

		public override bool Enabled => true;

		public TLayer()
		{
			browser = new CefSharp();
			CefSharp.Paint += CefSharp_Paint;
			Main.instance.Exiting += Instance_Exiting;
			On.Terraria.Main.DrawCursor += Main_DrawCursor;
		}

		private void Main_DrawCursor(On.Terraria.Main.orig_DrawCursor orig, Vector2 bonus, bool smart)
		{
			OnDraw(Main.spriteBatch);

			orig(bonus, smart);
		}

		public override void OnMouseMove(MouseMoveEventArgs args)
		{
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
			browser.MouseDown(posX, posY);

			args.Handled = true;
		}

		private int posX;
		private int posY;

		public override void OnMouseUp(MouseButtonEventArgs args)
		{
			browser.MouseUp(posX, posY);

			args.Handled = true;
		}

		public override void OnDraw(SpriteBatch spriteBatch)
		{
			if (texture != null)
			{
				Main.spriteBatch.End();
				Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.AnisotropicClamp, DepthStencilState.None, null, TChromium.effect);
				Main.spriteBatch.Draw(texture, new Vector2(10), Color.White);
				Main.spriteBatch.End();
				Main.spriteBatch.Begin();
			}
		}

		private void Instance_Exiting(object sender, EventArgs e)
		{
			browser.Shutdown();
		}

		private Texture2D texture;

		private byte[] arr;

		private void CefSharp_Paint(IntPtr buffer, int width, int height)
		{
			Debug.WriteLine($"Redraw - width: {width}, height: {height}, buffer: {buffer}");

			if (arr == null) arr = new byte[width * height * 4];
			Marshal.Copy(buffer, arr, 0, width * height * 4);

			if (texture == null) texture = new Texture2D(Main.graphics.GraphicsDevice, width, height);

			Main.graphics.GraphicsDevice.Textures[0] = null;
			texture.SetData(arr);
		}
	}

	public class TChromium : Mod
	{
		internal static Effect effect;

		public override void Load()
		{
			effect = GetEffect("Effects/BGRtoRGB");

			BaseLibrary.BaseLibrary.Layers.PushLayer(new TLayer());
		}
	}
}