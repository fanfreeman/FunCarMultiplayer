using UnityEngine;
using System.Collections;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class VehicleController : MonoBehaviour
{
    [Header("Meshes")]

    [Tooltip("Add a vehicle body here. we recommend creating an empty GameObject (reset it's transforms) and put your car body parts inside.")]
    public GameObject vehicleBody;
    [Tooltip("Add a front left wheel here. This should be fine as is but the same rule as the body can apply if you wish")]
    public GameObject wheelLeftFront;
    [Tooltip("Add a front right wheel here. This should be fine as is but the same rule as the body can apply if you wish")]
    public GameObject wheelRightFront;
    [Tooltip("Add a back left wheel here. This should be fine as is but the same rule as the body can apply if you wish")]
    public GameObject wheelLeftBack;
    [Tooltip("Add a back right wheel here. This should be fine as is but the same rule as the body can apply if you wish")]
    public GameObject wheelRightBack;

    [Header("Physics Materials")]

    [Tooltip("Add a Physics Material for the tires, To edit in the inspector select the material in the project folder. we recommend a low dynamic and static friction of 0.1, and Minimum Friction Combine")]
    public PhysicMaterial tirePhysicsMat;
    [Tooltip("Add a Physics Material for the body of the vehicle, To edit in the inspector select the material in the project folder. increase Bounciness above 0 will cause stronger bounce on collision")]
    public PhysicMaterial bodyPhysicsMat;

    [Header("Vehicle Tuning")]

    [Tooltip("Want to go faster? Pick a higher number")]
    [Range(0f, 3000f)]
    [ContextMenuItem("Reset 'Horsepower' to default", "resetHorsepower")]
    public float horsepower = 220f;
    [Tooltip("Want responsive steering? Go higher!")]
    [Range(0f, 300f)]
    [ContextMenuItem("Reset 'Steering Capability' to default", "resetSteering")]
    public float steeringCapability = 70f;
    [Tooltip("Want to stop drifting sideways so much? Bigger number will help resist the soapy floor")]
    [Range(0f, 100f)]
    [ContextMenuItem("Reset 'Tire Grip' to default", "resetTireGrip")]
    public float tireGrip = 100f;
    [Tooltip("Want to help your wheels stay on the floor? This will hang the wheels a little more (1 = half wheel radius) 1 or less is recommended")]
    [Range(0f, 10f)]
    [ContextMenuItem("Reset 'Wheel Hang Distance' to default", "resetWheelHangDistance")]
    public float wheelHangDistance = 1f;
    [Tooltip("Want to make the body swing around like babies rattle? Higher value is better. Each axis can be tweaked. Most times Y can equal 0")]
    [ContextMenuItem("Reset 'Suspension Lengths' to default", "resetSuspensionLengths")]
    public Vector3 suspensionLengths = new Vector3(0.4f, 0f, 0.4f);
    [Tooltip("Want to limit the swing strength of the body? A higher value is less swing (more tension)")]
    [Range(0f, 100)]
    [ContextMenuItem("Reset 'Suspension Tension' to default", "resetSuspensionTension")]
    public float suspensionTension = 15f;
    [Tooltip("If you happen to drive through a puddle, this is the width of the trail renderer")]
    [ContextMenuItem("Reset 'Tire Trail Width' to default", "resetTireTrailWidth")]
    public float tireTrailWidth = 0.5f;

    [Header("Behaviour")]

    [Tooltip("Did you land on your head? This will fix that and flip you upright")]
    public bool autoCorrectRot = true;


    [Header("Vehicle Lights (Optional)")]
    public Renderer brakelights;
    public Material brakelightON;
    public Material brakelightOFF;
    public Renderer headlights;
    public Material headlightsON;
    public Material headlightsOFF;
    public Renderer indicatorLEFT;
    public Renderer indicatorRIGHT;
    public Material indicatorON;
    public Material indicatorOFF;
    private bool brakesON = false;
    private bool headsON = true;
    private bool indLeftON = false;
    private bool indRightON = false;

    [HideInInspector]
    public Rigidbody rbody;
    private Rigidbody jbody;
    [HideInInspector]
    public GameObject physicsBody;
    private GameObject colBody;
    [HideInInspector]
    public GameObject suspensionBody;
    private GameObject colSuspension;
    [HideInInspector]
    public GameObject wheels;
    private GameObject colLB;
    private GameObject colLF;
    private GameObject colRB;
    private GameObject colRF;
    private GameObject turnLF;
    private GameObject turnRF;
    //if car is destoryed, player should not controll it!;
    [HideInInspector]
    public bool isDestoryed;

    [HideInInspector]
    public float inputX;
    private float inputY;
    private float xVel;
    [HideInInspector]
    public float zVel;
    [HideInInspector]
    public bool touchLB;
    [HideInInspector]
    public bool touchLF;
    [HideInInspector]
    public bool touchRB;
    [HideInInspector]
    public bool touchRF;
    private int tiresOnGround;
    private int FtiresOnGround;
    private int BtiresOnGround;
    private float airTime;
    private float unstableTime = 0f;
    [HideInInspector]
    public bool roofOnGround = false;
    private Vector3 defaultCOG;
    private float boundSize = 1f;

    [Header("Camera")]
    public GameObject cameraObject;


#if UNITY_EDITOR
    private void resetHorsepower()
    {
        horsepower = 220f;
        EditorUtility.SetDirty(this);
    }
    private void resetSteering()
    {
        steeringCapability = 70f;
        EditorUtility.SetDirty(this);
    }
    private void resetTireGrip()
    {
        tireGrip = 100f;
        EditorUtility.SetDirty(this);
    }
    private void resetWheelHangDistance()
    {
        wheelHangDistance = 1f;
        EditorUtility.SetDirty(this);
    }
    private void resetSuspensionLengths()
    {
        suspensionLengths = new Vector3(0.4f, 0f, 0.4f);
        EditorUtility.SetDirty(this);
    }
    private void resetSuspensionTension()
    {
        suspensionTension = 15f;
        EditorUtility.SetDirty(this);
    }
    private void resetTireTrailWidth()
    {
        tireTrailWidth = 0.5f;
        EditorUtility.SetDirty(this);
    }
#endif

    void Awake()
    {
        isDestoryed = false;

        //make sure vehicleBody has the correct parent//
        vehicleBody.transform.SetParent(transform);

        // transfer the rotation on the parent object onto this object
        transform.localRotation = transform.rotation;
        transform.parent.localRotation = Quaternion.identity;

        //create physics body//
        physicsBody = new GameObject(gameObject.name + " physics");
        physicsBody.transform.position = (wheelLeftBack.transform.position + wheelLeftFront.transform.position + wheelRightBack.transform.position + wheelRightFront.transform.position) / 4;
        physicsBody.transform.rotation = transform.rotation;
        transform.position = physicsBody.transform.position;
        //physicsBody.AddComponent<VehiclePhysicsController>();

        //create a wheels holder
        wheels = new GameObject(gameObject.name + " wheels");
        wheels.transform.position = physicsBody.transform.position;
        wheels.transform.rotation = physicsBody.transform.rotation;

        //create rigidbody//
        rbody = physicsBody.AddComponent<Rigidbody>();
        rbody.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        rbody.mass = 100f;
        rbody.angularDrag = 5f;

        //set up the wheels//

        //rear left wheel//
        Mesh mesh = wheelLeftBack.GetComponentInChildren<MeshFilter>().mesh;
        getDiameter(mesh);
        colLB = new GameObject("colLB");
        SphereCollider col = colLB.AddComponent<SphereCollider>();
        if (wheelLeftBack.transform.childCount > 0)
        {
            col.radius = (boundSize / 2f) * wheelLeftBack.transform.GetChild(0).transform.lossyScale.y;
            colLB.transform.position = wheelLeftBack.transform.position;
        }
        else
        {
            col.radius = (boundSize / 2f) * wheelLeftBack.transform.lossyScale.y;
            colLB.transform.position = wheelLeftBack.transform.position + mesh.bounds.center;
        }
        col.material = tirePhysicsMat;
        col.center = new Vector3(0f, 0f, 0f);
        colLB.transform.SetParent(physicsBody.transform);
        wheel wheelScript = colLB.AddComponent<wheel>();
        wheelScript.vehicleObj = gameObject;
        wheelScript.wheelObj = wheelLeftBack;
        wheelScript.wheelLB = true;
        wheelScript.wheelHangDistance = wheelHangDistance;
        wheelScript.tireTrailWidth = tireTrailWidth;
        wheelLeftBack.transform.SetParent(wheels.transform);

        //front left wheel//
        mesh = wheelLeftFront.GetComponentInChildren<MeshFilter>().mesh;
        getDiameter(mesh);
        colLF = new GameObject("colLF");
        col = colLF.AddComponent<SphereCollider>();
        if (wheelLeftFront.transform.childCount > 0)
        {
            col.radius = (boundSize / 2f) * wheelLeftFront.transform.GetChild(0).transform.lossyScale.y;
            colLF.transform.position = wheelLeftFront.transform.position;
        }
        else
        {
            col.radius = (boundSize / 2f) * wheelLeftFront.transform.lossyScale.y;
            colLF.transform.position = wheelLeftFront.transform.position + mesh.bounds.center;
        }
        col.material = tirePhysicsMat;
        col.center = new Vector3(0f, 0f, 0f);
        colLF.transform.SetParent(physicsBody.transform);
        wheelScript = colLF.AddComponent<wheel>();
        wheelScript.vehicleObj = gameObject;
        wheelScript.wheelObj = wheelLeftFront;
        wheelScript.wheelLF = true;
        wheelScript.wheelHangDistance = wheelHangDistance;
        wheelScript.tireTrailWidth = tireTrailWidth;

        turnLF = new GameObject("turnLF");
        turnLF.transform.SetParent(wheels.transform);
        turnLF.transform.position = wheelLeftFront.transform.position;
        turnLF.transform.rotation = wheels.transform.rotation;
        wheelLeftFront.transform.SetParent(turnLF.transform);
        wheelScript.turnObj = turnLF;
        wheelScript.frontTire = true;

        //rear right wheel//
        mesh = wheelRightBack.GetComponentInChildren<MeshFilter>().mesh;
        getDiameter(mesh);
        colRB = new GameObject("colRB");
        col = colRB.AddComponent<SphereCollider>();
        if (wheelRightBack.transform.childCount > 0)
        {
            col.radius = (boundSize / 2f) * wheelRightBack.transform.GetChild(0).transform.lossyScale.y;
            colRB.transform.position = wheelRightBack.transform.position;
        }
        else
        {
            col.radius = (boundSize / 2f) * wheelRightBack.transform.lossyScale.y;
            colRB.transform.position = wheelRightBack.transform.position + mesh.bounds.center;
        }
        col.material = tirePhysicsMat;
        col.center = new Vector3(0f, 0f, 0f);
        colRB.transform.SetParent(physicsBody.transform);
        wheelScript = colRB.AddComponent<wheel>();
        wheelScript.vehicleObj = gameObject;
        wheelScript.wheelObj = wheelRightBack;
        wheelScript.wheelRB = true;
        wheelScript.wheelHangDistance = wheelHangDistance;
        wheelScript.tireTrailWidth = tireTrailWidth;
        wheelRightBack.transform.SetParent(wheels.transform);

        //front right wheel//
        mesh = wheelRightFront.GetComponentInChildren<MeshFilter>().mesh;
        getDiameter(mesh);
        colRF = new GameObject("colRF");
        col = colRF.AddComponent<SphereCollider>();
        if (wheelRightFront.transform.childCount > 0)
        {
            col.radius = (boundSize / 2f) * wheelRightFront.transform.GetChild(0).transform.lossyScale.y;
            colRF.transform.position = wheelRightFront.transform.position;
        }
        else
        {
            col.radius = (boundSize / 2f) * wheelRightFront.transform.lossyScale.y;
            colRF.transform.position = wheelRightFront.transform.position + mesh.bounds.center;
        }
        col.material = tirePhysicsMat;
        col.center = new Vector3(0f, 0f, 0f);
        colRF.transform.SetParent(physicsBody.transform);
        wheelScript = colRF.AddComponent<wheel>();
        wheelScript.vehicleObj = gameObject;
        wheelScript.wheelObj = wheelRightFront;
        wheelScript.wheelRF = true;
        wheelScript.wheelHangDistance = wheelHangDistance;
        wheelScript.tireTrailWidth = tireTrailWidth;

        turnRF = new GameObject("turnRF");
        turnRF.transform.SetParent(wheels.transform);
        turnRF.transform.position = wheelRightFront.transform.position;
        turnRF.transform.rotation = wheels.transform.rotation;
        wheelRightFront.transform.SetParent(turnRF.transform);
        wheelScript.turnObj = turnRF;
        wheelScript.frontTire = true;

        //create a body holder//
        suspensionBody = new GameObject(gameObject.name + " suspension");
        suspensionBody.transform.position = physicsBody.transform.position;
        suspensionBody.transform.rotation = physicsBody.transform.rotation;

        jbody = suspensionBody.AddComponent<Rigidbody>();
        jbody.useGravity = false;
        jbody.mass = 10f;
        jbody.drag = 1f;
        jbody.angularDrag = 2f;
        CharacterJoint joint = suspensionBody.AddComponent<CharacterJoint>();
        joint.connectedBody = rbody;
        joint.axis = new Vector3(1f, 0f, 0f);
        joint.swingAxis = new Vector3(1f, 0f, 0f);

        SoftJointLimitSpring temp = joint.twistLimitSpring;
        temp.spring = suspensionTension * 400f;
        temp.damper = 0.4f;
        joint.twistLimitSpring = temp;

        SoftJointLimit temp2 = joint.lowTwistLimit;
        temp2.limit = suspensionLengths.x * -1f;
        temp2.bounciness = 1f;
        joint.lowTwistLimit = temp2;

        SoftJointLimit temp3 = joint.highTwistLimit;
        temp3.limit = suspensionLengths.x;
        temp3.bounciness = 1f;
        joint.highTwistLimit = temp3;

        SoftJointLimitSpring temp4 = joint.swingLimitSpring;
        temp4.spring = suspensionTension * 400f;
        temp4.damper = 0.4f;
        joint.swingLimitSpring = temp4;

        SoftJointLimit temp5 = joint.swing1Limit;
        temp5.limit = suspensionLengths.y * 0.1f;
        temp5.bounciness = 1f;
        joint.swing1Limit = temp5;

        SoftJointLimit temp6 = joint.swing2Limit;
        temp6.limit = suspensionLengths.z;
        temp6.bounciness = 1f;
        joint.swing2Limit = temp6;

        //create body collider//
        colSuspension = new GameObject("colSuspension");
        colSuspension.transform.position = suspensionBody.transform.position;
        colSuspension.transform.rotation = suspensionBody.transform.rotation;
        colSuspension.transform.SetParent(suspensionBody.transform);
        BoxCollider col2 = colSuspension.AddComponent<BoxCollider>();
        float distX = Vector3.Distance(wheelLeftFront.transform.position, wheelRightFront.transform.position);
        float distZ = Vector3.Distance(wheelLeftFront.transform.position, wheelLeftBack.transform.position);
        col2.size = new Vector3(distX - (col.radius), (col.radius) * 0.5f, distZ + ((col.radius) * 2f));
        col2.center = new Vector3(0f, col.radius, 0f);
        col2.material = bodyPhysicsMat;

        jbody.centerOfMass = col2.center + new Vector3(0f, col.radius * 0.5f, 0f);
        rbody.centerOfMass = col2.center + new Vector3(0f, -col.radius * 0.5f, 0f);
        defaultCOG = rbody.centerOfMass;

        colBody = new GameObject("colBody");
        colBody.transform.position = physicsBody.transform.position;
        colBody.transform.rotation = physicsBody.transform.rotation;
        colBody.transform.SetParent(physicsBody.transform);
        BoxCollider col3 = colBody.AddComponent<BoxCollider>();
        col3.size = new Vector3(distX - col.radius, col.radius * 0.5f, distZ);
        col3.center = new Vector3(0f, col.radius, 0f);
        col3.material = bodyPhysicsMat;

        //rbody.isKinematic = true;
    }

    void getDiameter(Mesh mesh)
    {
        //of x y z, if two of them are equal, that must be the diameter//
        if (mesh.bounds.size.z == mesh.bounds.size.x)
        {
            boundSize = mesh.bounds.size.z;
        }
        else if (mesh.bounds.size.z == mesh.bounds.size.y)
        {
            boundSize = mesh.bounds.size.z;
        }
        else if (mesh.bounds.size.x == mesh.bounds.size.y)
        {
            boundSize = mesh.bounds.size.x;
        }
        //if the wheel is not round, go with the largest length for diameter//
        else
        {
            boundSize = mesh.bounds.size.z;

            if (mesh.bounds.size.x > boundSize)
            {
                boundSize = mesh.bounds.size.x;
            }
            if (mesh.bounds.size.y > boundSize)
            {
                boundSize = mesh.bounds.size.y;
            }
        }
    }

    // Use this for initialization
    void Start()
    {
        inputX = 0f;
        inputY = 0f;

        xVel = 0f;
        zVel = 0f;

        //prevent colliding with seams in ground//
        Physics.defaultContactOffset = 0.001f;
    }

    void Update()
    {
        if (isDestoryed) return;
        //track how many tires are touching the ground//
        tiresOnGround = 0;
        FtiresOnGround = 0;
        BtiresOnGround = 0;
        if (touchLB)
        {
            tiresOnGround++;
            BtiresOnGround++;
            airTime = 0f;
        }
        if (touchLF)
        {
            tiresOnGround++;
            FtiresOnGround++;
            airTime = 0f;
        }
        if (touchRB)
        {
            tiresOnGround++;
            BtiresOnGround++;
            airTime = 0f;
        }
        if (touchRF)
        {
            tiresOnGround++;
            FtiresOnGround++;
            airTime = 0f;
        }

        if (tiresOnGround <= 0)
        {
            airTime += Time.deltaTime;
        }

        if (tiresOnGround >= 4)
        {
            unstableTime = 0f;
        }
        else
        {
            unstableTime += Time.deltaTime;
        }

        //prevent being stuck upside down//
        if (autoCorrectRot)
        {
            Vector3 rayUp = physicsBody.transform.TransformDirection(Vector3.up);
            //Debug.DrawRay(physicsBody.transform.position, rayUp*5f, Color.green);
            RaycastHit[] hits;
            roofOnGround = false;
            hits = Physics.RaycastAll(physicsBody.transform.position, rayUp, 5f);

            if (hits.Length > 2)
            {
                roofOnGround = true;
            }

            if (tiresOnGround <= 0 && roofOnGround == true)
            {
                rbody.centerOfMass = new Vector3(0f, -5f * transform.localScale.y, 0f);
            }

            if (tiresOnGround >= 3)
            {
                rbody.centerOfMass = defaultCOG;
            }

            if (unstableTime > 5f)
            {
                if (rbody.velocity.magnitude < 2f)
                {
                    rbody.centerOfMass = new Vector3(0f, -5f * transform.localScale.y, 0f);

                    if (unstableTime > 10f)
                    {
                        physicsBody.transform.rotation = Quaternion.Euler(0f, 0f, 0f);
                        unstableTime = 6f;
                    }
                }
            }
        }

        //head lights//
        if (headlights)
        {
            //switch lights on/off by pressing L key//
            if (Input.GetKeyDown(KeyCode.L))
            {
                if (headsON)
                {
                    headlights.sharedMaterial = headlightsOFF;
                    headsON = false;
                }
                else
                {
                    headlights.sharedMaterial = headlightsON;
                    headsON = true;
                }
            }
        }

        //indicator lights//
        if (indicatorLEFT)
        {
            //pressing left//
            if (inputX < 0f)
            {
                if (!indLeftON)
                {
                    indicatorLEFT.sharedMaterial = indicatorON;
                    indLeftON = true;
                }
                float floor = 0f;
                float ceiling = 1f;
                float emission = floor + Mathf.PingPong(Time.time * 2f, ceiling - floor);
                indicatorLEFT.sharedMaterial.SetColor("_EmissionColor", new Color(1f, 1f, 1f) * emission);
            }
            //not pressing left//
            else
            {
                if (indLeftON)
                {
                    indicatorLEFT.sharedMaterial = indicatorOFF;
                    indLeftON = false;
                }
            }
        }
        if (indicatorRIGHT)
        {
            //pressing right//
            if (inputX > 0f)
            {
                if (!indRightON)
                {
                    indicatorRIGHT.sharedMaterial = indicatorON;
                    indRightON = true;
                }
                float floor = 0f;
                float ceiling = 1f;
                float emission = floor + Mathf.PingPong(Time.time * 2f, ceiling - floor);
                indicatorRIGHT.sharedMaterial.SetColor("_EmissionColor", new Color(1f, 1f, 1f) * emission);
            }
            //not pressing right//
            else
            {
                if (indRightON)
                {
                    indicatorRIGHT.sharedMaterial = indicatorOFF;
                    indRightON = false;
                }
            }
        }

        //brake lights//
        if (brakelights)
        {
            //braking//
            if (Input.GetKey(KeyCode.DownArrow) || Input.GetKey(KeyCode.S))
            {
                if (!brakesON)
                {
                    if (zVel > 0.01f)
                    {
                        brakelights.sharedMaterial = brakelightON;
                        brakesON = true;
                    }
                }
                else if (zVel < 0f)
                {
                    brakelights.sharedMaterial = brakelightOFF;
                    brakesON = false;
                }
            }
            //not braking//
            else if (brakesON)
            {
                brakelights.sharedMaterial = brakelightOFF;
                brakesON = false;
            }
        }
    }

    // Update is called once per frame
    public void Move(float steering, float accel, float footbrake, float handbrake)
    {
        if(isDestoryed)
        {
            return;
        }
        //input//
        //inputX = Input.GetAxis("Horizontal");
        //inputY = Input.GetAxis("Vertical");
        inputX = steering;
        inputY = accel;

        //track velocity//
        xVel = physicsBody.transform.InverseTransformDirection(rbody.velocity).x;
        zVel = physicsBody.transform.InverseTransformDirection(rbody.velocity).z;

        //accellerate forwards//
        if (inputY > 0)
        {
            if (FtiresOnGround > 0 || airTime < 0.6f)
            {
                rbody.AddForce(inputY * physicsBody.transform.forward * (horsepower * 400f) * Time.deltaTime);
            }
        }
        //accellerate backwards
        else
        {
            if (BtiresOnGround > 0)
            {
                rbody.AddForce(inputY * physicsBody.transform.forward * (horsepower * 400f) * Time.deltaTime);
            }
        }

        if (tiresOnGround > 0 || airTime < 0.3f)
        {
            //stop sliding sideways//
            rbody.AddForce(physicsBody.transform.right * xVel * -(tireGrip * 140f) * Time.deltaTime);

            //downforce//
            rbody.AddForce(physicsBody.transform.up * (Mathf.Abs(zVel) * -5000f) * Time.deltaTime);
        }
        else
        {
            if (airTime > 0.3f)
            {
                //increase gravity//
                rbody.AddForce((Physics.gravity * rbody.mass) * 2f);
            }
        }

        //steer//
        float y = inputX * Time.deltaTime * steeringCapability;
        if (y > Mathf.Abs(zVel * 0.2f))
        {
            y = Mathf.Abs(zVel * 0.2f);
        }
        else if (y < (Mathf.Abs(zVel * 0.2f)) * -1f)
        {
            y = (Mathf.Abs(zVel * 0.2f)) * -1f;
        }
        if (zVel < 0f) // steer while backing up
        {
            y *= -1f;
        }

        //steer//
        rbody.angularVelocity = new Vector3(rbody.angularVelocity.x, 0f, rbody.angularVelocity.z);
        if (airTime > 0f)
        {
            physicsBody.transform.Rotate(0f, y * 1.5f, 0f);
        }
        else
        {
            physicsBody.transform.Rotate(0f, y * 2f, 0f);
        }

        //smoothen out the visual movement//
        float smooth = Vector3.Distance(transform.position, physicsBody.transform.position) * 0.3f;
        smooth = 0.1f + smooth * smooth;

        transform.position = Vector3.Lerp(transform.position, physicsBody.transform.position, smooth);
        transform.rotation = Quaternion.Lerp(transform.rotation, physicsBody.transform.rotation, smooth / 2f);
        transform.rotation = Quaternion.Euler(transform.rotation.eulerAngles.x, physicsBody.transform.rotation.eulerAngles.y, transform.rotation.eulerAngles.z);

        Quaternion goalRot = Quaternion.Euler(suspensionBody.transform.localEulerAngles - transform.localEulerAngles);
        vehicleBody.transform.localRotation = Quaternion.Lerp(vehicleBody.transform.localRotation, goalRot, 0.3f);
    }

    void OnDestroy()
    {
        Destroy(wheels);
        Destroy(physicsBody);
        Destroy(suspensionBody);
    }

    void LateUpdate()
    {
        if(isDestoryed)
        {
            return;
        }

        wheels.transform.position = transform.position;
        wheels.transform.rotation = transform.rotation;
    }

    public void SetToDestory(bool i = true)
    {
        isDestoryed = true;
    }

}