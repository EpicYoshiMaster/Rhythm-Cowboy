using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BottleManager : MonoBehaviour
{
	[SerializeField] GameObject[] bottlePrefab;
	[SerializeField] float placementRadius = 10;
	public static BottleManager instance;

	public GameObject PlaceBottle(Note noteTime) {
		if(bottlePrefab.Length <= 0) return null;

		GameObject summonBottle = bottlePrefab[Random.Range(0, bottlePrefab.Length)];

		return Instantiate(summonBottle, new Vector3(GetBottleXLocation(noteTime), 0, 0), summonBottle.transform.rotation);
	}

	public void HitBottle(GameObject bottle) {
		if(bottle == null) return;

		Bottle bottleScript = bottle.GetComponent<Bottle>();
		bottleScript.Explode();
	}

	public void MissBottle(GameObject bottle) {
		if(bottle == null) return;

		Bottle bottleScript = bottle.GetComponent<Bottle>();
	}

	float GetBottleXLocation(Note noteTime) {
		//0 to 1 value representing how far on the range we should end up
		float percentage = 0.25f * (noteTime.Beat - 1) + 0.0625f * (noteTime.Sixteenth - 1);

		float x = Mathf.Lerp(-placementRadius, placementRadius, percentage);

		return x;
	}

    // Start is called before the first frame update
    void Start()
    {
        instance = this;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
