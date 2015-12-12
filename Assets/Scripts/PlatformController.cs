using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PlatformController : RaycastController
{
    struct PassengerMovement
    {
        public Transform transform;
        public Vector3 velocity;
        public bool standingOnPlatform;
        public bool moveBeforePlatform;

        public PassengerMovement( Transform _transform, Vector3 _velocity, bool _standingOnPlatform, bool _moveBeforePlatform )
        {
            transform = _transform;
            velocity = _velocity;
            standingOnPlatform = _standingOnPlatform;
            moveBeforePlatform = _moveBeforePlatform;
        }
    }

    private List<PassengerMovement> passengerMovement;
    private Dictionary<Transform, Controller2D> passengerDictionary =  new Dictionary<Transform, Controller2D>();

    public LayerMask passengerMask;
    public Vector3 move;

    public override void Start()
    {
        base.Start();
    }

    // Update is called once per frame
    private void Update()
    {
        UpdateRaycastOrigins();

        Vector3 velocity = move * Time.deltaTime;
        CalculatePassengerMovement( velocity );
        MovePassengers( true );
        transform.Translate( velocity );
        MovePassengers( false );
    }

    private void MovePassengers( bool beforeMovePlatform )
    {
        foreach ( PassengerMovement passenger in passengerMovement ) {
            if ( !passengerDictionary.ContainsKey( passenger.transform ) ) {
                passengerDictionary.Add( passenger.transform, passenger.transform.GetComponent<Controller2D>() );
            }
            if ( passenger.moveBeforePlatform == beforeMovePlatform ) {
                passengerDictionary[ passenger.transform ].Move( passenger.velocity, passenger.standingOnPlatform );
            }
        }
    }

    private void CalculatePassengerMovement( Vector3 velocity )
    {
        HashSet<Transform> movedPassengers = new HashSet<Transform>();
        passengerMovement = new List<PassengerMovement>();

        float directionX = Mathf.Sign(velocity.x);
        float directionY = Mathf.Sign(velocity.y);

        // Vertically moving platform
        if ( velocity.y != 0 ) {
            float rayLength = Mathf.Abs(velocity.y) + SKIN_WIDTH;

            for ( int i = 0; i < verticalRayCount; i++ ) {
                Vector2 rayOrigin = (directionY == -1) ? _raycast_origins.bottomLeft : _raycast_origins.topLeft;
                rayOrigin += Vector2.right * ( verticalRaySpacing * i );
                RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.up*directionY, rayLength, passengerMask);

                if ( hit ) {
                    if ( !movedPassengers.Contains( hit.transform ) ) {
                        movedPassengers.Add( hit.transform );
                        float pushX = (directionY == 1) ? velocity.x : 0;
                        float pushY = velocity.y - (hit.distance - SKIN_WIDTH)*directionY;

                        hit.transform.Translate( new Vector3( pushX, pushY ) );
                        passengerMovement.Add( new PassengerMovement( hit.transform, new Vector3( pushX, pushY ), ( directionY == 1 ), true ) );
                    }
                }
            }
        }

        // Horizontally moving platform
        if ( velocity.x != 0 ) {
            float rayLength = Mathf.Abs(velocity.x) + SKIN_WIDTH;

            for ( int i = 0; i < horizontalRayCount; i++ ) {
                Vector2 rayOrigin = ( directionX == -1 ) ? _raycast_origins.bottomLeft : _raycast_origins.bottomRight;
                rayOrigin += Vector2.up * ( horizontalRaySpacing * i );
                RaycastHit2D hit = Physics2D.Raycast( rayOrigin, Vector2.right * directionX, rayLength, passengerMask );

                if ( hit ) {
                    if ( !movedPassengers.Contains( hit.transform ) ) {
                        movedPassengers.Add( hit.transform );
                        float pushX = velocity.x - ( hit.distance - SKIN_WIDTH ) * directionX; ;
                        float pushY = -SKIN_WIDTH;

                        passengerMovement.Add( new PassengerMovement( hit.transform, new Vector3( pushX, pushY ), false, true ) );
                    }
                }
            }
        }

        // Passenger on top of horiz or downward moving plat
        if ( directionY == -1 || velocity.y == 0 && velocity.x != 0 ) {
            float rayLength = SKIN_WIDTH * 2;

            for ( int i = 0; i < verticalRayCount; i++ ) {
                Vector2 rayOrigin = _raycast_origins.topLeft + Vector2.right * ( verticalRaySpacing * i );
                RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.up, rayLength, passengerMask);

                if ( hit ) {
                    if ( !movedPassengers.Contains( hit.transform ) ) {
                        movedPassengers.Add( hit.transform );
                        float pushX = velocity.x;
                        float pushY = velocity.y;

                        passengerMovement.Add( new PassengerMovement( hit.transform, new Vector3( pushX, pushY ), true, false ) );
                    }
                }
            }
        }

    }
};