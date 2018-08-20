using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.UI;

public class UI : MonoBehaviour
{
	public RawImage colorBlock;
	private List<RawImage> images = new List<RawImage> ();
	private Hashtable colors = new Hashtable ();
	// Use this for initialization
	void Start ()
	{
	}
	
	// Update is called once per frame
	void Update ()
	{
		var dots = GameObject.FindGameObjectsWithTag ("Dot");
		int oldSize = colors.Count;
		colors.Clear ();
		for (int i = 0; i < dots.Length; i++) {
			var color = dots [i].GetComponent<SpriteRenderer> ().color;
			if (!colors.ContainsValue (color) && dots[i].GetComponent<Collider2D>().isTrigger) {
				colors.Add (i, color);
			}
		}
		if (oldSize != colors.Count) {
			refreshUI ();
		}
	}

	private void refreshUI ()
	{
		for (int i = 0; i < images.Count; i++) {
			Destroy (images [i].gameObject);
		}
		images.Clear ();
		for (int i = 0; i < colors.Count; i++) {
			Color color = (Color)colors [i];
			RawImage image = Instantiate (colorBlock);
			images.Add (image);
			var rectTransform = image.GetComponent<RectTransform> ();
			image.GetComponent<RectTransform> ().SetParent (gameObject.transform);
			rectTransform.anchoredPosition = new Vector2 (0, 0);
			rectTransform.position += Vector3.right * i * 100;
			image.color = color;
		}
	}

}
