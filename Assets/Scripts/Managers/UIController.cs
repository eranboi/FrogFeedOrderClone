using System;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Managers
{
    public class UIController : MonoBehaviour
    {
        public static UIController Instance;
        [SerializeField] private GameObject failPanel;
        [SerializeField] private GameObject winPanel;
        [SerializeField] private Button nextLevelButton;
        [SerializeField] private Button restartButton;
        [SerializeField] private TextMeshProUGUI movesText;
        
        

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void Start()
        {
            nextLevelButton.onClick.AddListener(LoadNextLevel);
            restartButton.onClick.AddListener(RestartLevel);
        }

        private void LoadNextLevel()
        {
            var totalLevelCount = 3;
            var sceneBuildIndex = (SceneManager.GetActiveScene().buildIndex + 1) % totalLevelCount;

            SceneManager.LoadScene(sceneBuildIndex);
        }

        private void RestartLevel()
        {
            var sceneBuildIndex = SceneManager.GetActiveScene().buildIndex;
            SceneManager.LoadScene(sceneBuildIndex);

        }
        
        public void OpenFailPanel()
        {
            failPanel.SetActive(true);
        }

        public void OpenWinPanel()
        {
            winPanel.SetActive(true);
        }

        public void UpdateMovesText(string moveCount)
        {
            movesText.text = $"Moves :  {moveCount}";
        }
    }
}