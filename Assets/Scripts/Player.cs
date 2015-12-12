using UnityEngine;
using System.Collections;

[RequireComponent( typeof( Controller2D ) )]
public class Player : MonoBehaviour
{
    public float jumpHeight = 4;
    public float timeToJumpApex = 0.4f;

    private Controller2D controller;
    private Vector3 velocity;
    private float gravity;
    private float jumpVelocity;

    private float accelerationTimeAirborne = .2f;
    private float accelerationTimeGrounded = .1f;
    private float moveSpeed = 6;
    private float velocityXSmoothing;

    void Start()
    {
        controller = GetComponent<Controller2D>();
        gravity = -( 2 * jumpHeight ) / Mathf.Pow( timeToJumpApex, 2 );
        jumpVelocity = Mathf.Abs( gravity * timeToJumpApex );
        print( "Gravity: " + gravity + " jump: " + jumpVelocity );
    }

    void Update()
    {
        if ( controller.collisions.above || controller.collisions.below ) {
            velocity.y = 0;
        }
        Vector2 input = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));

        if ( Input.GetKeyDown( KeyCode.Space ) && controller.collisions.below ) {
            velocity.y = jumpVelocity;
        }

        float targetVelocityX  = input.x * moveSpeed;

        velocity.x = Mathf.SmoothDamp( velocity.x, targetVelocityX, ref velocityXSmoothing,
                                ( controller.collisions.below ) ? accelerationTimeGrounded : accelerationTimeAirborne );
        velocity.y += gravity * Time.deltaTime;
        controller.Move( velocity * Time.deltaTime );
    }
}
