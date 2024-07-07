using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowCard : MonoBehaviour
{
    public Transform target_card;
    [SerializeField] private float speed;
    void Update()
    {
        transform.position = Vector2.Lerp(transform.position, target_card.position, speed);
    }
}
