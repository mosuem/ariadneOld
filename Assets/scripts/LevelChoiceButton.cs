using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelChoiceButton
{

	public GameObject button;

	public GameObject deleteButton;

	public GameObject newImage;

	public LevelChoiceButton (GameObject newButton, GameObject deleteButton, GameObject newImage)
	{
		this.button = newButton;
		this.newImage = newImage;
		this.deleteButton = deleteButton;
	}

	public void Delete ()
	{
		GameObject.Destroy (button);
		GameObject.Destroy (newImage);
		GameObject.Destroy (deleteButton);
	}
}
