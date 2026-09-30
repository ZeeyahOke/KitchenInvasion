using TMPro;
using UnityEngine;

namespace KitchenInvasion {
    public class UIManager : MonoBehaviour {
        [SerializeField] GameObject startPanel;
        [SerializeField] GameObject hudPanel;
        [SerializeField] GameObject endPanel;
        [SerializeField] GameObject leaderboardPanel;
        [SerializeField] RectTransform healthFill;
        [SerializeField] TMP_Text healthText;
        [SerializeField] TMP_Text scoreText;
        [SerializeField] TMP_Text timeText;
        [SerializeField] GameObject placeHint;
        [SerializeField] TMP_Text resultTitle;
        [SerializeField] TMP_Text resultStats;
        [SerializeField] TMP_Text leaderboardText;

        void OnEnable() {
            GameEvents.OnMenuOpened += ShowStart;
            GameEvents.OnGameStarted += ShowHud;
            GameEvents.OnGameEnded += ShowEnd;
            GameEvents.OnPotPlaced += HideHint;
            GameEvents.OnScoreChanged += UpdateScore;
            GameEvents.OnHealthChanged += UpdateHealth;
            GameEvents.OnTimeChanged += UpdateTime;
        }

        void OnDisable() {
            GameEvents.OnMenuOpened -= ShowStart;
            GameEvents.OnGameStarted -= ShowHud;
            GameEvents.OnGameEnded -= ShowEnd;
            GameEvents.OnPotPlaced -= HideHint;
            GameEvents.OnScoreChanged -= UpdateScore;
            GameEvents.OnHealthChanged -= UpdateHealth;
            GameEvents.OnTimeChanged -= UpdateTime;
        }

        void ShowPanel(GameObject panel) {
            startPanel.SetActive(panel == startPanel);
            hudPanel.SetActive(panel == hudPanel);
            endPanel.SetActive(panel == endPanel);
            leaderboardPanel.SetActive(panel == leaderboardPanel);
        }

        void ShowStart() { ShowPanel(startPanel); }

        void ShowHud() {
            ShowPanel(hudPanel);
            placeHint.SetActive(!GameManager.Instance.PotPlacer.HasPot);
        }

        void HideHint() { placeHint.SetActive(false); }

        void ShowEnd(SessionRecord result) {
            ShowPanel(endPanel);
            resultTitle.text = result.won ? "You Survived!" : "You Got Cooked!";
            resultStats.text = "Score: " + result.score
                + "\nEnemies defeated: " + result.enemiesDefeated
                + "\nTime survived: " + result.timeSurvived.ToString("0") + "s";
        }

        void UpdateScore(int score) { scoreText.text = "Score " + score; }

        void UpdateTime(float timeLeft) { timeText.text = Mathf.CeilToInt(timeLeft) + "s"; }

        void UpdateHealth(int health, int maxHealth) {
            healthFill.anchorMax = new Vector2((float)health / maxHealth, 1f);
            healthText.text = health + " HP";
        }

        public void PressStart() { GameEvents.ButtonClicked(); GameManager.Instance.StartGame(); }

        public void PressMainMenu() { GameEvents.ButtonClicked(); GameManager.Instance.OpenMenu(); }

        public void PressHardMode(bool isHard) { GameEvents.ButtonClicked(); GameManager.Instance.SetHardMode(isHard); }

        public void PressLeaderboard() {
            GameEvents.ButtonClicked();
            ShowPanel(leaderboardPanel);
            SessionHistory history = Leaderboard.Load();
            string lines = "";
            for (int i = 0; i < history.sessions.Count; i++) {
                SessionRecord s = history.sessions[i];
                lines += (i + 1) + ".  " + s.score + " pts  |  " + s.enemiesDefeated + " defeated  |  "
                    + s.timeSurvived.ToString("0") + "s\n<size=70%>" + s.date + "</size>\n\n";
            }
            leaderboardText.text = lines == "" ? "No games played yet." : lines;
        }
    }
}
