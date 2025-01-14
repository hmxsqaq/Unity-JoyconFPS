using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Hmxs.Scripts.JoyconTest
{
	public class JoyconGyroTest : MonoBehaviour
	{
		[Title("Settings")]
		[InfoBox("Left:0 Right:1")]
		[SerializeField] private int joyconIndex;
		[SerializeField] private float rotationSensitivity = 50.0f;
		// [SerializeField] private float tiltSensitivity = 1.0f;
		[SerializeField] private float smoothing = 5f;

		[Title("Info")]
		[SerializeField, ReadOnly] private float[] stick;
		[SerializeField, ReadOnly] private Vector3 gyro;
		[SerializeField, ReadOnly] private Vector3 accel;
		[SerializeField, ReadOnly] private Quaternion orientation;

		private List<Joycon> _joycons = new();
		private Vector3 _smoothGyro;
		private Vector3 _currentRotation;
		private Vector3 _initialRotation;

		private void Start()
		{
			_joycons = JoyconManager.instance.Joycons;
			if (_joycons.Count < joyconIndex + 1) Destroy(gameObject); // Destroy redundant object
			_initialRotation = transform.rotation.eulerAngles;
			_currentRotation = transform.rotation.eulerAngles;
		}

		private void Update()
		{
			if (_joycons.Count <= 0) return;
			Joycon joycon = _joycons[joyconIndex];

			// Get Input Info
			stick = joycon.GetStick();
			gyro = joycon.GetGyro();
			accel = joycon.GetAccel();
			orientation = joycon.GetVector();

			if (joycon.GetButtonDown(Joycon.Button.SHOULDER_2))
			{
				Debug.Log("Recenter");
				transform.rotation = Quaternion.Euler(_initialRotation);
				_currentRotation = _initialRotation;
				return;
			}

			_smoothGyro = Vector3.Lerp(_smoothGyro, gyro, Time.deltaTime * smoothing);
			_currentRotation += _smoothGyro * (rotationSensitivity * Time.deltaTime);

			transform.rotation = Quaternion.Euler(
				-_currentRotation.y,
				_currentRotation.z,
				-_currentRotation.x
			);
		}
	}
}
