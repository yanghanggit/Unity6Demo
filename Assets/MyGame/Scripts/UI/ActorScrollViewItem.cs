using Mosframe;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

public class ActorScrollViewItem : UIBehaviour, IDynamicScrollViewItem
{
    [Header("UI Components")]
    [SerializeField] Image _icon;
    [SerializeField] TMP_Text _title;
    [SerializeField] Image _background;
    [SerializeField] private Button _overlayButton;


    protected override void OnEnable()
    {

        base.OnEnable();
        _overlayButton.onClick.AddListener(this.OnClick);
    }

    protected override void OnDisable()
    {

        base.OnDisable();
        _overlayButton.onClick.RemoveListener(this.OnClick);
    }

    void OnClick()
    {
        Debug.Log("Clicked on " + _title.text);
    }

    public void onUpdateItem(int index)
    {

        Debug.Assert(_icon != null, "_icon != null");
        Debug.Assert(_title != null, "_title != null");
        Debug.Assert(_background != null, "_background != null");
        Debug.Assert(_overlayButton != null, "_overlayButton != null");

        _title.text = string.Format("Actor{0:d3}", (index + 1));
    }
}
