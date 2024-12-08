using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class FollowCard : MonoBehaviour
{
    public Transform target_card;
    public TextMeshProUGUI player_attack_text;
    public TextMeshProUGUI player_defence_text;
    public TextMeshProUGUI ActionModifierText;
    public TextMeshProUGUI ActionTypeText;
    public GameObject Deletion;
    public List<GameObject> up_arrows;
    public List<GameObject> down_arrows;

    [SerializeField] private float speed;
    void Update()
    {
        transform.position = Vector2.Lerp(transform.position, target_card.position, speed*Time.deltaTime);
    }
}
