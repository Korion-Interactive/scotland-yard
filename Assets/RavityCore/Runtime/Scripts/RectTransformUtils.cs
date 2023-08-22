using UnityEngine;
// ReSharper disable UnusedMember.Global

namespace Ravity
{
	public static class RectTransformUtils
	{
		public static RectTransform GetRectTransform(this GameObject gameObject) => (RectTransform) gameObject.transform;
		public static RectTransform GetRectTransform(this Component component) => (RectTransform) component.transform;

		#region Anchored Position
		public static void SetAnchoredPosition(Behaviour behaviour, float x, float y)
		{
			RectTransform rectTransform = (RectTransform) behaviour.transform;
			rectTransform.anchoredPosition = new Vector2(x,y);
		}

		public static void SetAnchoredPositionX(Behaviour behaviour, float x)
		{
			RectTransform rectTransform = (RectTransform) behaviour.transform;
			SetAnchoredPositionX(rectTransform, x);
		}

		public static void SetAnchoredPositionX(RectTransform rectTransform, float x)
		{
			Vector2 anchoredPosition = rectTransform.anchoredPosition;
			anchoredPosition.x = x;
			rectTransform.anchoredPosition = anchoredPosition;
		}

		public static void SetAnchoredPositionY(Behaviour behaviour, float y)
		{
			RectTransform rectTransform = (RectTransform) behaviour.transform;
			SetAnchoredPositionY(rectTransform, y);
		}

		public static void SetAnchoredPositionY(RectTransform rectTransform, float y)
		{
			Vector2 anchoredPosition = rectTransform.anchoredPosition;
			anchoredPosition.y = y;
			rectTransform.anchoredPosition = anchoredPosition;
		}

		#endregion

		#region Left, Right, Top and  Bottom
		public static void SetLeftRightTopBottom(GameObject gameObject, float left, float top, float right, float bottom)
		{
			RectTransform rectTransform =  (RectTransform) gameObject.transform;
			SetLeftRightTopBottom(rectTransform, left, right, top, bottom);
		}

		public static void SetLeftRightTopBottom(Behaviour behaviour, float left, float right, float top, float bottom)
		{
			RectTransform rectTransform = (RectTransform) behaviour.transform;
			SetLeftRightTopBottom(rectTransform, left, right, top, bottom);
		}

		public static void SetLeftRightTopBottom(RectTransform rectTransform, float left, float right, float top, float bottom)
		{
			rectTransform.offsetMin = new Vector2(left,bottom);
			rectTransform.offsetMax = new Vector2(-right,-top);
		}

		public static void SetLeftRight(Behaviour behaviour, float left, float right)
		{
			RectTransform rectTransform =  (RectTransform) behaviour.transform;
			SetLeftRight(rectTransform, left, right);
		}

		public static void SetLeftRight(GameObject gameObject, float left, float right)
		{
			RectTransform rectTransform = (RectTransform) gameObject.transform;
			SetLeftRight(rectTransform, left, right);
		}

		public static void SetLeftRight(RectTransform rectTransform, float left, float right)
		{
			SetLeft(rectTransform,left);
			SetRight(rectTransform, right);
		}

		public static void SetLeft(RectTransform rectTransform, float left)
		{
			rectTransform.offsetMin = new Vector2(left,rectTransform.offsetMin.y);
		}

		public static void SetRight(RectTransform rectTransform, float right)
		{
			rectTransform.offsetMax = new Vector2(-right,rectTransform.offsetMax.y);
		}

		public static void SetTopBottom(Behaviour behaviour, float top, float bottom)
		{
			RectTransform rectTransform = (RectTransform) behaviour.transform;
			SetTopBottom(rectTransform, top, bottom);
		}

		public static void SetTopBottom(RectTransform rectTransform, float top,  float bottom)
		{
			SetTop(rectTransform, top);
			SetBottom(rectTransform, bottom);
		}

		public static void SetTop(Behaviour behaviour, float top)
		{
			RectTransform rectTransform = (RectTransform) behaviour.transform;
			SetTop(rectTransform, top);
		}

		public static void SetTop(RectTransform rectTransform, float top)
		{
			rectTransform.offsetMax = new Vector2(rectTransform.offsetMax.x, -top);
		}

		public static void SetBottom(Behaviour behaviour, float bottom)
		{
			RectTransform rectTransform = (RectTransform) behaviour.transform;
			SetBottom(rectTransform, bottom);
		}

		public static void SetBottom(RectTransform rectTransform, float bottom)
		{
			rectTransform.offsetMin = new Vector2(rectTransform.offsetMin.x, bottom);
		}
		#endregion

		#region Size Delta
		public static void SetSizeDeltaWidth(Behaviour behaviour, float width)
		{
			RectTransform rectTransform =  (RectTransform) behaviour.transform;
			SetSizeDeltaWidth(rectTransform, width);
		}
		
		public static void SetSizeDeltaWidth(GameObject gameObject, float width)
		{
			RectTransform rectTransform =  (RectTransform) gameObject.transform;
			SetSizeDeltaWidth(rectTransform, width);
		}

		public static void SetSizeDeltaWidth(RectTransform rectTransform, float width)
		{
			Vector2 sizeDelta = rectTransform.sizeDelta;
			sizeDelta.x = width;
			rectTransform.sizeDelta = sizeDelta;
		}

		public static void SetSizeDeltaHeight(Behaviour behaviour, float height)
		{
			RectTransform rectTransform = (RectTransform) behaviour.transform;
			SetSizeDeltaHeight(rectTransform, height);
		}

		public static void SetSizeDeltaHeight(GameObject gameObject, float height)
		{
			RectTransform rectTransform = (RectTransform) gameObject.transform;
			SetSizeDeltaHeight(rectTransform, height);
		}
		
		public static void SetSizeDeltaHeight(RectTransform rectTransform, float height)
		{
			Vector2 sizeDelta = rectTransform.sizeDelta;
			sizeDelta.y = height;
			rectTransform.sizeDelta = sizeDelta;
		}
		#endregion
	}
}
