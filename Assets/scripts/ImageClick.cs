using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class ImageClick : MonoBehaviour, IPointerClickHandler
{
	public int i;
	public GameObject button;

	public void OnPointerClick (PointerEventData eventData)
	{
		var script = button.GetComponentInChildren<ExitScript> ();
		script.NextLevelButton (i);
	}
}
