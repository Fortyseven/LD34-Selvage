/**
* Based on a tutorial series by Sebastian Lague 
* https://www.youtube.com/playlist?list=PLFt_AvWsXl0f0hqURlhyIoAabKPgRsqjz
**/

using UnityEngine;
using System.Collections;

[RequireComponent( typeof( Controller2D ) )]
public class Player : MonoBehaviour
{
    public float maxJumpHeight = 4;
    public float minJumpHeight = 1;

    public float timeToJumpApex = 0.4f;

    private Controller2D controller;
    private Vector3 velocity;
    private float gravity;
    private float maxJumpVelocity;
    private float minJumpVelocity;

    private float accelerationTimeAirborne = .2f;
    private float accelerationTimeGrounded = .1f;
    private float moveSpeed = 6;
    private float velocityXSmoothing;

    void Start()
    {
        controller = GetComponent<Controller2D>();
        gravity = -( 2 * maxJumpHeight ) / Mathf.Pow( timeToJumpApex, 2 );

        maxJumpVelocity = Mathf.Abs( gravity * timeToJumpApex );
        minJumpVelocity = Mathf.Sqrt( 2 * Mathf.Abs( gravity ) * minJumpHeight );
    }

    void Update()
    {
        Vector2 input = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));

        if ( Input.GetKeyDown( KeyCode.Space ) && controller.collisions.below ) {
            velocity.y = maxJumpVelocity;
        }

        if ( Input.GetKeyUp( KeyCode.Space ) ) {
            if ( velocity.y > minJumpVelocity ) {
                velocity.y = minJumpVelocity;
            }
        }

        float targetVelocityX  = input.x * moveSpeed;

        velocity.x = Mathf.SmoothDamp( velocity.x, targetVelocityX, ref velocityXSmoothing,
                                ( controller.collisions.below ) ? accelerationTimeGrounded : accelerationTimeAirborne );
        velocity.y += gravity * Time.deltaTime;

        if ( controller.collisions.above || controller.collisions.below ) {
            velocity.y = 0;
        }

        controller.Move( velocity * Time.deltaTime, input );

        if ( controller.collisions.above || controller.collisions.below ) {
            velocity.y = 0;
        }
    }
}
