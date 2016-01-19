using UnityEngine;
using System.Collections;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class CameraController : MonoBehaviour
{

    [Header("Camera Target")]

    [Tooltip("Drag your vehicle here. This is the GameObject with the VehicleController script attached")]
    public GameObject carObj;

    [Header("Camera Settings")]

    [Tooltip("By default you look at the vehicle centre, change the X values to look more to the sides, Y to look above or below and Z to look infront of or behind your vehicle")]
    [ContextMenuItem("Reset 'Look At Offset' to default", "resetLookAtOffset")]
    public Vector3 lookAtOffset = new Vector3(0f, 0f, 0f);
    [Tooltip("This will move the camera up/down, left or right, back or forward all the while aiming at the LookAt position.")]
    [ContextMenuItem("Reset 'Follow Position' to default", "resetFollowPosition")]
    public Vector3 followPosition = new Vector3(0f, 2f, -5f);
    [Tooltip("When checked the camera will follow behind the vehicle (like a proper racing game). When unchecked the camera will not follow behind (best for isometric or top down camera)")]
    public bool useCarRotation = true;
    [Tooltip("How much the camera lags/drags behind the vehicle when it accelerates (this increases the sense of speed). 0 means the camera will not follow, 100 means the camera sticks like glue to the vehicle")]
    [Range(0f, 100f)]
    [ContextMenuItem("Reset 'Follow Speed' to default", "resetFollowSpeed")]
    public float followSpeed = 5f;
    [Tooltip("how much the camera distorts perspective as the vehicle accelerates. When the car slows the distorition lessens")]
    [Range(0f, 3f)]
    [ContextMenuItem("Reset 'Field Of View Effect' to default", "resetFieldOfViewEffect")]
    public float FieldOfViewEffect = 0f;



    //private Transform lookPos;
    private Vector3 goalPos;
    private Vector3 finalLookPos;
    private Camera myCam;
    private float initFOV;
    private GameObject physicsBody;
    private bool useVehicleUpVector = true;

#if UNITY_EDITOR
    private void resetLookAtOffset()
    {
        lookAtOffset = new Vector3(0f, 0f, 0f);
        EditorUtility.SetDirty(this);
    }
    private void resetFollowPosition()
    {
        followPosition = new Vector3(0f, 2f, -5f);
        EditorUtility.SetDirty(this);
    }
    private void resetFollowSpeed()
    {
        followSpeed = 5f;
        EditorUtility.SetDirty(this);
    }
    private void resetFieldOfViewEffect()
    {
        FieldOfViewEffect = 0f;
        EditorUtility.SetDirty(this);
    }
#endif


    // Use this for initialization
    void Start()
    {
        Application.targetFrameRate = 60;
        //physicsBody = carObj.GetComponent<vehicleController>().physicsBody;
        physicsBody = carObj;
        //lookPos = physicsBody.transform;
        myCam = GetComponent<Camera>();
        initFOV = myCam.fieldOfView;
    }

    // Update is called once per frame
    void Update()
    {
        //pinch camera based on speed of vehicle//
        float fov = initFOV + (carObj.GetComponent<VehicleController>().zVel * FieldOfViewEffect);
        if (fov < initFOV)
        {
            fov = initFOV;
        }
        else if (fov > initFOV * 2.5f)
        {
            fov = initFOV * 2.5f;
        }
        myCam.fieldOfView = fov;

        if (followSpeed >= 100f)
        {
            //moveCamera2();
            moveCamera();
        }
    }

    void FixedUpdate()
    {
        if (followSpeed < 100f)
        {
            moveCamera();
        }
    }

    private void OnDrawGizmos()
    {
        if (Application.isPlaying)
        {
            Gizmos.color = Color.cyan;

            // draw center track point
            Gizmos.DrawSphere(transform.position, 1f);

            // draw center track point direction
            Vector3 lookDirection = (finalLookPos - transform.position).normalized;
            Gizmos.DrawLine(transform.position, transform.position + lookDirection * 10f);
        }
    }

    void moveCamera()
    {
        if (useCarRotation)
        {
            goalPos = carObj.transform.position + (carObj.transform.forward * followPosition.z) + (carObj.transform.up * followPosition.y) + (carObj.transform.right * followPosition.x);
            finalLookPos = carObj.transform.position + (carObj.transform.forward * lookAtOffset.z) + (carObj.transform.up * lookAtOffset.y) + (carObj.transform.right * lookAtOffset.x);
        }
        else
        {
            goalPos = carObj.transform.position + followPosition;
            finalLookPos = carObj.transform.position + lookAtOffset;
        }

        // set camera position
        transform.position = Vector3.Lerp(transform.position, goalPos, followSpeed * Time.deltaTime);
        
        // set camera rotation
        if (useVehicleUpVector) // 出现上下翻转的路段时，用这种camera的旋转方式
        {
            Quaternion lookRotaion = Quaternion.LookRotation(finalLookPos - transform.position, carObj.transform.up);
            transform.rotation = Quaternion.Lerp(transform.rotation, lookRotaion, Time.deltaTime * 10f);

            //transform.LookAt(finalLookPos, carObj.transform.up);
        }
        else transform.LookAt(finalLookPos);
    }

    void moveCamera2()
    {
        if (useCarRotation)
        {
            goalPos = physicsBody.transform.position + (physicsBody.transform.forward * followPosition.z) + (physicsBody.transform.up * followPosition.y) + (physicsBody.transform.right * followPosition.x);
            finalLookPos = physicsBody.transform.position + (physicsBody.transform.forward * lookAtOffset.z) + (physicsBody.transform.up * lookAtOffset.y) + (physicsBody.transform.right * lookAtOffset.x);
        }
        else
        {
            goalPos = physicsBody.transform.position + followPosition;
            finalLookPos = physicsBody.transform.position + lookAtOffset;
        }
        transform.position = Vector3.Lerp(transform.position, goalPos, followSpeed * Time.deltaTime);
        transform.LookAt(finalLookPos);
    }

    public void SetUseVehicleUpVector(bool value)
    {
        useVehicleUpVector = value;
    }
}