using UnityEngine;
using System.Collections;

public abstract class SingletonMono<T> : MonoBehaviour
{
    #region Singleton
    public static T gameInstance { get; private set; }
    #endregion

    #region Unity Runtime
    protected virtual void Awake()
    {
        if (!Application.isPlaying) return;

        #region Singleton
        if (transform.parent == null)
        {
            /*BUG: When a new scene is loaded, all instances of a SingletonMono<T> object that have parents 
                    end up destroying themselves and leaving none left.*/
            DontDestroyOnLoad(gameObject);
        }

        if (gameInstance == null)
        {
            gameInstance = GetComponent<T>();
        }
        else if (!gameInstance.Equals(this))
        {
            Destroy(gameObject);
        }
        #endregion
    }
    #endregion
}
