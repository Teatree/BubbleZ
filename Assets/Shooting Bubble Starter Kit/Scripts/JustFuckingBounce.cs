using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JustFuckingBounce : MonoBehaviour {

    public LayerMask collisionMask;

    public Transform ball;
    private float speed = 15;

    // Update is called once per frame
    void Start () {
        GetComponent<Rigidbody2D>().AddForce(new Vector2(5f, 5f));

        //Ray2D ray = new Ray2D(transform.position, transform.up);
        //RaycastHit2D hit = Physics2D.Raycast(transform.position, transform.up, Time.deltaTime * G.bubbleSpeed + 0.1f);

        //if (hit.collider && hit.transform.gameObject.layer == LayerMask.NameToLayer("BoxC")) {
        //    Vector2 refDir = Vector2.Reflect(ray.direction, hit.normal);
        //    float rot = 90 - Mathf.Atan2(refDir.y, refDir.x) * Mathf.Rad2Deg;
        //    transform.eulerAngles = new Vector3(0,0,rot);

        //    Debug.Log("colliding!");
        //}
	}
}
