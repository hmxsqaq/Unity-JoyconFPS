using System.Collections.Generic;
using FPS;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Hmxs.Scripts.UI
{
	public class JoyconPointer : MonoBehaviour
	{
		[SerializeField] private Canvas canvas;
		[SerializeField] private float xSensitivity = 1.0f;
		[SerializeField] private float ySensitivity = 1.0f;
		[SerializeField] private InputSetting inputSetting;
		[SerializeField] private Color normalColor = Color.white;
		[SerializeField] private Color selectedColor = Color.red;

		private Vector2 _sceneSize;

		[ShowInInspector, ReadOnly] private Button _selectedButton;
		private Button selectedButton
		{
			get => _selectedButton;
			set
			{
				if (_selectedButton == value) return;
				if (_selectedButton)
					ExecuteEvents.Execute(_selectedButton.gameObject, new PointerEventData(EventSystem.current), ExecuteEvents.pointerExitHandler);
				if (value)
					ExecuteEvents.Execute(value.gameObject, new PointerEventData(EventSystem.current), ExecuteEvents.pointerEnterHandler);
				_selectedButton = value;
			}
		}

		private Camera _camera;
		private GraphicRaycaster _raycaster;
		private Image _image;

		private void Start()
		{
			_sceneSize = new Vector2(Screen.width, Screen.height);
			_camera = Camera.main;
			_raycaster = canvas.GetComponent<GraphicRaycaster>();
			_image = GetComponent<Image>();
		}

		private void Update()
		{
			// move pointer
			var gyro = JoyconInput.instance.GetGyro(0);
			var delta = new Vector3(gyro.z * xSensitivity, gyro.y * ySensitivity, 0);
			transform.position += delta;
			ClampPosition();

			// recenter
			if (JoyconInput.instance.GetButtonDown(inputSetting.recenterButtonDesc))
				transform.position = _sceneSize / 2;

			// check if pointer is over button
			var pointerEventData = new PointerEventData(EventSystem.current)
			{
				position = transform.position
			};
			var results = new List<RaycastResult>();
			_raycaster.Raycast(pointerEventData, results);
			foreach (var result in results)
				selectedButton = result.gameObject.GetComponent<Button>();

			// select
			if (JoyconInput.instance.GetButtonDown(inputSetting.fireButtonDesc) && selectedButton)
			{
				ExecuteEvents.Execute(selectedButton.gameObject, new PointerEventData(EventSystem.current), ExecuteEvents.pointerDownHandler);
				selectedButton.onClick?.Invoke();
			}
			if (JoyconInput.instance.GetButtonUp(inputSetting.fireButtonDesc) && selectedButton)
				ExecuteEvents.Execute(selectedButton.gameObject, new PointerEventData(EventSystem.current), ExecuteEvents.pointerUpHandler);

			_image.color = selectedButton ? selectedColor : normalColor;
		}

		private void ClampPosition()
		{
			var position = transform.position;
			position.x = Mathf.Clamp(position.x, 0, _sceneSize.x);
			position.y = Mathf.Clamp(position.y, 0, _sceneSize.y);
			transform.position = position;
		}
	}
}
