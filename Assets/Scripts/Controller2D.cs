/**
* Based on a tutorial series by Sebastian Lague 
* https://www.youtube.com/playlist?list=PLFt_AvWsXl0f0hqURlhyIoAabKPgRsqjz
**/

using UnityEngine;
using System.Collections;

public class Controller2D : RaycastController
{
    public struct CollisionInfo
    {
        public bool above, below;
        public bool left, right;
        public bool climbingSlope;
        public float slopeAngle, slopeAngleOld;
        public bool descendingSlope;
        public Vector3 velocityOld;
        public bool fallingThroughPlatform;

        public void Reset()
        {
            above = below = left = right = false;
            climbingSlope = false;
            slopeAngleOld = slopeAngle;
            slopeAngle = 0;
            descendingSlope = false;
        }
    }

    public CollisionInfo collisions;
    public Vector2 playerInput;

    private float maxClimbAngle = 80;
    private float maxDescendAngle = 75;

    public override void Start()
    {
        base.Start();
    }

    public void Move( Vector3 velocity, bool standingOnPlatform = false )
    {
        Move( velocity, Vector2.zero, standingOnPlatform );
    }

    public void Move( Vector3 velocity, Vector2 input, bool standingOnPlatform = false )
    {
        UpdateRaycastOrigins();
        collisions.Reset();
        collisions.velocityOld = velocity;

        playerInput = input;

        if ( velocity.y < 0 ) {
            DescendSlope( ref velocity );
        }

        if ( velocity.x != 0 ) {
            HorizontalCollisions( ref velocity );
        }
        if ( velocity.y != 0 ) {
            VerticalCollisions( ref velocity );
        }
        transform.Translate( velocity );

        if ( standingOnPlatform ) {
            collisions.below = true;
        }

    }

