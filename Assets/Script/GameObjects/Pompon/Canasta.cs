using UnityEngine;

public class Canasta : MonoBehaviour
{
    public PomponGame gameManager;
    public string tipoCanasta;

    private void OnTriggerEnter(Collider other)
    {
        Pompon pompon = other.GetComponent<Pompon>();
        if (pompon != null)
        {
            gameManager.RegistrarPompon(pompon, tipoCanasta);
        }
    }
}
