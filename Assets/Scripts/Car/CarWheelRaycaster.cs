using UnityEngine;

public enum SurfaceType
{
    Ground = 8,
    Road = 7,
    Ice = 6
}

public class CarWheelRaycaster : MonoBehaviour
{
    [SerializeField] private LayerMask layer;
    [SerializeField] private Rigidbody rb;
    [Min(0)]
    [SerializeField] private float rayLength;
    [SerializeField] private float springCoefficient = 60000;
    [SerializeField] private float dampingCoefficient = 200000;

    [Range(0, 1)]
    [SerializeField] private float targetDisplacement = .5f;
    private Vector3 normal = Vector3.up;
    private float hitDistance;
    private int hitLayer;
    public Vector3 Normal => normal;
    public bool isGrounded {get; private set;}
    public SurfaceType surface => (SurfaceType) hitLayer;

    public Vector3 localUp;

    private float previousHitDistance;

    void FixedUpdate()
    {
        isGrounded = false;
        if (Physics.Raycast(transform.position, -transform.up, out var hit, rayLength, layer.value)) 
        {
            normal = hit.normal;
            hitDistance = hit.distance;
            hitLayer = hit.collider.gameObject.layer;
            isGrounded = Vector3.Dot(localUp, normal) > .7f;
        }
        if (isGrounded)
        {
            float springVelocity = hitDistance - previousHitDistance;
            previousHitDistance = hitDistance;
            float springForce = springCoefficient * (hitDistance - (rayLength * targetDisplacement));
            float dampingForce = dampingCoefficient * springVelocity;
            rb.AddForceAtPosition(-hit.normal * (springForce + dampingForce), transform.position);
        }
    }

    void OnDrawGizmos()
    {
        Vector3 lineEnd = transform.position - transform.up * rayLength;
        Gizmos.color = new(0, 0.7f, 1);
        Gizmos.DrawLine(transform.position, lineEnd);
        if (isGrounded) Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(lineEnd, .15f);
        Gizmos.color = new(.8f, .3f, 1);
        Gizmos.DrawSphere(transform.position + (-transform.up * rayLength * targetDisplacement), .08f);
    }

}