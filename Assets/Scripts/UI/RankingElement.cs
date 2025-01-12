using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Hmxs.Scripts.UI
{
	public class RankingElement : MonoBehaviour
	{
		[SerializeField] private TextMeshProUGUI numberText;
		[SerializeField] private TextMeshProUGUI timeSpanText;

		public void Set(int number, TimeSpan timeSpan)
		{
			numberText.text = number.ToString();
			timeSpanText.text = timeSpan.ToString("mm':'ss'.'ff");
		}
	}
}
