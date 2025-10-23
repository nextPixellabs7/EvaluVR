using UnityEngine;

public class Pompon : MonoBehaviour
{
    [System.NonSerialized]
    protected string size;

    public string GetSize() => this.size;

    protected bool correcta;

    public bool estaCorrecta() => this.correcta;

    public void setCorrecta(bool args) => this.correcta = args;

}
