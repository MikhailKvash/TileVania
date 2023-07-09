using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Arrow : MonoBehaviour
{
    [SerializeField] float arrowSpeed = 20f;
    [SerializeField] AudioClip ArrowHitSFX;
    [SerializeField] [Range(0f, 1f)] float ArrowHitVolume = 1f;

    Rigidbody2D myRigidbody;
    PlayerMovement player;
    float xSpeed;

    void Start()
    {
        myRigidbody = GetComponent<Rigidbody2D>();
        player = FindObjectOfType<PlayerMovement>();
        xSpeed = player.transform.localScale.x * arrowSpeed;
        transform.localScale = new Vector2((Mathf.Sign(xSpeed)) * transform.localScale.x, transform.localScale.y);
    }

    void Update()
    {
        myRigidbody.velocity = new Vector2(xSpeed,0f);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if(other.tag == "Enemy")
        {
            AudioSource.PlayClipAtPoint(ArrowHitSFX, transform.position, ArrowHitVolume);
            Destroy(other.gameObject);
        }
        Destroy(gameObject);
    }
    
}
