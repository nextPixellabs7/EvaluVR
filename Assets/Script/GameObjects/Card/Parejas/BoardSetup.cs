using System.Collections.Generic;
using UnityEngine;

public class BoardSetup : MonoBehaviour
{
    [Header("Anclas (12)")]
    public Transform[] anchors;

    [Header("Cartas (12)")]
    public Card[] cards;
    
    [Header("Ajustes")]
    public int parejasTotales = 6;
    public bool shuffleAtStart = true;
    public bool autoAssignPairIds = false;

    void Reset()
    {
        parejasTotales = 6;
    }

    void Awake()
    {
        if (anchors == null || anchors.Length != parejasTotales * 2)
        {
            Debug.LogError("BoardSetup: Debes asignar exactamente 12 anchors.");
            return;
        }
        if (cards == null || cards.Length != parejasTotales * 2)
        {
            Debug.LogError("BoardSetup: Debes asignar exactamente 12 cartas.");
            return;
        }

        // Genera lista de indices y baraja
        var idx = new List<int>(anchors.Length);
        for (int i = 0; i < anchors.Length; i++) idx.Add(i);
        if (shuffleAtStart) Shuffle(idx);

        // Coloca cada carta en un anchor barajado y setea HomeAnchor
        for (int c = 0; c < cards.Length; c++)
        {
            var anchor = anchors[idx[c]];
            cards[c].SetHomeAnchor(anchor);
            cards[c].transform.SetPositionAndRotation(anchor.position, anchor.rotation);
        }
    }

    void Shuffle(List<int> a)
    {
        for (int i = a.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            (a[i], a[j]) = (a[j], a[i]);
        }
    }
}
