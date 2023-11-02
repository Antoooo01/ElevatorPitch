using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class FloorPicker : MonoBehaviour
{
	[SerializeField]
	private GameObject _buttonPrefab;

	private Elevator _elevator;

	public void SetElevator(Elevator elevator)
	{
		_elevator = elevator;
	}

	public void MakeButtons(int floorCount)
	{
		for(int i = 0; i < floorCount; i++)
		{
			GameObject newButton = Instantiate(_buttonPrefab, transform);
			newButton.GetComponentInChildren<TextMeshProUGUI>().text = "Floor " + i.ToString();
			newButton.GetComponent<FloorPickerButton>().floor = i;
		}
	}

	public void FloorButtonClick(int floorValue)
	{
		//Debug.Log("Floor " + floorValue + " has had its button clicked");
		_elevator.PickFloor(floorValue);
	}

	public void SetActive(bool status)
	{
		CanvasGroup canvasGroup = GetComponent<CanvasGroup>();

        if (status)
		{
			canvasGroup.alpha = 1.0f;
			canvasGroup.interactable = true;
		}
		else
		{
            canvasGroup.alpha = 0f;
			canvasGroup.interactable = false;

        }
	}
}
