using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;

using Craftplacer.Library.Windows;

using HazdryxEngine.Drawing;

using InputManager;

namespace LayerBot
{
	internal class Program
	{
		private const int canvasX = 588;
		private const int canvasY = 383;
		private static int canvasWidth = 500;
		private static int canvasHeight = 500;
		private const int colorX = 659;
		private const int colorY = 409;
		private const int hueX = 618;
		private const int hueY = 464;
		private const int hueLength = 315;
		private const int brightnessX = 658;
		private const int brightnessY = 454;
		private const int brightnessLength = 328;
		private const int delay = 25;
		private const int colorDelay = 50;

		private const int padding = 100;

		private static int lastHueY = 0;
		private static int lastBrightnessY = 0;

		private static Dictionary<Color, List<(int, int)>> pixels = new Dictionary<Color, List<(int, int)>>();
		private static int width;
		private static int height;

		private static void Main(string[] args)
		{
			var img = Image.FromFile(@"A:\Desktop\doubt.png");
			var bmp = new FastBitmap(img);

			width = bmp.Width;
			height = bmp.Height;

			for (int y = 0; y < img.Height; y++)
			{
				for (int x = 0; x < img.Width; x++)
				{
					var color = bmp[x, y];
					if (!pixels.ContainsKey(color))
					{
						pixels[color] = new List<(int, int)>();
					}

					pixels[color].Add((x, y));

					//Console.Title = Text;
				}
			}

			Color currentColor = Color.Transparent;

			canvasWidth -= padding;
			canvasHeight -= padding;

			int lastX = 0;
			int lastY = 0;

			Thread.Sleep(2500);

			foreach (var item in pixels)
			{
				var color = item.Key;
				var positions = item.Value;

				if (color.A == 0)
					continue;

				if (color != currentColor)
					ChangeColor(currentColor = color);

				foreach (var pos in positions)
				{
					if (!Text.Contains("Layer")) return;

					float posX = (float)(pos.Item1) / (float)width;
					float posY = (float)(pos.Item2) / (float)height;

					int mouseX = (int)Math.Ceiling(canvasX + ((float)canvasWidth * posX));
					int mouseY = (int)Math.Ceiling(canvasY + padding + ((float)canvasHeight * posY));

					if (mouseX == lastX && mouseY == lastY) continue;

					//Console.Title = $"{mouseX},{mouseY}";

					Debug.Assert(mouseY < 1080);
					Mouse.Move(mouseX, mouseY);
					Mouse.PressButton(Mouse.MouseKeys.Left, delay);

					lastX = mouseX;
					lastY = mouseY;
				}
			}
		}

		public static string Text
		{
			get
			{
				StringBuilder builder = new StringBuilder(256);
				GetWindowText(Window.ForegroundWindow.Handle, builder, builder.Capacity);
				return builder.ToString();
			}
		}

		private static void ChangeColor(Color c)
		{
			Mouse.Move(colorX, colorY);
			Mouse.PressButton(Mouse.MouseKeys.Left);

			float h = c.GetHue() / 360.0f;
			float b = c.GetBrightness();
			float s = c.GetSaturation();

			var hy = 0;

			if (s < 0.075f || (c.R + c.G + c.B) <= (3 * 4))
			{
				hy = hueY - 2;
			}
			else
			{
				Debug.Assert(h < 1f);

				const int end = hueY + hueLength;

				hy = (int)Math.Ceiling(end - (hueLength * h));

				if (end < hy)
				{
					hy = end;
				}
			}

			if (hy != lastHueY)
			{
				Debug.Assert(hy < 1080);
				Mouse.Move(hueX, hy);
				Mouse.ButtonDown(Mouse.MouseKeys.Left);
				Thread.Sleep(colorDelay);
				Mouse.ButtonUp(Mouse.MouseKeys.Left);
				lastHueY = hy;
			}

			float brightnessValue;

			if (0.5f <= b)
			{
				brightnessValue = 1f;
				brightnessValue -= s * 0.5f;
			}
			else
			{
				brightnessValue = b * 0.5f;
			}

			int by = (int)Math.Floor(brightnessY + (brightnessLength - (brightnessLength * brightnessValue)));

			Debug.WriteLine($"S{Math.Round(s, 2)} * B{Math.Round(b, 2)} = V{Math.Round(brightnessValue, 2)}");

			if (by != lastBrightnessY)
			{
				Debug.Assert(by < 1080);
				Mouse.Move(brightnessX, by);

				Mouse.ButtonDown(Mouse.MouseKeys.Left);
				Thread.Sleep(colorDelay);
				Mouse.ButtonUp(Mouse.MouseKeys.Left);

				lastBrightnessY = by;
			}

			Mouse.Move(colorX, colorY);
			Mouse.PressButton(Mouse.MouseKeys.Left);
		}

		private static float invert(float input)
		{
			return 1 - (input * -1);
		}

		[DllImport("user32.dll")]
		public static extern int GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount);
	}
}