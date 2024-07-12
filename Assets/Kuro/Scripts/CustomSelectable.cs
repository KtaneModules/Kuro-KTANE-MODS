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
    private Color defaultTextColor = new Color(148f/ 255, 155f / 255, 164f / 255);
    private Color activeTextColor = Color.black;

    private Color grayBackgroundColor = new Color(64f / 255, 66f / 255, 73f / 255);
    private Color activeBackgroundColor = Color.white;


    private TextChannel textChannel;
    private VoiceChannel voiceChannel;

    public KMSelectable Selectable { get; set; }

    private void Awake()
    {
        Selectable = GetComponent<KMSelectable>();
        GameObject gameObject = _renderer.gameObject;

        Selectable.OnHighlight += () => 
        {
            textMesh.color = activeTextColor;
            _renderer.material.color = activeBackgroundColor;
            gameObject.SetActive(true);
        };
        Selectable.OnHighlightEnded += () => 
        {
            if ((textChannel != null && textChannel.Active) || (voiceChannel != null && voiceChannel.Active))
            {
                textMesh.color = Color.white;
                _renderer.material.color = grayBackgroundColor;
            }
            else
            {
                textMesh.color = defaultTextColor;
                gameObject.SetActive(false);
            }
            
            //Debug.Log(0);
        };
    }

    public void SetTextChannel(TextChannel channel) 
    {
        textChannel = channel;
    }

    public void SetVoiceChannel(VoiceChannel channel)
    {
        voiceChannel = channel;
    }
}
