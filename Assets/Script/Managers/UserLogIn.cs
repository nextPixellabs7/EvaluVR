using UnityEngine;
using TMPro;
using Microsoft.MixedReality.Toolkit.Experimental.UI;

public class UserLogIn : MonoBehaviour
{
    public User GM;
    public TextMeshProUGUI bienvenido;
    private string newName;

    private TMP_InputField inputField;

    private void Start()
    {
        inputField = GetComponent<TMP_InputField>();
        inputField.onSelect.AddListener(x => AbrirTeclado());
    }

    public void AbrirTeclado()
    {
        NonNativeKeyboard.Instance.InputField = inputField;
        NonNativeKeyboard.Instance.PresentKeyboard(inputField.text);
    }

    public void setName()
    {
        newName = inputField.text;
        if(newName != "")
        {
            GM.SetText(newName);
            bienvenido.text = ("Bienvenido\n" + GM.GetText());
        }
        
    }
}
