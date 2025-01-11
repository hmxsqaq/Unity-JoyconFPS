using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Hmxs.Scripts.JoyconTest
{
	public class JoyconOrientationTest : MonoBehaviour
	{
		[Title("Settings")]
		[InfoBox("Based on connecting order"), SerializeField]
		private int joyconIndex;
		[SerializeField] private float rotationSensitivity = 1.0f;

		[Title("Info")]
		[SerializeField, ReadOnly] private float[] stick;
		[SerializeField, ReadOnly] private Vector3 gyro;
		[SerializeField, ReadOnly] private Vector3 accel;
		[SerializeField, ReadOnly] private Quaternion orientation;

		private List<Joycon> _joycons = new();

		private void Start()
		{
			_joycons = JoyconManager.instance.Joycons;
			if (_joycons.Count < joyconIndex + 1) Destroy(gameObject); // Destroy redundant object
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
				joycon.Recenter();
				return;
			}

			var smoothOrientation = Quaternion.Lerp(transform.rotation, orientation, rotationSensitivity);
			smoothOrientation = RemoveRoll(smoothOrientation);
			transform.rotation = smoothOrientation;
		}

		public static Quaternion RemoveRoll(Quaternion q)
		{
			Vector3 forward = q * Vector3.forward;
			return Quaternion.LookRotation(forward, Vector3.up);
		}
	}
}
