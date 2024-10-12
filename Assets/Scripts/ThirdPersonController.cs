using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThirdPersonController : MonoBehaviour
{
    [Header("References")]
    public Rigidbody rb;
    public Transform planet;
    public Transform playerObject;
    public Transform cameraTransform;

    [Header("Movement")]
    public float speed = 10f;
    public float rotationSpeed = 10f;
    public float jumpForce = 10f;

    protected Vector3 gravity;
    public float gravitySpeed = 9.8f;

    public bool isGrounded = false;

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    private void Update()
    {
        Jump();
        Fall();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        float horizontalInput = Input.GetAxis("Horizontal");
        float verticalInput = Input.GetAxis("Vertical");

        Vector3 camForward = cameraTransform.forward;
        Vector3 camRight = cameraTransform.right;
        camForward.y = 0f;
        camRight.y = 0f;

        Vector3 forwardRelative = verticalInput * camForward;
        Vector3 rightRelative = horizontalInput * camRight;

        Vector3 targetDirection = (forwardRelative + rightRelative).normalized;

        if (targetDirection != Vector3.zero)
        {
            // Berechne die Zielrotation basierend auf der Bewegungsrichtung
            Quaternion targetRotation = Quaternion.LookRotation(targetDirection);

            // Interpoliere die Rotation smooth
            Quaternion smoothRotation = Quaternion.Slerp(
                rb.rotation,
                targetRotation,
                rotationSpeed * Time.fixedDeltaTime
            );

            // Setze die Rotation des Rigidbody
            rb.MoveRotation(smoothRotation);

            // Interpoliere die Position smooth
            rb.MovePosition(rb.position + targetDirection * speed * Time.fixedDeltaTime);
        }

        Debug.DrawLine(transform.position, transform.position + targetDirection * 50f, Color.green);
    }

    private void Jump()
    {
        if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
        {
            rb.AddForce(transform.up * jumpForce, ForceMode.Impulse);
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            isGrounded = true;
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            isGrounded = false;
        }
    }

    private void Fall()
    {
        //gravity = (planet.position - transform.position).normalized * gravitySpeed;
        gravity = Vector3.down * gravitySpeed;
        rb.AddForce(gravity, ForceMode.Acceleration);
    }
}
