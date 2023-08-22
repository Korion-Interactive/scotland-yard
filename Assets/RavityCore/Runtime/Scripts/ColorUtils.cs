using UnityEngine;

namespace Ravity
{
	public static class ColorUtils
	{
		public static Color CreateColor(byte red, byte green, byte blue, float alpha = 1.0f)
		{
			return new Color(red / 255.0f, green / 255.0f, blue / 255.0f,alpha);
		}
		
		public static Color CreateColor(byte red, byte green, byte blue, byte alpha)
		{
			return CreateColor(red, green, blue, alpha / 255.0f);
		}

		public static Color CreateGray(float gray, float alpha = 1.0f)
		{
			return new Color(gray, gray, gray,alpha);
		}

		public static Color CreateWhite(float alpha)
		{
			return new Color(1f, 1f, 1f,alpha);
		}

		public static Color CopyWithAlpha(this Color color, float alpha)
		{
			color.a = alpha;
			return color;
		}
	}
}
