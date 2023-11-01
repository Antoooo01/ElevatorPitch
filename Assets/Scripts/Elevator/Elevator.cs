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

public class Elevator : MonoBehaviour
{
	public float moveSpeed;
	[SerializeField] private int _currentFloor = 0;
	[SerializeField] private List<ElevatorPoint> _movePoints;


	private ElevatorQueueEvent _queueEvent = new ElevatorQueueEvent();

	private eElevatorState _state = eElevatorState.STATIONARY;
	private bool _currentDirection = true; //could be replaced with an enum for readability
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
				break;
			case eElevatorState.MOVING:

				if (AtTarget())
				{
					_currentFloor = _targetFloor;

					RemoveTarget();
					_waitTimer = 5; //Todo: make variable instead of hardcoded
					_state = eElevatorState.WAITING;
					break;
				}

				Move();

				break;
			case eElevatorState.WAITING:

				if (_waitTimer > 0)
				{
					_waitTimer -= Time.deltaTime;
					break;
				}

				TargetLogic();
				print("timer done");
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
		bool floorFound = false;
		if (_currentDirection)
		{
			if (_currentFloor + 1 < _movePoints.Count)
			{
				AddQueuePoint(_movePoints[_currentFloor + 1]);
			}
		}
		else if (!_currentDirection)
		{
			if (_currentFloor - 1 > 0)
			{
				AddQueuePoint(_movePoints[_currentFloor - 1]);
			}
		}

		if (floorFound)
			return;

		_currentDirection = !_currentDirection;

		if (_currentDirection)
		{
			if (_currentFloor + 1 < _movePoints.Count)
			{
				AddQueuePoint(_movePoints[_currentFloor + 1]);
			}
		}
		else if (!_currentDirection)
		{
			if (_currentFloor - 1 > 0)
			{
				AddQueuePoint(_movePoints[_currentFloor - 1]);
			}
		}
	}

	private void AddQueuePoint(ElevatorPoint point)
	{
		if (_movePoints.IndexOf(point) < _targetFloor)
		{
			if (!_queuePointsBelow.Contains(point))
			{
				_queuePointsBelow.Add(point);
				_queuePointsBelow.Sort((x, y) => y.index.CompareTo(x.index));
				TargetLogic();
			}
		}
		else
		{
			if (!_queuePointsAbove.Contains(point))
			{
				_queuePointsAbove.Add(point);
				_queuePointsAbove.Sort((x, y) => x.index.CompareTo(y.index));
				TargetLogic();
			}
		}
	}

	private void Move()
	{
		transform.position += VectorToTarget().normalized * moveSpeed * Time.deltaTime;
	}

	private void TargetLogic()
	{
		//Todo: make this very mush more readable
		bool targetFound = false;

		if(_currentDirection && _queuePointsAbove.Count > 0)
		{
			_targetFloor = _movePoints.IndexOf(_queuePointsAbove[0]);
			targetFound = true;
		}
		else if (!_currentDirection && _queuePointsBelow.Count > 0)
		{
			_targetFloor = _movePoints.IndexOf(_queuePointsBelow[0]);
			targetFound = true;
		}

		if (targetFound)
		{
			_state = eElevatorState.MOVING;
			return;
		}

		_currentDirection = !_currentDirection;

		if (_currentDirection && _queuePointsAbove.Count > 0)
		{
			_targetFloor = _movePoints.IndexOf(_queuePointsAbove[0]);
			targetFound = true;
		}
		else if (!_currentDirection && _queuePointsBelow.Count > 0)
		{
			_targetFloor = _movePoints.IndexOf(_queuePointsBelow[0]);
			targetFound = true;
		}

		if (targetFound)
		{
			_state = eElevatorState.MOVING;
			return;
		}

		_state = eElevatorState.STATIONARY;
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
}
