using Hmxs.Scripts;
using Sirenix.OdinInspector;
using UnityEngine;

namespace FPS
{
	[CreateAssetMenu(menuName = "FPS/InputSetting", fileName = "InputSetting", order = 0)]
	public class InputSetting : ScriptableObject
	{
		[Title("Character")]
		public JoyconButtonDescription jumpButtonDesc = new() { joyconIndex = 0, button = Joycon.Button.DPAD_DOWN };
		public JoyconButtonDescription recenterButtonDesc = new() { joyconIndex = 0, button = Joycon.Button.STICK };
		public JoyconButtonDescription runButtonDesc = new() { joyconIndex = 0, button = Joycon.Button.DPAD_LEFT };
		[EnumToggleButtons] public RotateType rotateType = RotateType.Orientation;
		
		[Title("Gun")]
		public JoyconButtonDescription fireButton = new(){ joyconIndex = 0, button = Joycon.Button.SHOULDER_2 };
		public JoyconButtonDescription reloadButton = new(){ joyconIndex = 0, button = Joycon.Button.DPAD_UP };
		public JoyconButtonDescription aimButton = new(){ joyconIndex = 0, button = Joycon.Button.SHOULDER_1 };
		public JoyconButtonDescription throwGrenadeButton = new(){ joyconIndex = 0, button = Joycon.Button.DPAD_RIGHT };
		public JoyconButtonDescription inspectButton = new(){ joyconIndex = 0, button = Joycon.Button.CAPTURE };
	}

	public enum RotateType
	{
		Orientation,
		Gyro
	}
}
