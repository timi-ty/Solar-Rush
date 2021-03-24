using UnityEngine;
using UnityEngine.Events;

public class HomeScreen : MonoBehaviour
{
    #region Components
    private Animator mAnimator;
    #endregion

    #region Events
    public UnityEvent OnGameStartAnimationFinished = new UnityEvent();
    #endregion

    private void Start()
    {
        mAnimator = GetComponent<Animator>();
    }

    public void PlayGameStartAnimation()
    {
        mAnimator.SetTrigger("gameStart");
    }

    public void OnGameStartAnimationDone()
    {
        GameManager.gameInstance.OnStartGame();
        OnGameStartAnimationFinished?.Invoke();
    }
}
