using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bottle : MonoBehaviour
{
	//Show

	//Hide

	//Explode/Break

	public void Explode() {
		Animator anim = GetComponent<Animator>();

		anim.enabled = true;
	}
}
