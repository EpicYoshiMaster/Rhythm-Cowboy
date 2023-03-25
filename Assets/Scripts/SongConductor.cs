using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class SongConductor : MonoBehaviour
{
	[SerializeField] float BPM = 120;
	[SerializeField] int BeatsInMeasure = 4;
	[SerializeField] float StartOffset = 0.0f;

	public int MeasureNumber;
	public int BeatNumber;
	public int LoopNumber;
	public float LastBeat;

	private bool IsPlaying = false;
	[SerializeField] AudioSource SongPlayer;
	public float Beat;
	public float SongStartTime;
	public int PreviousTimeSamples;
	public int TotalSampleCount;
	public float SongPosition;

	public static SongConductor Instance;

	public UnityEvent<int> trigger;

    // Start is called before the first frame update
    void Start()
    {
        SongPlayer = GetComponent<AudioSource>();
		Beat = (60 / BPM) * (4 / BeatsInMeasure);

		Instance = this;

		StartSong();
    }

	public void SetBPM(float NewBPM) {
		BPM = NewBPM;
		Beat = (60 / BPM) * (4 / BeatsInMeasure);
	}

	void StartSong() {
		SetBPM(BPM);
		SongPlayer.Play();

		MeasureNumber = 1;
		BeatNumber = 1;
		LoopNumber = 0;
		LastBeat = 0;

		IsPlaying = true;

		PreviousTimeSamples = -1;
		TotalSampleCount = SongPlayer.timeSamples;

		SongPosition = ((float)TotalSampleCount / SongPlayer.clip.frequency) - StartOffset;
		SongStartTime = SongPosition;

		Debug.Log("Initial Value: " + SongPosition);

		trigger.Invoke(BeatNumber);
	}

    // Update is called once per frame
    void Update()
    {
		if(!IsPlaying) return;

		if(SongPlayer.timeSamples < PreviousTimeSamples) {
			LoopNumber++;
		}

		PreviousTimeSamples = SongPlayer.timeSamples;

		TotalSampleCount = (LoopNumber * SongPlayer.clip.samples) + SongPlayer.timeSamples;
        
		SongPosition = ((float)TotalSampleCount / SongPlayer.clip.frequency) - StartOffset;

		while(SongPosition > LastBeat + Beat) {
			Debug.Log("New Beat with SampleFreq:" + SongPosition + ", LastBeat + Beat: " + (LastBeat + Beat));

			LastBeat += Beat;
			BeatNumber++;

			while(BeatNumber >= BeatsInMeasure + 1) {
				MeasureNumber++;
				BeatNumber -= BeatsInMeasure;
			}

			trigger.Invoke(BeatNumber);
		}
    }
}
