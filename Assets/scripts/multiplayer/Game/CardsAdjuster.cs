using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CardsAdjuster : MonoBehaviour
{
    public GridLayoutGroup layout;
    [SerializeField] private int[] xspacing = {0,0,0,0,0,0};
    [SerializeField] private int yspacing = 0;
    private void Start()
    {
        layout = GetComponent<GridLayoutGroup>();
    }
    private void Update()
    {  
        if (layout.transform.childCount <= 10)
            layout.spacing = new Vector2(30, yspacing);
        else
        {
            int mod = (layout.transform.childCount+1) / 2;
            switch (mod){
                case 6:
                    layout.spacing = new Vector2(xspacing[0], yspacing); //4
                    break;
                case 7:
                    layout.spacing = new Vector2(xspacing[1], yspacing); //-24
                    break;
                case 8:
                    layout.spacing = new Vector2(xspacing[2], yspacing); //-43
                    break;
                case 9:
                    layout.spacing = new Vector2(xspacing[3], yspacing); //-58
                    break;
                case 10:
                    layout.spacing = new Vector2(xspacing[4], yspacing); //-69
                    break;
                case 11:
                    layout.spacing = new Vector2(xspacing[5], yspacing); //-78
                    break;
            }
        }
    }
}
