using UnityEngine;
using UnityEngine.SceneManagement;

public class ChangeScene : MonoBehaviour
{
    
    public void CambiarEscena(int index)
    {
        SceneManager.LoadScene(index);
    }

}
