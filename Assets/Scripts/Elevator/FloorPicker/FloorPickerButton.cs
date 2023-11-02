using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class FloorPickerButton : MonoBehaviour
{
	private int _floor;
	public int floor
	{ set => _floor = value; }

	private FloorPicker _floorPicker;

	private void OnEnable()
	{
		_floorPicker = GetComponentInParent<FloorPicker>();
	}

	public void SendFloorValue()
	{
		_floorPicker.FloorButtonClick(_floor);
	}
}
