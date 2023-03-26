using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using TMPro;

public class NoteManager : MonoBehaviour
{
	[SerializeField] float NoteSuccessRadius = 0.1f;
	[SerializeField] float NoteFailRadius = 0.2f;
	[SerializeField] Pattern[] PatternList;
	[SerializeField] Pattern[] EasyPatterns;
	[SerializeField] Pattern[] NormalPatterns;
	[SerializeField] Pattern[] HardPatterns;
	[SerializeField] int NormalStartWave = 11;
	[SerializeField] int HardStartWave = 21;
	[SerializeField] int FreeStartWave = 31;
	[SerializeField] float ShowSpeechBubbleTime = 2.0f;
	[SerializeField] GameObject[] SpeechBubbles;
	private Pattern CurrentPattern;
	private List<Note> AllNotes = new List<Note>();

	private GameManager gameManager;
	private SongConductor Cond;
	private BottleManager bottleManager;
	public bool IsPlaying = false;
	public static NoteManager Instance;

	private GameObject Cowboy;
	private Vector3 cowboyOriginalPos;

	private TextMeshProUGUI instructionsText;
	[HideInInspector] public int WaveNumber = 0;
	private bool MissedANote = true;
	private float SpeechBubbleDuration = 0.0f;

	public void SetIsPlaying(bool bIsPlaying) {
		IsPlaying = bIsPlaying;

		WaveNumber = 0;

		MissedANote = true;

		HideSpeechBubble();
	}

	public void CleanUpAll() {
		for(int i = 0; i < AllNotes.Count; i++) {
			if(AllNotes[i].bottle != null) {
				Destroy(AllNotes[i].bottle);
			}
		}

		AllNotes.Clear();
	}

	public void CleanUpPlayedNotes() {
		for(int i = 0; i < AllNotes.Count; i++) {
			if(!AllNotes[i].HasBeenHit) break;

			if(AllNotes[i].bottle != null) {
				Destroy(AllNotes[i].bottle);

				AllNotes.RemoveAt(i);
				i--;
			}
		}
	}

	public void QueuePattern(Pattern pattern) {
		for(int i = 0; i < pattern.Beats.Length; i++) {
			Note newNote = new Note();

			newNote.Beat = pattern.Beats[i].Beat;
			newNote.Sixteenth = pattern.Beats[i].Sixteenth;
			newNote.PlaceMeasure = Cond.MeasureNumber;

			AllNotes.Add(newNote);
		}
	}

	public void OnPlayerInput() {
		for(int i = 0; i < AllNotes.Count; i++) {
			if(AllNotes[i].HasBeenHit) continue;
			if(!AllNotes[i].HasBeenPlaced) continue;

			if(IsNoteInRadius(AllNotes[i], NoteSuccessRadius)) {
				AllNotes[i].HasBeenHit = true;
				bottleManager.HitBottle(AllNotes[i].bottle);

				gameManager.OnHitNote();

				return;
			}

			if(IsNoteInRadius(AllNotes[i], NoteFailRadius)) {
				AllNotes[i].HasBeenHit = true;
				bottleManager.MissBottle(AllNotes[i].bottle);

				gameManager.OnPlayerMissNote();

				MissedANote = true;

				return;
			}

			break;
		}
	}

	Pattern GetNextPattern() {
		int FreeWaveRandom = Random.Range(0, 3);

		if(WaveNumber < NormalStartWave) {
			return EasyPatterns[Random.Range(0, EasyPatterns.Length)];
		}

		if(WaveNumber < HardStartWave) {
			return NormalPatterns[Random.Range(0, NormalPatterns.Length)];
		}

		if(WaveNumber < FreeStartWave) {
			return HardPatterns[Random.Range(0, HardPatterns.Length)];
		}

		switch(FreeWaveRandom) {
			case 0: return EasyPatterns[Random.Range(0, EasyPatterns.Length)];
			case 1: return NormalPatterns[Random.Range(0, NormalPatterns.Length)];
			case 2: return HardPatterns[Random.Range(0, HardPatterns.Length)];
			default: return HardPatterns[Random.Range(0, HardPatterns.Length)];
		}
	}

