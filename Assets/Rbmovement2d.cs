using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rbmovement2d : MonoBehaviour
{
    public float speed = 10.0f;
    public Rigidbody2D rb;
    public Vector2 movement;

    // Start is called before the first frame update
    void Start() {
        rb = (Rigidbody2D)this.GetComponent<Rigidbody2D>();
    }

    // Update is called once per frame
    void Update() {
        movement = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
        if(Input.GetKeyDown(KeyCode.Space)) {
            if(rb.transform.position[1] < 0) {
                rb.transform.position = (Vector2)transform.position + new Vector2(0.0f, 40.0f);
            } else {
                rb.transform.position = (Vector2)transform.position + new Vector2(0.0f, -40.0f);
            }
            Debug.Log("space key was pressed");
        }
    }

    void FixedUpdate() {
        moveCharacter(movement);
    }

    void moveCharacter(Vector2 direction) {
        // rb.velocity = direction * speed;
        rb.MovePosition((Vector2)transform.position + (direction * speed * Time.deltaTime));
    }
}
