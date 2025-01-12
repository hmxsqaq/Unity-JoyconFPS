using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using System;
using Hmxs.Scripts.Utility;

public class JoyconManager : SingletonMono<JoyconManager>
{
	// Settings accessible via Unity
	public bool enableImu = true;
	public bool enableLocalize = true;

	// Different operating systems either do or don't like the trailing zero
	private const ushort vendorID = 0x57e;
	private const ushort vendorID2 = 0x057e;
	private const ushort productL = 0x2006;
	private const ushort productR = 0x2007;

	public List<Joycon> Joycons; // Array of all connected Joy-Cons

	protected override void Awake()
	{
		Joycons = new List<Joycon>();
		bool isLeft = false;
		HIDapi.hid_init();

		IntPtr ptr = HIDapi.hid_enumerate(vendorID, 0x0);
		IntPtr topPtr = ptr;

		if (ptr == IntPtr.Zero)
		{
			ptr = HIDapi.hid_enumerate(vendorID2, 0x0);
			if (ptr == IntPtr.Zero)
			{
				HIDapi.hid_free_enumeration(ptr);
				Debug.Log("No Joy-Cons found!");
			}
		}

		while (ptr != IntPtr.Zero)
		{
			var enumerate = (hid_device_info)Marshal.PtrToStructure(ptr, typeof(hid_device_info));

			Debug.Log(enumerate.product_id);
			if (enumerate.product_id is productL or productR)
			{
				switch (enumerate.product_id)
				{
					case productL:
						isLeft = true;
						Debug.Log("Left Joy-Con connected.");
						break;
					case productR:
						isLeft = false;
						Debug.Log("Right Joy-Con connected.");
						break;
					default:
						Debug.Log("Non Joy-Con input device skipped.");
						break;
				}

				IntPtr handle = HIDapi.hid_open_path(enumerate.path);
				HIDapi.hid_set_nonblocking(handle, 1);
				Joycons.Add(new Joycon(handle, enableImu, enableLocalize & enableImu, 0.05f, isLeft));
			}

			ptr = enumerate.next;
		}

		HIDapi.hid_free_enumeration(topPtr);
	}

	private void Start()
	{
		for (int i = 0; i < Joycons.Count; ++i)
		{
			Debug.Log(i);
			Joycon jc = Joycons[i];
			byte LEDs = 0x0;
			LEDs |= (byte)(0x1 << i);
			jc.Attach(leds_: LEDs);
			jc.Begin();
		}
	}

	private void Update()
	{
		foreach (var joycon in Joycons) joycon.Update();
	}

	protected override void OnDestroy()
	{
		foreach (var joycon in Joycons) joycon.Detach();
		base.OnDestroy();
	}
}
