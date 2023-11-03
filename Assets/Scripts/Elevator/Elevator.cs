using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

enum eElevatorState
{
	STATIONARY = 0,
	MOVING = 1,
	WAITING = 2,
}

enum eDirection
{
	UP = 0,
	DOWN = 1,
}

public class Elevator : MonoBehaviour
{

    [SerializeField, Tooltip("UI to choose floors\nOptional")]
	private FloorPicker _floorPicker;
	[SerializeField, Tooltip("Speed of elevator")]
	private float _moveSpeed = 3;
	[SerializeField, Tooltip("Time until elevator starts after entering")]
	private float _waitStart = 2;
	[SerializeField, Tooltip("Time until elevator continues after reaching its current target")]
	private float _waitContinue = 5;
	[SerializeField, Tooltip("Signifies the floor it starts on")]
	private int _currentFloor;
    [SerializeField, Tooltip("How close the elevator needs to be to arrive\nLower is more accurate")]
    private float _distanceSensitivity = 0.01f;
	[SerializeField, Tooltip("Custom list of points the elevator travels between\nOrder is significant")]
	private List<ElevatorPoint> _movePoints;

	public int FloorCount
	{ get => _movePoints.Count; }
	


	private ElevatorQueueEvent _queueEvent = new ElevatorQueueEvent();

	private eElevatorState _state = eElevatorState.STATIONARY;
	private eDirection _currentDirection = eDirection.UP;
	private int _targetFloor;

	private List<ElevatorPoint> _queuePointsAbove = new List<ElevatorPoint>();
	private List<ElevatorPoint> _queuePointsBelow = new List<ElevatorPoint>();

	private float _waitTimer = 0;

	// Start is called before the first frame update
	void Start()
	{
		_queueEvent.AddListener(AddQueuePoint);

		for(int i = 0; i < _movePoints.Count; i++)
		{
			_movePoints[i].QueueEvent = _queueEvent;
			_movePoints[i].Index = i;
		}


		if (_floorPicker)
		{
			_floorPicker.SetElevator(this);
			_floorPicker.MakeButtons(FloorCount);
			_floorPicker.SetActive(false);
		}
	}

	// Update is called once per frame
	void Update()
	{
		switch (_state)
		{
			case eElevatorState.STATIONARY:
				break;
			case eElevatorState.MOVING:

				Move();

				break;
			case eElevatorState.WAITING:

				if (_waitTimer > 0)
				{
					_waitTimer -= Time.deltaTime;
					break;
				}

				//print("timer done");

				if (SetValidTarget())
				{
					_state = eElevatorState.MOVING;
				}
				else
				{ 
					_state = eElevatorState.STATIONARY;
				}

				break;
		}
		
	}

	private void OnTriggerEnter(Collider other)
	{
		if (other.CompareTag("Player"))
		{
			other.transform.SetParent(transform, true);

			//print("Entered");
			if (_floorPicker)
			{
				_floorPicker.SetActive(true);
				return;
			}

			SimplePickFloor();
		}
	}

	private void OnTriggerExit(Collider other)
	{
		if (other.CompareTag("Player"))
		{
			other.transform.SetParent(null, true);

			//print("Exited");
			if (_floorPicker)
				_floorPicker.SetActive(false);
		}
	}

	public void PickFloor(int floor)
	{
		AddQueuePoint(_movePoints[floor]);
	}

	private void SimplePickFloor()
	{
		if (_currentDirection == eDirection.UP)
		{
			if (_currentFloor + 1 < _movePoints.Count)
			{
				AddQueuePoint(_movePoints[_currentFloor + 1]);
				return;
			}
		}
		else if (_currentDirection == eDirection.DOWN)
		{
			if (_currentFloor - 1 >= 0)
			{
				AddQueuePoint(_movePoints[_currentFloor - 1]);
				return;
			}
		}

		FlipDirection();

		if (_currentDirection == eDirection.UP)
		{
			if (_currentFloor + 1 < _movePoints.Count)
			{
				AddQueuePoint(_movePoints[_currentFloor + 1]);
			}
		}
		else if (_currentDirection == eDirection.DOWN)
		{
			if (_currentFloor - 1 >= 0)
			{
				AddQueuePoint(_movePoints[_currentFloor - 1]);
			}
		}
	}

