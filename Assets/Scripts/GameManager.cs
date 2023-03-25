using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
	//Need to define call and response mechanism

	//1 Measure of call
	//1 Measure of response as a set

	//Only need pure 1 measure information
	//Beat Number + 16th?

	public int Number;
	public Note[] Pattern;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

[System.Serializable]
public class Note {
	public Note(int beat, int sixteenth) {
		Beat = beat;
		Sixteenth = sixteenth;
	}
	public int Beat;
	public int Sixteenth;
}