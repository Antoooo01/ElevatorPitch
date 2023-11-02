using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

[System.Serializable]
public class ElevatorPoint : MonoBehaviour
{
	private ElevatorQueueEvent _queueEvent;
	public ElevatorQueueEvent QueueEvent
	{ set { _queueEvent = value; } }
	private int _index;
	public int Index
	{ get { return _index; } set { _index = value; } }

	private void OnTriggerEnter(Collider other)
	{
		CallElevator();
	}

	private void CallElevator()
	{
		_queueEvent.Invoke(this);
	}
}
