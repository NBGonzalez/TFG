using UnityEngine;
using UnityEngine.SceneManagement;

public class BotonProvisionalSalir : MonoBehaviour
{

    public void BotonBack()
    {
        SceneManager.LoadScene("MainScene");
    }
}
