using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Hmxs.Scripts.UI
{
	public class JoyconButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
	{
		[SerializeField] private Color normalColor;
		[SerializeField] private Color selectedColor;
		[SerializeField] private AudioClip selectSound;

		private Text _text;
		private Button _button;

		private void Start()
		{
			_text = GetComponentInChildren<Text>();
			_button = GetComponent<Button>();
			_button.onClick.AddListener(() =>
			{
				Debug.Log($"Button {name} clicked");
				if (selectSound)
					AudioSource.PlayClipAtPoint(selectSound, new Vector3(0, 0, 0));
			});
		}

		public void OnPointerEnter(PointerEventData eventData) => _text.color = selectedColor;
		public void OnPointerExit(PointerEventData eventData) => _text.color = normalColor;
	}
}
