using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CardsAdjuster : MonoBehaviour
{
    public GridLayoutGroup layout;
    private void Start()
    {
        layout = GetComponent<GridLayoutGroup>();
    }
    private void Update()
    {
        if (layout.transform.childCount <= 10)
            layout.spacing = new Vector2(20, 20);
        else
        {
            int mod = (layout.transform.childCount+1) / 2;
            switch (mod){
                case 6:
                    layout.spacing = new Vector2(4, 20);
                    break;
                case 7:
                    layout.spacing = new Vector2(-24, 20);
                    break;
                case 8:
                    layout.spacing = new Vector2(-43, 20);
                    break;
                case 9:
                    layout.spacing = new Vector2(-58, 20);
                    break;
                case 10:
                    layout.spacing = new Vector2(-69, 20);
                    break;
                case 11:
                    layout.spacing = new Vector2(-78, 20);
                    break;
            }
        }
    }
}
