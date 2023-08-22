using UnityEngine;
using UnityEngine.SceneManagement;

namespace Ravity
{
    public abstract class Singleton<T> : MonoBehaviour where T : Singleton<T>
    {
        public static T Instance { get; private set; }

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = (T) this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Debug.Log($"Destroying instance of {typeof(T)} in '{SceneManager.GetActiveScene().name}' scene, because there already was one. Make sure that this is intended.");
                Destroy(this);
            }
        }
    }
}
