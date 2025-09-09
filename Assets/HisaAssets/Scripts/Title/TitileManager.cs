using UnityEngine;
using UnityEngine.SceneManagement;

public class TitileManager : MonoBehaviour
{
    [SerializeField] string selectSceneName;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetButtonDown("Select"))
        {
            SceneManager.LoadScene(selectSceneName);
        }
    }
}