    private void HorizontalCollisions( ref Vector3 velocity )
    {
        float direction_x = Mathf.Sign( velocity.x );
        float rayLength = Mathf.Abs( velocity.x ) + SKIN_WIDTH;

        for ( int i = 0; i < horizontalRayCount; i++ ) {
            Vector2 rayOrigin = ( direction_x == -1 ) ? _raycast_origins.bottomLeft : _raycast_origins.bottomRight;
            rayOrigin += Vector2.up * ( horizontalRaySpacing * i );
            RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.right * direction_x, rayLength, collisionMask);

            Debug.DrawRay( rayOrigin, Vector2.right * direction_x * rayLength, Color.red );

            if ( hit ) {

                if ( hit.distance == 0 ) {
                    continue;
                }

                float slopeAngle = Vector2.Angle(hit.normal, Vector2.up);

                if ( i == 0 && slopeAngle <= maxClimbAngle ) {

                    if ( collisions.descendingSlope ) {
                        collisions.descendingSlope = false;
                        velocity = collisions.velocityOld;
                    }

                    float distanceToSlopeStart = 0;

                    if ( slopeAngle != collisions.slopeAngleOld ) {
                        distanceToSlopeStart = hit.distance - SKIN_WIDTH;
                        velocity.x -= distanceToSlopeStart * direction_x;
                    }

                    ClimbSlope( ref velocity, slopeAngle );
                    velocity.x += distanceToSlopeStart * direction_x;
                }

                if ( !collisions.climbingSlope || slopeAngle > maxClimbAngle ) {
                    velocity.x = ( hit.distance - SKIN_WIDTH ) * direction_x;
                    rayLength = hit.distance;

                    if ( collisions.climbingSlope ) {
                        velocity.y = Mathf.Tan( collisions.slopeAngle * Mathf.Deg2Rad ) * Mathf.Abs( velocity.x );
                    }

                    collisions.left = ( direction_x == -1 );
                    collisions.right = ( direction_x == 1 );
                }
            }
        }
    }

    private void ClimbSlope( ref Vector3 velocity, float slope_angle )
    {
        float moveDistance = Mathf.Abs(velocity.x);
        float climbVelocityY = Mathf.Sin( slope_angle * Mathf.Deg2Rad ) * moveDistance;

        if ( velocity.y <= climbVelocityY ) {
            velocity.y = climbVelocityY;
            velocity.x = Mathf.Cos( slope_angle * Mathf.Deg2Rad ) * moveDistance * Mathf.Sign( velocity.x );
            collisions.below = true;
            collisions.climbingSlope = true;
            collisions.slopeAngle = slope_angle;
        }
    }

    private void DescendSlope( ref Vector3 velocity )
    {
        float directionX = Mathf.Sign(velocity.x);

        Vector2 rayOrigin = (directionX == -1) ? _raycast_origins.bottomRight : _raycast_origins.bottomLeft;
        RaycastHit2D hit = Physics2D.Raycast(rayOrigin, -Vector2.up, Mathf.Infinity, collisionMask);
        if ( hit ) {
            float slopeAngle = Vector2.Angle(hit.normal, Vector2.up);
            if ( slopeAngle != 0 && slopeAngle <= maxDescendAngle ) {
                if ( Mathf.Sign( hit.normal.x ) == directionX ) {
                    if ( ( hit.distance - SKIN_WIDTH ) <= Mathf.Tan( slopeAngle * Mathf.Deg2Rad ) * Mathf.Abs( velocity.x ) ) {
                        float moveDistance = Mathf.Abs( velocity.x );
                        float descendVelocityY = Mathf.Sin( slopeAngle * Mathf.Deg2Rad ) * moveDistance;
                        velocity.x = Mathf.Cos( slopeAngle * Mathf.Deg2Rad ) * moveDistance * Mathf.Sign( velocity.x );
                        velocity.y -= descendVelocityY;

                        collisions.slopeAngle = slopeAngle;
                        collisions.descendingSlope = true;
                        collisions.below = true;
                    }
                }
            }
        }
    }

    private void VerticalCollisions( ref Vector3 velocity )
    {
        float directionY = Mathf.Sign( velocity.y );
        float rayLength = Mathf.Abs( velocity.y ) + SKIN_WIDTH;

        for ( int i = 0; i < verticalRayCount; i++ ) {
            Vector2 rayOrigin = ( directionY == -1 ) ? _raycast_origins.bottomLeft : _raycast_origins.topLeft;
            rayOrigin += Vector2.right * ( verticalRaySpacing * i + velocity.x );
            RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.up * directionY, rayLength, collisionMask);

            Debug.DrawRay( rayOrigin, Vector2.up * directionY * rayLength, Color.red );

            if ( hit ) {

                if ( hit.collider.tag == "Through" ) {
                    if ( directionY == 1 || hit.distance == 0 ) {
                        continue;
                    }
                    if ( collisions.fallingThroughPlatform ) {
                        continue;
                    }
                    if ( playerInput.y == -1 ) {
                        collisions.fallingThroughPlatform = true;
                        Invoke( "ResetFallingThroughPlatform", 0.5f );
                        continue;
                    }
                }

                velocity.y = ( hit.distance - SKIN_WIDTH ) * directionY;
                rayLength = hit.distance;
                if ( collisions.climbingSlope ) {
                    velocity.x = velocity.y / Mathf.Tan( collisions.slopeAngle * Mathf.Deg2Rad ) * Mathf.Sign( velocity.x );
                }

                collisions.below = ( directionY == -1 );
                collisions.above = ( directionY == 1 );
            }
        }

        if ( collisions.climbingSlope ) {
            float directionX = Mathf.Sign(velocity.x);
            rayLength = Mathf.Abs( velocity.x ) + SKIN_WIDTH;
            Vector2 rayOrigin = ((directionX == -1) ? _raycast_origins.bottomLeft : _raycast_origins.bottomRight) +
                                Vector2.up * velocity.y;
            RaycastHit2D hit = Physics2D.Raycast( rayOrigin, Vector2.right * directionX, rayLength, collisionMask );
            if ( hit ) {
                float slopeAngle = Vector2.Angle(hit.normal, Vector2.up);
                if ( slopeAngle != collisions.slopeAngle ) {
                    velocity.x = ( hit.distance - SKIN_WIDTH ) * directionX;
                    collisions.slopeAngle = slopeAngle;
                }
            }
        }
    }

    public void ResetFallingThroughPlatform()
    {
        collisions.fallingThroughPlatform = false;
    }

}
