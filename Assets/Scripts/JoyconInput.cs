using System;
using System.Collections.Generic;
using Hmxs.Scripts.Utility;
using UnityEngine;

namespace Hmxs.Scripts
{
	public class JoyconInput : SingletonMono<JoyconInput>
	{
		private class ButtonState
		{
			private bool _current;

			public bool current
			{
				get => _current;
				set
				{
					down = value && !_current;
					up = !value && _current;
					_current = value;
				}
			}
			public bool down { get; private set; }
			public bool up { get; private set; }
		}

		private List<Joycon> _joycons = new();

		private List<List<ButtonState>> _buttonStates;

		private void Start()
		{
			_joycons = JoyconManager.instance.Joycons;
			if (_joycons.Count <= 0)
			{
				Debug.LogWarning("No Joycon Found");
				return;
			}
			_buttonStates = new List<List<ButtonState>>();
			for (int i = 0; i < _joycons.Count; i++)
			{
				_buttonStates.Add(new List<ButtonState>());
				foreach (Joycon.Button _ in Enum.GetValues(typeof(Joycon.Button)))
					_buttonStates[i].Add(new ButtonState());
			}
		}

		private void Update()
		{
			if (_joycons.Count <= 0) return;
			for (int i = 0; i < _joycons.Count; i++)
			{
				Joycon joycon = _joycons[i];
				foreach (Joycon.Button button in Enum.GetValues(typeof(Joycon.Button)))
				{
					bool state = joycon.GetButton(button);
					_buttonStates[i][(int)button].current = state;
				}
			}
		}

		public bool IsIndexValid(int index) => index >= 0 && index < _joycons.Count;

		public bool GetButton(JoyconButtonDescription buttonDescription)
		{
			int index = buttonDescription.joyconIndex;
			int button = (int)buttonDescription.button;
			if (index >= 0 && index < _joycons.Count)
				return _buttonStates[index][button].current;
			Debug.LogWarning($"Joycon Index Error: {index}");
			return false;
		}

		public bool GetButtonDown(JoyconButtonDescription buttonDescription)
		{
			int index = buttonDescription.joyconIndex;
			int button = (int)buttonDescription.button;
			if (index >= 0 && index < _joycons.Count)
				return _buttonStates[index][(int)button].down;
			Debug.LogWarning($"Joycon Index Error: {index}");
			return false;
		}

		public bool GetButtonUp(JoyconButtonDescription buttonDescription)
		{
			int index = buttonDescription.joyconIndex;
			int button = (int)buttonDescription.button;
			if (index >= 0 && index < _joycons.Count)
				return _buttonStates[index][(int)button].up;
			Debug.LogWarning($"Joycon Index Error: {index}");
			return false;
		}

		public float[] GetStick(int index)
		{
			if (index >= 0 && index < _joycons.Count)
				return _joycons[index].GetStick();
			Debug.LogWarning($"Joycon Index Error: {index}");
			return new float[2];
		}

		public Vector3 GetGyro(int index)
		{
			if (index >= 0 && index < _joycons.Count)
				return _joycons[index].GetGyro();
			Debug.LogWarning($"Joycon Index Error: {index}");
			return Vector3.zero;
		}

		public Vector3 GetAccel(int index)
		{
			if (index >= 0 && index < _joycons.Count)
				return _joycons[index].GetAccel();
			Debug.LogWarning($"Joycon Index Error: {index}");
			return Vector3.zero;
		}

		public Quaternion GetOrientation(int index)
		{
			if (index >= 0 && index < _joycons.Count)
				return _joycons[index].GetVector();
			Debug.LogWarning($"Joycon Index Error: {index}");
			return Quaternion.identity;
		}

		public void Recenter(int index)
		{
			if (index >= 0 && index < _joycons.Count)
			{
				_joycons[index].Recenter();
				return;
			}
			Debug.LogWarning($"Joycon Index Error: {index}");
		}

		public void SetRumble(int index, float lowFreq, float highFreq, float amp, int time = 0)
		{
			if (index >= 0 && index < _joycons.Count)
			{
				_joycons[index].SetRumble(lowFreq, highFreq, amp, time);
				return;
			}
			Debug.LogWarning($"Joycon Index Error: {index}");
		}
	}
}
