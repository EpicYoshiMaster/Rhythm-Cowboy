using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class NoteManager : MonoBehaviour
{
	[SerializeField] float NoteSuccessRadius = 0.1f;
	[SerializeField] float NoteFailRadius = 0.2f;
	[SerializeField] Pattern[] PatternList;

	private Pattern CurrentPattern;
	private List<Note> AllNotes = new List<Note>();

	private GameManager gameManager;
	private SongConductor Cond;
	private BottleManager bottleManager;
	public bool IsPlaying = false;
	public static NoteManager Instance;

	public void SetIsPlaying(bool bIsPlaying) {
		IsPlaying = bIsPlaying;
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

			Debug.Log("Hit Time: " + Cond.SongPosition + ", Note Time: " + GetNoteTime(AllNotes[i], true));

			if(IsNoteInRadius(AllNotes[i], NoteSuccessRadius)) {
				AllNotes[i].HasBeenHit = true;
				bottleManager.HitBottle(AllNotes[i].bottle);

				Debug.Log("Hit");

				gameManager.OnHitNote();

				return;
			}

			if(IsNoteInRadius(AllNotes[i], NoteFailRadius)) {
				AllNotes[i].HasBeenHit = true;
				bottleManager.MissBottle(AllNotes[i].bottle);

				Debug.Log("Player Miss");

				gameManager.OnMissNote();

				return;
			}

			break;
		}
	}

	Pattern GetNextPattern() {
		return PatternList[Random.Range(0, PatternList.Length)];
	}

	//If bGameplay is true, return the gameplay time, not the placement time
	float GetNoteTime(Note currNote, bool bGameplay) {
		float NoteTime = (float)(((currNote.Beat - 1) * Cond.GetBeatLength()) + (0.25 * (currNote.Sixteenth - 1) * Cond.GetBeatLength()));

		NoteTime += (bGameplay ? (currNote.PlaceMeasure * Cond.GetMeasureLength()) : ((currNote.PlaceMeasure - 1) * Cond.GetMeasureLength()));

		return NoteTime;
	}

	bool IsNoteInRadius(Note currNote, float Radius) {
		float NoteTime = GetNoteTime(currNote, true);

		//Debug.Log("Hit Time: " + Cond.SongPosition + ", Note Time: " + NoteTime + "Difference: " + Mathf.Abs(Cond.SongPosition - NoteTime));

		return (Cond.SongPosition >= NoteTime - Radius && Cond.SongPosition <= NoteTime + Radius);
	}

    // Start is called before the first frame update
    void Start()
    {
		gameManager = GameManager.Instance;
        Cond = SongConductor.Instance;
		bottleManager = BottleManager.instance;
		Instance = this;
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

        for(int i = 0; i < AllNotes.Count; i++) {
			if(AllNotes[i].HasBeenHit) continue;

			//Check for expired notes
			if(AllNotes[i].HasBeenPlaced) {
				if(Cond.SongPosition >= GetNoteTime(AllNotes[i], true) && !IsNoteInRadius(AllNotes[i], NoteFailRadius)) {
					AllNotes[i].HasBeenHit = true;
					bottleManager.MissBottle(AllNotes[i].bottle);

					gameManager.OnMissNote();

					Debug.Log("Miss");
				}

				continue;
			}

			if(Cond.SongPosition >= GetNoteTime(AllNotes[i], false)) {
				AllNotes[i].HasBeenPlaced = true;
				AllNotes[i].bottle = bottleManager.PlaceBottle(AllNotes[i]);
			}
		}
    }

	public void OnMeasure(int Measure) {
		if(!IsPlaying) return;

		if(Measure % 2 != 0) {
			CleanUpPlayedNotes();

			QueuePattern(GetNextPattern());
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
