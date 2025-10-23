using UnityEngine;
using UnityEngine.UI;

public class User : MonoBehaviour
{

    private string nombre;

    public void SetText(string name)
    {
        this.nombre = name;
    }

    public string GetText()
    {
        return this.nombre;
    }

}
