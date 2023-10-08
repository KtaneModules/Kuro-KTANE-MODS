using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(KMSelectable))]
public class CustomSelectable : MonoBehaviour {

    [SerializeField]
    private MeshRenderer _renderer;

    private bool _isHighlighted;

    public KMSelectable Selectable { get; set; }

    private void Awake()
    {
        Selectable = GetComponent<KMSelectable>();

        Selectable.OnHighlight += () => { _renderer.gameObject.SetActive(true); Debug.Log("True"); };
        Selectable.OnHighlightEnded += () => { _renderer.gameObject.SetActive(false); Debug.Log("False"); };
    }
}
