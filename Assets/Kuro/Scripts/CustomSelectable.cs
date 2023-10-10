using System.Collections;
using System.Collections.Generic;
using UnityEditorInternal;
using UnityEngine;

[RequireComponent(typeof(KMSelectable))]
public class CustomSelectable : MonoBehaviour {

    [SerializeField]
    private MeshRenderer _renderer;
    [SerializeField]
    private TextMesh textMesh;
    private Color defaultColor = new Color(148f/ 255, 155f / 255, 164f / 255);
    private Color activeColor = Color.black;

    public KMSelectable Selectable { get; set; }

    private void Awake()
    {
        Selectable = GetComponent<KMSelectable>();
        GameObject gameObject = _renderer.gameObject;

        Selectable.OnHighlight += () => 
        {
            textMesh.color = activeColor;
            gameObject.SetActive(true);
            //Debug.Log(1);
        };
        Selectable.OnHighlightEnded += () => 
        {
            textMesh.color = defaultColor;
            gameObject.SetActive(false);
            //Debug.Log(0);
        };
    }
}
