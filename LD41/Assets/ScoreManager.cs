using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Linq;

public class ScoreManager : MonoBehaviour {

	public InputField input;

	public Button submitScore;

	public Text[] entries;

	public Color playerColour;

    public UnitSelection player;

	private string value;
	private Dictionary<string, UserScore> userScores;
	private dreamloLeaderBoard dl;

	private bool restartOnLoad;

	public enum leaderboardState
	{
		waiting,
		leaderboard,
		done
	};

	public leaderboardState ls;

	public struct UserScore {
		public Dictionary<string, int> scores;
		public string name;
	}

	private string lastHighScores;

	private string youUserID = "{12345-12345-12345-12345-12345-12345}";
	private int lastHighestTower;

	private bool loading;
		
	void Start () {
		this.dl = dreamloLeaderBoard.GetSceneDreamloLeaderboard ();
		lastHighScores = dl.highScores;
		loading = true;
		LoadLeaderboard ();
        lastHighestTower = -1;
	}
	
	void Update () {		
		if (ls == leaderboardState.leaderboard) {
			List<dreamloLeaderBoard.Score> scoreList = dl.ToListHighToLow ();
			if (scoreList != null && dl.highScores != lastHighScores) {
				foreach (dreamloLeaderBoard.Score currentScore in scoreList) {
					SetScore (currentScore.playerName, currentScore.playerName, "towers", currentScore.score);
				}
				lastHighScores = dl.highScores;
				ls = leaderboardState.done;
				if (loading) {
					loading = false;
					foreach (Text text in entries) {
						text.text = "";
					}
				}
				if (restartOnLoad) {
					Debug.Log ("highscore string: " + lastHighScores);
					SceneManager.LoadScene (SceneManager.GetActiveScene ().buildIndex);
				}
			}
		}

        if (player.gameOver || lastHighestTower == player.towersDestroyedCount || ls == leaderboardState.leaderboard)
			return;
		lastHighestTower = player.towersDestroyedCount;
        SetScore (youUserID, "you", "towers", player.towersDestroyedCount);
		string[] userIds = GetUserIDs ("towers");
		int playerIndex = System.Array.IndexOf (userIds, youUserID);

		int iterations = Mathf.Min (entries.Length, userIds.Length);

		//entries [entries.Length - 1].text = (2 + playerIndex) + ". " + " you / " + player.highestMass;
		if (playerIndex + iterations + 1 > userIds.Length) {
			playerIndex = userIds.Length - iterations;
		} else {
			playerIndex -= iterations / 2;
		}

		if (playerIndex < 0) {
			playerIndex = 0;
		}

		for (int i = 0; i < iterations; i++) {
			string curUserId = userIds [playerIndex + i];
			entries [i].text = (playerIndex + 1 + i) + ". " + (curUserId == youUserID ? "you" : curUserId)  + " / " + GetScore (curUserId, "towers");
			entries [i].color = (curUserId == youUserID ? playerColour : Color.black);
		}
	}

	public int GetScore(string userid, string scoreType)
	{
		if (userScores == null) userScores = new Dictionary<string, UserScore>();

		if (!userScores.ContainsKey(userid))
		{
			return 0;
		}
		if (!userScores[userid].scores.ContainsKey(scoreType))
		{
			return 0;
		}
		return userScores[userid].scores[scoreType];
	}

	public void SetScore(string userid, string username, string scoreType, int value)
	{
		if (userScores == null) userScores = new Dictionary<string, UserScore>();
		if (!userScores.ContainsKey(userid))
		{
			userScores[userid] = new UserScore() { name = username, scores = new Dictionary<string, int>() };
		}
		userScores[userid].scores[scoreType] = value;
	}

	public string GetUserName(string userid)
	{
		return userScores[userid].name;
	}

	public string[] GetUserIDs(string sortingType)
	{
		if (userScores == null) userScores = new Dictionary<string, UserScore>();

		return userScores.Keys.OrderByDescending(n => GetScore(n, sortingType)).ToArray();
	}

	public void LoadLeaderboard() {
		dl.LoadScores();
		ls = leaderboardState.leaderboard;
	}

	public void TextFieldChanged () {
		submitScore.interactable = input.text.Length > 0;
	}

	public void SubmitScore () {
		if (ls == leaderboardState.leaderboard)
			return;
		int score = player.towersDestroyedCount;
        int time = (int)player.timeAlive;
        int unitsDead = player.unitsDeadCount;
        int unitsAlive = (int)player.unitsAlive;
		value = input.text;
		if (value.Length == 0)
			return;
		submitScore.interactable = false;
		input.interactable = false;
		dl.AddScore (value, score, time, unitsDead + "/" + unitsAlive);
		restartOnLoad = true;
		ls = leaderboardState.leaderboard;
		Debug.Log ("Submitted score " + score + " as " + value);
	}
}
