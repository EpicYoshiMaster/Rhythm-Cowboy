using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class GameManager : MonoBehaviour
{
	public enum GameState {
		GS_Idle,
		GS_Placing,
		GS_Shooting
	};
	[SerializeField] AudioSource gunShot;
	[SerializeField] AudioSource gunMiss;
	[SerializeField] AudioSource titleScreenMusic;
	public Pattern[] PatternList;

	private GameObject titleScreen;
	private GameObject gameOverScreen;
	private GameObject gameplayStats;
	private NoteManager noteManager;
	private TextMeshProUGUI levelText;
	private TextMeshProUGUI bottlesShotText;
	private TextMeshProUGUI livesText;
	private TMP_InputField gameOverText;
	private SongConductor Cond;
	private List<GameObject> BeatPulsers = new List<GameObject>();

	[SerializeField] float DelayAcceptInput = 1.0f;
	//private GameState CurrentState = GameState.GS_Idle;
	
	[SerializeField] int Lives = 3;
	[SerializeField] float PulseAmount = 1.1f;
	[SerializeField] float ReturnSpeed = 8f;

	private float RemainingInputWait = 0.0f;

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
		gameplayStats = GameObject.FindGameObjectWithTag("Gameplay");

		levelText = GameObject.FindGameObjectWithTag("Level").GetComponent<TextMeshProUGUI>();
		bottlesShotText = GameObject.FindGameObjectWithTag("Bottles Shot").GetComponent<TextMeshProUGUI>();
		livesText = GameObject.FindGameObjectWithTag("Lives").GetComponent<TextMeshProUGUI>();
		gameOverText = GameObject.FindGameObjectWithTag("Game Over Text").GetComponent<TMP_InputField>();

		BeatPulsers.Add(GameObject.FindGameObjectWithTag("Level"));
		BeatPulsers.Add(GameObject.FindGameObjectWithTag("Bottles Shot"));
		BeatPulsers.Add(GameObject.FindGameObjectWithTag("Lives"));
		BeatPulsers.Add(GameObject.FindGameObjectWithTag("Instructions"));

		SetHitCount(0);
		SetLivesCount(3);
		
		SetTitlescreenVisible(true);
    }

	void SetTitlescreenVisible(bool bVisible) {
		if(bVisible) {
			titleScreen.SetActive(true);
			gameplayStats.SetActive(false);
			titleScreenMusic.Play();

			RemainingInputWait = DelayAcceptInput;
		}
		else {
			titleScreen.SetActive(false);
			titleScreenMusic.Stop();
		}

		gameOverScreen.SetActive(false);
	}

	public void OnHitNote() {
		SetHitCount(HitCount + 1);

		bottlesShotText.SetText("Bottles Shot: " + HitCount);

		gunShot.Play();
	}

	public void OnPlayerMissNote() {
		gunMiss.Play();

		OnMissNote();
	}

	public void OnMissNote() {
		SetLivesCount(Lives - 1);
		MissCount++;

		if(Lives <= 0) {
			OnLoseGame();
		}
	}

	public void SetLevelNumber(int Level) {
		levelText.SetText("Level: " + Level);
	}

	public void SetHitCount(int NewHitCount) {
		HitCount = NewHitCount;

		bottlesShotText.SetText("Bottles Shot: " + HitCount);
	}

	public void SetLivesCount(int NewLivesCount) {
		Lives = NewLivesCount;

		livesText.SetText("Lives Remaining: " + Lives);
	}

	public void SetGameOverStats(int Level, int NewHitCount) {
		gameOverText.text = "You reached Level " + Level + "!\nYou shot " + NewHitCount + " bottles!\nNice Job!\n\nPress Any Button to\nReturn to the Title Screen...";
	}

	void OnLoseGame() {
		Cond.StopSong();

		SetGameOverStats(noteManager.WaveNumber, HitCount);

		noteManager.CleanUpAll();
		noteManager.SetIsPlaying(false);
		gameplayStats.SetActive(false);
		gameOverScreen.SetActive(true);

		RemainingInputWait = DelayAcceptInput;
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

		if(RemainingInputWait > 0) {
			RemainingInputWait -= Time.deltaTime;
			return;
		}

		for(int i = 0; i < BeatPulsers.Count; i++) {
			BeatPulsers[i].transform.localScale = Vector3.Lerp(BeatPulsers[i].transform.localScale, new Vector3(1, 1, 1), Time.deltaTime * ReturnSpeed);
		}

		if(Input.anyKeyDown) {
			if(gameOverScreen.activeInHierarchy) {
				SetTitlescreenVisible(true);
				return;
			}

			if(!Cond.IsCurrentlyPlaying()) {
				SetHitCount(0);
				SetLivesCount(3);

				SetTitlescreenVisible(false);
				gameplayStats.SetActive(true);

				Cond.StartSong();
				noteManager.SetIsPlaying(true);
			}
			else {
				noteManager.OnPlayerInput();
			}
		}
    }

	public void Pulse() {
		for(int i = 0; i < BeatPulsers.Count; i++) {
			BeatPulsers[i].transform.localScale = new Vector3(1, 1, 1) * PulseAmount;
		}
	}

	public void OnNewBeat(int MeasureNumber, int BeatNumber) {
		Pulse();
	}
}