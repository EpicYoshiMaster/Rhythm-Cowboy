using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
	public enum GameState {
		GS_Idle,
		GS_Placing,
		GS_Shooting
	};

	[SerializeField] GameObject[] bottlePrefab;
	[SerializeField] AudioSource gunShot;
	[SerializeField] AudioSource titleScreenMusic;
	public Pattern[] PatternList;

	[SerializeField] float placementRadius = 10;
	[SerializeField] float NoteSuccessRadius = 0.1f;
	private int GameplayMeasure;

	private Pattern CurrentPattern;
	private int CurrentIndex;
	private GameObject titleScreen;
	private GameObject gameOverScreen;

	private SongConductor Cond;
	private GameState CurrentState = GameState.GS_Idle;
	private List<GameObject> spawnedBottles = new List<GameObject>();
	[SerializeField] int Lives = 3;

	public int HitCount;
	public int MissCount;

    // Start is called before the first frame update
    void Start()
    {
		Cond = SongConductor.Instance;
		titleScreen = GameObject.FindGameObjectWithTag("Titlescreen");
		gameOverScreen = GameObject.FindGameObjectWithTag("Game Over");
		
		SetTitlescreenVisible(true);

		//DoNextPattern();
		CurrentPattern = GetNextPattern();
		CurrentIndex = 0;
    }

	void SetTitlescreenVisible(bool bVisible) {
		if(bVisible) {
			titleScreen.SetActive(true);
			titleScreenMusic.Play();
		}
		else {
			titleScreen.SetActive(false);
			titleScreenMusic.Stop();
		}

		gameOverScreen.SetActive(false);
	}

	void OnLoseGame() {
		Cond.StopSong();
		gameOverScreen.SetActive(true);
	}

	void PlaceBottle(Note noteTime) {
		if(bottlePrefab.Length <= 0) return;

		GameObject summonBottle = bottlePrefab[Random.Range(0, bottlePrefab.Length)];

		spawnedBottles.Add(Instantiate(summonBottle, new Vector3(GetBottleXLocation(noteTime), 0, 0), summonBottle.transform.rotation));
	}

	//start of range is beat 1 sixteenth 1
	//End of range is beat sixteenth 4

	float GetBottleXLocation(Note noteTime) {
		//0 to 1 value representing how far on the range we should end up
		float percentage = 0.25f * (noteTime.Beat - 1) + 0.0625f * (noteTime.Sixteenth - 1);

		float x = Mathf.Lerp(-placementRadius, placementRadius, percentage);

		return x;
	}

	Pattern GetNextPattern() {
		CurrentIndex = 0;

		return PatternList[Random.Range(0, PatternList.Length)];
	}

	float GetMeasureOffsetForNote(Note noteTime) {
		return (float)(((noteTime.Beat - 1) * Cond.GetBeatLength()) + (0.25 * (noteTime.Sixteenth - 1) * Cond.GetBeatLength()));
	}

	bool IsInRangeOfNote(Note noteTime) {
		float NoteTime = GetMeasureOffsetForNote(noteTime);

		//If we're currently placing
		//Then we've pressed early, so adjust for the next measure
		//If we're shooting, this is the measure for the note
		NoteTime += (CurrentState == GameState.GS_Placing) ? (GameplayMeasure) * Cond.GetMeasureLength() : (GameplayMeasure - 1) * Cond.GetMeasureLength();

		Debug.Log("Hit Time: " + Cond.SongPosition + ", Note Time: " + NoteTime + "Difference: " + Mathf.Abs(Cond.SongPosition - NoteTime));

		if(Cond.SongPosition >= NoteTime - NoteSuccessRadius && Cond.SongPosition <= NoteTime + NoteSuccessRadius) {
			return true;
		}

		return false;
	}

	bool IsNoteExpired(Note noteTime) {
		float NoteTime = (GameplayMeasure - 1) * Cond.GetMeasureLength() + GetMeasureOffsetForNote(noteTime);

		return Cond.SongPosition > NoteTime + NoteSuccessRadius;
	}

	Note GetCurrentNote() {
		if(CurrentIndex < CurrentPattern.Beats.Length) {
			return CurrentPattern.Beats[CurrentIndex];
		}

		return null;
	}

    // Update is called once per frame
    void Update()
    {
		if(Input.anyKeyDown) {
			if(gameOverScreen.activeInHierarchy) {
				SetTitlescreenVisible(true);
				return;
			}

			if(!Cond.IsCurrentlyPlaying()) {
				SetTitlescreenVisible(false);

				Cond.StartSong();

				CurrentState = GameState.GS_Placing;
			}
			else {
				Note currentNote;

				if(CurrentState == GameState.GS_Placing) {
					currentNote = CurrentPattern.Beats[0];
				}
				else {
					currentNote = GetCurrentNote();
				}

				if(currentNote != null && IsInRangeOfNote(currentNote)) {
					HitCount++;

					gunShot.Play();

					Bottle bottleScript = spawnedBottles[CurrentIndex].GetComponent<Bottle>();
					bottleScript.Explode();

					CurrentIndex++;
				}
			}
		}

		if(CurrentState == GameState.GS_Placing) {
			Note nextNote = GetCurrentNote();

			if(nextNote != null && Cond.SongPosition > ((GameplayMeasure - 1) * Cond.GetMeasureLength()) + GetMeasureOffsetForNote(nextNote)) {
				PlaceBottle(nextNote);
				CurrentIndex++;
			}
		}
		else if(CurrentState == GameState.GS_Shooting) {
			Note currentNote = GetCurrentNote();

			if(currentNote != null && IsNoteExpired(currentNote)) {
				MissCount++;
				Lives--;

				if(Lives <= 0) {
					OnLoseGame();
				}

				Bottle bottleScript = spawnedBottles[CurrentIndex].GetComponent<Bottle>();
				bottleScript.Explode();
				CurrentIndex++;
			}
		}
    }

	public void OnMeasure(int Measure) {
		GameplayMeasure = Measure;

		if(CurrentState == GameState.GS_Placing) {
			CurrentState = GameState.GS_Shooting;
			CurrentIndex = 0;
		}
		else if(CurrentState == GameState.GS_Shooting) {

			CurrentState = GameState.GS_Placing;
			CurrentPattern = GetNextPattern();
			CurrentIndex = 0;

			foreach(GameObject bottle in spawnedBottles) {
				if(bottle != null) {
					Bottle bottleScript = bottle.GetComponent<Bottle>();
					bottleScript.Explode();
				}
			}

			spawnedBottles.Clear();
		}
	}

	public void OnBeat(int Measure, int Beat) {

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
}