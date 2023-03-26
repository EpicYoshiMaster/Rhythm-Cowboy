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
	[SerializeField] AudioSource gunShot;
	[SerializeField] AudioSource titleScreenMusic;
	public Pattern[] PatternList;

	private GameObject titleScreen;
	private GameObject gameOverScreen;
	private NoteManager noteManager;

	private SongConductor Cond;
	//private GameState CurrentState = GameState.GS_Idle;
	
	[SerializeField] int Lives = 3;

	public int HitCount;
	public int MissCount;

	public static GameManager Instance;

    // Start is called before the first frame update
    void Start()
    {
		Instance = this;
		Cond = SongConductor.Instance;
		noteManager = NoteManager.Instance;
		titleScreen = GameObject.FindGameObjectWithTag("Titlescreen");
		gameOverScreen = GameObject.FindGameObjectWithTag("Game Over");
		
		SetTitlescreenVisible(true);
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

	public void OnHitNote() {
		HitCount++;
		gunShot.Play();
	}

	public void OnMissNote() {
		//Lives--;
		MissCount++;

		if(Lives <= 0) {
			OnLoseGame();
		}
	}

	void OnLoseGame() {
		Cond.StopSong();
		gameOverScreen.SetActive(true);
	}

	float GetMeasureOffsetForNote(Note noteTime) {
		return (float)(((noteTime.Beat - 1) * Cond.GetBeatLength()) + (0.25 * (noteTime.Sixteenth - 1) * Cond.GetBeatLength()));
	}

    // Update is called once per frame
    void Update()
    {
		if(Cond == null) {
			Cond = SongConductor.Instance;
		}

		if(noteManager == null) {
			noteManager = NoteManager.Instance;
		}

		if(Input.anyKeyDown) {
			if(gameOverScreen.activeInHierarchy) {
				SetTitlescreenVisible(true);
				return;
			}

			if(!Cond.IsCurrentlyPlaying()) {
				SetTitlescreenVisible(false);

				Cond.StartSong();
				noteManager.SetIsPlaying(true);
			}
			else {
				noteManager.OnPlayerInput();
			}
		}
    }
}