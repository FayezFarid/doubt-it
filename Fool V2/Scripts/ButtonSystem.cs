using UnityEngine;
using UnityEngine.UI;
public class ButtonSystem : MonoBehaviour
{
    [SerializeField] public int EqNumber {  get; private set; }
    public Color32 color32 = new Color32(255, 255, 255, 255);
    public Color32 OldColor;
    public bool HasBeenSelected=false;
    public bool Selectable;
    
    public void Awake()
    {
        Selectable = true;
        OldColor=GetComponent<Image>().color;
    }
    public void NumberSelected()=> GameHandlerv2.Instance.SelectedNumberButton(gameObject);
    public void DisActive()=>  GetComponent<Image>().color = OldColor;
    public void Activate()=> GetComponent<Image>().color = color32;

 
}
