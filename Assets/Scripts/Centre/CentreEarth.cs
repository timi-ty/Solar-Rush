using UnityEngine;

public class CentreEarth : MonoBehaviour
{
    #region Graphics
    public Sprite arcadeSprite;
    public Sprite infiniteSprite;
    #endregion

    #region Components
    private SpriteRenderer mRenderer;
    #endregion

    private void PlayInfinite()
    {
        mRenderer = GetComponent<SpriteRenderer>();
        mRenderer.sprite = infiniteSprite;
    }

    private void PlayArcade()
    {
        mRenderer = GetComponent<SpriteRenderer>();
        mRenderer.sprite = arcadeSprite;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        Comet comet = collision.transform.GetComponent<Comet>();

        if (comet)
        {
            GameManager.gameInstance.OnFailGame();
        }
    }
}