	//If bGameplay is true, return the gameplay time, not the placement time
	float GetNoteTime(Note currNote, bool bGameplay) {
		float NoteTime = (float)(((currNote.Beat - 1) * Cond.GetBeatLength()) + (0.25 * (currNote.Sixteenth - 1) * Cond.GetBeatLength()));

		NoteTime += (bGameplay ? (currNote.PlaceMeasure * Cond.GetMeasureLength()) : ((currNote.PlaceMeasure - 1) * Cond.GetMeasureLength()));

		return NoteTime;
	}

	bool IsNoteInRadius(Note currNote, float Radius) {
		float NoteTime = GetNoteTime(currNote, true);

		return (Cond.SongPosition >= NoteTime - Radius && Cond.SongPosition <= NoteTime + Radius);
	}

    // Start is called before the first frame update
    void Start()
    {
		gameManager = GameManager.Instance;
        Cond = SongConductor.Instance;
		bottleManager = BottleManager.instance;
		Instance = this;

		Cowboy = GameObject.FindGameObjectWithTag("Cowboy");
		cowboyOriginalPos = Cowboy.transform.position;

		instructionsText = GameObject.FindGameObjectWithTag("Instructions").GetComponent<TextMeshProUGUI>();
    }

    // Update is called once per frame
    void Update()
    {
		if(Cond == null) {
			Cond = SongConductor.Instance;
		}

		if(gameManager == null) {
			gameManager = GameManager.Instance;
		}

		if(bottleManager == null) {
			bottleManager = BottleManager.instance;
		}

		if(SpeechBubbleDuration > 0) {
			SpeechBubbleDuration -= Time.deltaTime;

			if(SpeechBubbleDuration <= 0) {
				HideSpeechBubble();
			}
		}

        for(int i = 0; i < AllNotes.Count; i++) {
			if(AllNotes[i].HasBeenHit) continue;

			//Check for expired notes
			if(AllNotes[i].HasBeenPlaced) {
				if(Cond.SongPosition >= GetNoteTime(AllNotes[i], true) && !IsNoteInRadius(AllNotes[i], NoteFailRadius)) {
					AllNotes[i].HasBeenHit = true;
					bottleManager.MissBottle(AllNotes[i].bottle);

					gameManager.OnMissNote();

					MissedANote = true;
				}

				continue;
			}

			if(Cond.SongPosition >= GetNoteTime(AllNotes[i], false)) {
				AllNotes[i].HasBeenPlaced = true;
				AllNotes[i].bottle = bottleManager.PlaceBottle(AllNotes[i]);
			}
		}
    }

	void ShowSpeechBubble() {
		if(SpeechBubbles.Length <= 0) return;

		SpeechBubbleDuration = ShowSpeechBubbleTime;

		int RandomSpeechBubble = Random.Range(0, SpeechBubbles.Length);

		SpeechBubbles[RandomSpeechBubble].SetActive(true);
	}

	void HideSpeechBubble() {
		for(int i = 0; i < SpeechBubbles.Length; i++) {
			SpeechBubbles[i].SetActive(false);
		}
	}

	public void OnMeasure(int Measure) {
		if(!IsPlaying) {
			instructionsText.SetText("Wait...");
			return;
		}

		if(Measure % 2 != 0) {
			CleanUpPlayedNotes();

			QueuePattern(GetNextPattern());

			instructionsText.SetText("Listen...");

			if(!MissedANote) {
				ShowSpeechBubble();
			}

			WaveNumber++;
			gameManager.SetLevelNumber(WaveNumber);

			MissedANote = false;
		}
		else {
			if(AllNotes.Count > 0) {
				instructionsText.SetText("Play!");
			}
			
			Cowboy.transform.position = cowboyOriginalPos;
		}
	}
}

[System.Serializable]
public class Pattern {
	public Note[] Beats;
}

[System.Serializable]
public class Note {
	[Range(1, 4)]
	public int Beat = 1;
	[Range(1, 4)]
	public int Sixteenth = 1;
	[HideInInspector] public int PlaceMeasure;
	[HideInInspector] public bool HasBeenPlaced = false;
	[HideInInspector] public bool HasBeenHit = false;
	[HideInInspector] public GameObject bottle;
}