	private void AddQueuePoint(ElevatorPoint point)
	{
		if (IsBelow(point.Index))
		{
			if (!_queuePointsBelow.Contains(point))
			{
				_queuePointsBelow.Add(point);
				_queuePointsBelow.Sort((x, y) => y.Index.CompareTo(x.Index));
				SetValidTarget();
			}
		}
		else
		{
			if (!_queuePointsAbove.Contains(point))
			{
				_queuePointsAbove.Add(point);
				_queuePointsAbove.Sort((x, y) => x.Index.CompareTo(y.Index));
				SetValidTarget();
			}
		}
	}

	private bool IsBelow(int index)
	{
		if (_currentDirection == eDirection.UP)
		{
			return index <= _currentFloor;
		}
		else
		{
			return index < _currentFloor;
		}
	}

	private void Move()
	{
		if (AtTarget())
		{
			RemoveTarget();
			_waitTimer = _waitContinue;
			_state = eElevatorState.WAITING;
			return;
		}

		if (_currentDirection == eDirection.UP)
		{
			int nextfloor = _currentFloor + 1;
			transform.position += VectorBetweenFloors(_currentFloor, nextfloor).normalized * _moveSpeed * Time.deltaTime;

			if ((transform.position - _movePoints[nextfloor].transform.position).magnitude < _distanceSensitivity)
			{
				_currentFloor++;
			}
		}
		else
		{
			int nextfloor = _currentFloor - 1;
			transform.position += VectorBetweenFloors(_currentFloor, nextfloor).normalized * _moveSpeed * Time.deltaTime;

			if ((transform.position - _movePoints[nextfloor].transform.position).magnitude < _distanceSensitivity)
			{
				_currentFloor--;
			}
		}
	}

	private bool SetValidTarget()
	{
		//for loop to minimize repetetive code, since it might check both directions
		for (int i = 0; i < 2; i++)
		{
			if (TargetInCurrentDirection())
			{
				if (_state == eElevatorState.STATIONARY)
				{
					_waitTimer = _waitStart;
					_state = eElevatorState.WAITING;
				}
				return true;
			}

			FlipDirection();
		}

		_state = eElevatorState.STATIONARY;
		return false;
	}

	private bool TargetInCurrentDirection()
	{
		if (_currentDirection == eDirection.UP && _queuePointsAbove.Count > 0)
		{
			_targetFloor = _queuePointsAbove[0].Index;
			 return true;
		}
		else if (_currentDirection == eDirection.DOWN && _queuePointsBelow.Count > 0)
		{
			_targetFloor = _queuePointsBelow[0].Index;
			return true;
		}

		return false;
	}

	private bool AtTarget()
	{
		return VectorToTarget().magnitude < _distanceSensitivity;
	}

	private Vector3 VectorToTarget()
	{
		return VectorBetweenFloors(_currentFloor, _targetFloor);
	}

	private Vector3 VectorBetweenFloors(int floorStart, int floorEnd)
	{
		return _movePoints[floorEnd].transform.position - _movePoints[floorStart].transform.position;
	}

	private void RemoveTarget()
	{
		if (_queuePointsAbove.Contains(_movePoints[_targetFloor]))
			_queuePointsAbove.Remove(_movePoints[_targetFloor]);

		else if (_queuePointsBelow.Contains(_movePoints[_targetFloor]))
			_queuePointsBelow.Remove(_movePoints[_targetFloor]);
	}

	private void FlipDirection()
	{
		if (_currentDirection == eDirection.UP)
			_currentDirection = eDirection.DOWN;
		else
			_currentDirection = eDirection.UP;
	}
}
