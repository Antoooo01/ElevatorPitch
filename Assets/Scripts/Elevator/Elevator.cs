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
	[SerializeField, Tooltip("Speed of elevator")]
	private float _moveSpeed = 3;
	[SerializeField, Tooltip("Time until elevator starts after entering")]
	private float _waitStart = 2;
	[SerializeField, Tooltip("Time until elevator continues after reaching its current target")]
	private float _waitContinue = 5;
	[SerializeField, Tooltip("Signifies the floor it starts on")]
	private int _currentFloor;
	[SerializeField, Tooltip("Custom list of points the elevator travels between\nOrder is significant")]
	private List<ElevatorPoint> _movePoints;


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
			_movePoints[i].queueEvent = _queueEvent;
			_movePoints[i].index = i;
		}
	}

	// Update is called once per frame
	void Update()
	{
		switch (_state)
		{
			case eElevatorState.STATIONARY:
				//if (_queuePointsAbove.Count > 0 || _queuePointsBelow.Count > 0)
				//{
				//	//TargetLogic();
				//	_waitTimer = 2;
				//	_state = eElevatorState.WAITING;
				//}

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

				print("timer done");

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
			//print("Entered");

			SimpleFloorPick();
		}
	}

	private void OnTriggerExit(Collider other)
	{
		if (other.CompareTag("Player"))
		{
			//print("Exited");
		}
	}

	private void SimpleFloorPick()
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
		if (point.index < _targetFloor)
		{
			if (!_queuePointsBelow.Contains(point))
			{
				_queuePointsBelow.Add(point);
				_queuePointsBelow.Sort((x, y) => y.index.CompareTo(x.index));
				SetValidTarget();
			}
		}
		else
		{
			if (!_queuePointsAbove.Contains(point))
			{
				_queuePointsAbove.Add(point);
				_queuePointsAbove.Sort((x, y) => x.index.CompareTo(y.index));
				SetValidTarget();
			}
		}
	}

	private void Move()
	{
		//TODO: remake to keep currentfloor accurate, and one floor at a time

		//todo: per floor basis instead
		transform.position += VectorToTarget().normalized * _moveSpeed * Time.deltaTime;

		if (AtTarget())
		{
			_currentFloor = _targetFloor; //make independant of target

			RemoveTarget();
			_waitTimer = _waitContinue;
			_state = eElevatorState.WAITING;
		}

	}

	private bool SetValidTarget()
	{
		//Todo: make this very much more readable
		bool targetFound = false;

		if(_currentDirection == eDirection.UP && _queuePointsAbove.Count > 0)
		{
			_targetFloor = _queuePointsAbove[0].index;
			targetFound = true;
		}
		else if (_currentDirection == eDirection.DOWN && _queuePointsBelow.Count > 0)
		{
			_targetFloor = _queuePointsBelow[0].index;
			targetFound = true;
		}

		if (targetFound)
		{
			if (_state == eElevatorState.STATIONARY)
			{ 
				_waitTimer = _waitStart;
				_state = eElevatorState.WAITING;
			}
			return true;
		}

		FlipDirection();

		if (_currentDirection == eDirection.UP && _queuePointsAbove.Count > 0)
		{
			_targetFloor = _queuePointsAbove[0].index;
			targetFound = true;
		}
		else if (_currentDirection == eDirection.DOWN && _queuePointsBelow.Count > 0)
		{
			_targetFloor = _queuePointsBelow[0].index;
			targetFound = true;
		}

		if (targetFound)
		{
			if (_state == eElevatorState.STATIONARY)
			{
				_waitTimer = _waitStart;
				_state = eElevatorState.WAITING;
			}
			return true;
		}

		FlipDirection();

		_state = eElevatorState.STATIONARY;
		return false;
	}

	private bool AtTarget()
	{
		return VectorToTarget().magnitude < 0.01;
	}

	private Vector3 VectorToTarget()
	{
		return (_movePoints[_targetFloor].transform.position - transform.position);
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
