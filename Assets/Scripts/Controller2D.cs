using UnityEngine;
using System.Collections;

[RequireComponent( typeof( BoxCollider2D ) )]
public class Controller2D : MonoBehaviour
{
    struct RaycastOrigins
    {
        public Vector2 topLeft, topRight;
        public Vector2 bottomLeft, bottomRight;
    }

    public struct CollisionInfo
    {
        public bool above, below;
        public bool left, right;
        public bool climbingSlope;
        public float slopeAngle, slopeAngleOld;

        public void Reset()
        {
            above = below = left = right = false;
            climbingSlope = false;
            slopeAngleOld = slopeAngle;
            slopeAngle = 0;
        }
    }

    public LayerMask collisionMask;
    public CollisionInfo collisions;

    const float SKIN_WIDTH = 0.015f;

    public int horizontalRayCount = 4;
    public int verticalRayCount = 4;

    private float maxClimbAngle = 80;

    float horizontalRaySpacing;
    private float verticalRaySpacing;

    private BoxCollider2D collider2d;

    private RaycastOrigins _raycast_origins;

    public void Start()
    {
        collider2d = GetComponent<BoxCollider2D>();
        CalculateRaySpacing();
    }

    private void UpdateRaycastOrigins()
    {
        Bounds bounds = collider2d.bounds;
        bounds.Expand( SKIN_WIDTH * -2 );

        _raycast_origins.bottomLeft = new Vector2( bounds.min.x, bounds.min.y );
        _raycast_origins.bottomRight = new Vector2( bounds.max.x, bounds.min.y );
        _raycast_origins.topLeft = new Vector2( bounds.min.x, bounds.max.y );
        _raycast_origins.topRight = new Vector2( bounds.max.x, bounds.max.y );
    }

    private void CalculateRaySpacing()
    {
        Bounds bounds = collider2d.bounds;
        bounds.Expand( SKIN_WIDTH * -2 );

        horizontalRayCount = Mathf.Clamp( horizontalRayCount, 2, int.MaxValue );
        verticalRayCount = Mathf.Clamp( verticalRayCount, 2, int.MaxValue );

        horizontalRaySpacing = bounds.size.y / ( horizontalRayCount - 1 );
        verticalRaySpacing = bounds.size.x / ( verticalRayCount - 1 );

    }

    public void Move( Vector3 velocity )
    {
        collisions.Reset();

        UpdateRaycastOrigins();

        if ( velocity.x != 0 ) {
            HorizontalCollisions( ref velocity );
        }
        if ( velocity.y != 0 ) {
            VerticalCollisions( ref velocity );
        }
        transform.Translate( velocity );
    }

    public void HorizontalCollisions( ref Vector3 velocity )
    {
        float direction_x = Mathf.Sign( velocity.x );
        float rayLength = Mathf.Abs( velocity.x ) + SKIN_WIDTH;

        for ( int i = 0; i < horizontalRayCount; i++ ) {
            Vector2 rayOrigin = ( direction_x == -1 ) ? _raycast_origins.bottomLeft : _raycast_origins.bottomRight;
            rayOrigin += Vector2.up * ( horizontalRaySpacing * i );
            RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.right * direction_x, rayLength, collisionMask);
            //Debug.DrawRay( _raycast_origins.bottomLeft + Vector2.right * verticalRaySpacing * i, Vector2.up * -2, Color.red );
            Debug.DrawRay( rayOrigin, Vector2.right * direction_x * rayLength, Color.red );
            if ( hit ) {
                float slopeAngle = Vector2.Angle(hit.normal, Vector2.up);

                if ( i == 0 && slopeAngle <= maxClimbAngle ) {
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

    public void VerticalCollisions( ref Vector3 velocity )
    {
        float directionY = Mathf.Sign( velocity.y );
        float rayLength = Mathf.Abs( velocity.y ) + SKIN_WIDTH;

        for ( int i = 0; i < verticalRayCount; i++ ) {
            Vector2 rayOrigin = ( directionY == -1 ) ? _raycast_origins.bottomLeft : _raycast_origins.topLeft;
            rayOrigin += Vector2.right * ( verticalRaySpacing * i + velocity.x );
            RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.up * directionY, rayLength, collisionMask);

            Debug.DrawRay( rayOrigin, Vector2.up * directionY * rayLength, Color.red );

            if ( hit ) {
                velocity.y = ( hit.distance - SKIN_WIDTH ) * directionY;
                rayLength = hit.distance;
                if ( collisions.climbingSlope ) {
                    velocity.x = velocity.y / Mathf.Tan( collisions.slopeAngle * Mathf.Deg2Rad ) * Mathf.Sign( velocity.x );
                }

                collisions.below = ( directionY == -1 );
                collisions.above = ( directionY == 1 );
            }
        }
    }
}
