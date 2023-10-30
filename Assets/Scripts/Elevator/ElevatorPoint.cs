using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

[System.Serializable]
public class ElevatorPoint : MonoBehaviour
{
	private ElevatorQueueEvent _queueEvent;
    public ElevatorQueueEvent queueEvent
    { set { _queueEvent = value; } }
    private int _index;
	public int index
	{ get { return _index; } set { _index = value; } }

	//test purposes
	//public void Start()
	//{
	//	_queueEvent.Invoke(this);
	//}
}