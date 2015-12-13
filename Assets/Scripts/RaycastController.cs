/**
* Based on a tutorial series by Sebastian Lague 
* https://www.youtube.com/playlist?list=PLFt_AvWsXl0f0hqURlhyIoAabKPgRsqjz
**/

using UnityEngine;

[RequireComponent( typeof( BoxCollider2D ) )]
public class RaycastController : MonoBehaviour
{
    protected const float SKIN_WIDTH = 0.015f;

    protected struct RaycastOrigins
    {
        public Vector2 topLeft, topRight;
        public Vector2 bottomLeft, bottomRight;
    }

    public LayerMask collisionMask;

    [HideInInspector]
    public int horizontalRayCount = 4;

    [HideInInspector]
    public int verticalRayCount = 4;

    [HideInInspector]
    protected float horizontalRaySpacing;

    [HideInInspector]
    protected float verticalRaySpacing;

    protected RaycastOrigins _raycast_origins;

    [HideInInspector]
    public BoxCollider2D collider2d;

    public virtual void Awake()
    {
        collider2d = GetComponent<BoxCollider2D>();        
    }

    public virtual void Start()
    {
        CalculateRaySpacing();
    }

    protected void UpdateRaycastOrigins()
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
}
