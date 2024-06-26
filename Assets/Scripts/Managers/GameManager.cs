using System;
using Grid_System;
using UnityEngine;

namespace Managers
{
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance; 
        public static Action OnClickProcessed;
        public static Action OnFrogClick;
        public static Action OnGameOver;
        public static Action OnLevelFinish;
        private int _clickCount;
        public int maxClickCount;

        private void OnEnable()
        {
            OnFrogClick += OnClickHandler;
            OnClickProcessed += ClickProcessed;
            OnLevelFinish += LevelFinished;
        }

        private void OnDisable()
        {
            OnFrogClick -= OnClickHandler; 
            OnClickProcessed -= ClickProcessed;
            OnLevelFinish -= LevelFinished;
        }

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

            _clickCount = maxClickCount;
        }

        private void Start()
        {
            UIController.Instance.UpdateMovesText(_clickCount.ToString());
        }

        private void LevelFinished()
        {
            
        }
        private void ClickProcessed()
        {
            if (GridController.Instance.CheckIsClear())
            {
                UIController.Instance.OpenWinPanel();
                print("win");
                return;
            }
            else if (_clickCount == 0)
            {
                print("lose");
                UIController.Instance.OpenFailPanel();                
            }
        }

        private void OnClickHandler()
        {
            _clickCount--;
            UIController.Instance.UpdateMovesText(_clickCount.ToString());
        }
    }
}