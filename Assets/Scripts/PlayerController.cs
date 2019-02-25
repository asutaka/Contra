using UnityEngine;

public class PlayerController : MonoBehaviour
{
    private GameObject currentProjectile;
    public GameObject basicProjectile;

    public float moveSpeed;
    public float jumpHeight;
    public float gravity;
    public float shootDelay;
    private float shootDelayCounter;
    //horizontal
    private float hsp;
    //vertical
    private float vsp;

    public float pixelSize;
    public int direction;

    public float[] shootAngles;
    private Quaternion rot;

    private Transform currentShootPoint;
    public Transform[] shootPoints;

    public LayerMask solid;
    public LayerMask oneway;

    private bool keyLeft;
    private bool keyRight;
    private bool keyUp;
    private bool keyDown;
    private bool keyJump;
    private bool keyAction;
    private bool keyJumpOff;

    private bool onGround;
    private bool jumped;
    private bool moving;
    private bool onPlatform;
    private bool obsticlenOnLeft;
    private bool obsticlenOnRight;

    private Vector2 botLeft;
    private Vector2 botRight;
    private Vector2 topLeft;
    private Vector2 topRight;

    private Animator[] animators;
    // Use this for initialization
    void Start()
    {
        currentProjectile = basicProjectile;
        animators = GetComponentsInChildren<Animator>();
        shootDelayCounter = 0;
        rot = new Quaternion(0,0,0,0);
    }

    // Update is called once per frame
    void Update()
    {
        CalculateBounds();
        onGround =
            CheckCollision(botLeft, Vector2.down, pixelSize, solid) ||
            CheckCollision(botRight, Vector2.down, pixelSize, solid) ||
            CheckCollision(botLeft, Vector2.down, pixelSize, oneway) ||
            CheckCollision(botRight, Vector2.down, pixelSize, oneway);
        onPlatform =
            CheckCollision(botLeft, Vector2.down, pixelSize, oneway) ||
            CheckCollision(botRight, Vector2.down, pixelSize, oneway);

        obsticlenOnLeft = CheckCollision(topLeft, Vector2.left, pixelSize, solid) || CheckCollision(botLeft, Vector2.left, pixelSize, solid);
        obsticlenOnRight = CheckCollision(topRight, Vector2.right, pixelSize, solid) || CheckCollision(botRight, Vector2.right, pixelSize, solid);

        getInput();
        CalculateDirection();
        CalculateShootAngles();
        CalculateShootPoint();
        Animate();
        Move();
        Shoot();
    }

    void getInput()
    {
        keyLeft = Input.GetKey(KeyCode.LeftArrow);
        keyRight = Input.GetKey(KeyCode.RightArrow);
        keyUp = Input.GetKey(KeyCode.UpArrow);
        keyDown = Input.GetKey(KeyCode.DownArrow);
        keyJump = Input.GetKeyDown(KeyCode.Z);
        keyAction = Input.GetKey(KeyCode.X);
        keyJumpOff = keyDown && keyJump;
    }

    void Move()
    {
        if (keyLeft && !obsticlenOnLeft)
        {
            hsp = -moveSpeed * Time.deltaTime;
            transform.localScale = new Vector3(-1, 1, 1);
        }
        if (keyRight && !obsticlenOnRight)
        {
            hsp = moveSpeed * Time.deltaTime;
            transform.localScale = new Vector3(1, 1, 1);
        }
        if ((!keyLeft && !keyRight) || (keyLeft && keyRight))
        {
            moving = false;
            hsp = 0;
        }
        if(keyRight || keyLeft)
        {
            moving = true;
        }

        if (onPlatform && keyJumpOff)
        {
            transform.position = new Vector2(transform.position.x, transform.position.y - pixelSize);
            onGround = false;
        }

        if (keyJump && onGround)
        {
            jumped = true;
            vsp = jumpHeight;
            onGround = false;
        }

        if (!onGround)
        {
            vsp -= gravity * Time.deltaTime;
        }
        //solid
        if ((vsp < 0) && (CheckCollision(botLeft, Vector2.down, Mathf.Abs(vsp), solid) || CheckCollision(botRight, Vector2.down, Mathf.Abs(vsp), solid)))
        {
            float dist1 = CheckCollisionDistance(botLeft, Vector2.down, Mathf.Abs(vsp), solid);
            float dist2 = CheckCollisionDistance(botRight, Vector2.down, Mathf.Abs(vsp), solid);

            if (dist1 <= dist2)
                vsp = -dist1;
            else
                vsp = -dist2;

            transform.position = new Vector2(transform.position.x + hsp, transform.position.y + vsp + pixelSize / 2);
            vsp = 0;
        }
        //oneway
        if ((vsp < 0) && (CheckCollision(botLeft, Vector2.down, Mathf.Abs(vsp), oneway) || CheckCollision(botRight, Vector2.down, Mathf.Abs(vsp), oneway)))
        {
            float dist1 = CheckCollisionDistance(botLeft, Vector2.down, Mathf.Abs(vsp), oneway);
            float dist2 = CheckCollisionDistance(botRight, Vector2.down, Mathf.Abs(vsp), oneway);

            if (dist1 <= dist2)
                vsp = -dist1;
            else
                vsp = -dist2;

            transform.position = new Vector2(transform.position.x + hsp, transform.position.y + vsp + pixelSize / 2);
            vsp = 0;
        }

        if ((vsp > 0) && (CheckCollision(topLeft, Vector2.up, vsp, solid) || CheckCollision(topRight, Vector2.up, vsp, solid)))
        {
            float dist1 = CheckCollisionDistance(topLeft, Vector2.up, vsp, solid);
            float dist2 = CheckCollisionDistance(topRight, Vector2.up, vsp, solid);

            if (dist1 <= dist2)
                vsp = dist1;
            else
                vsp = dist2;

            transform.position = new Vector2(transform.position.x, transform.position.y + vsp + pixelSize / 2);
            vsp = 0;
        }

        if ((hsp > 0) && (CheckCollision(topRight, Vector2.right, hsp, solid) || CheckCollision(botRight, Vector2.right, hsp, solid)))
        {
            float dist1 = CheckCollisionDistance(topRight, Vector2.right, hsp, solid);
            float dist2 = CheckCollisionDistance(botRight, Vector2.right, hsp, solid);
            if (dist1 <= dist2) hsp = dist1;
            else hsp = dist2;
            transform.position = new Vector2(transform.position.x + hsp, transform.position.y);
            hsp = 0;
        }

        if ((hsp < 0) && (CheckCollision(topLeft, Vector2.left, Mathf.Abs(hsp), solid) || CheckCollision(botLeft, Vector2.left, Mathf.Abs(hsp), solid)))
        {
            float dist1 = CheckCollisionDistance(topLeft, Vector2.left, Mathf.Abs(hsp), solid);
            float dist2 = CheckCollisionDistance(botLeft, Vector2.left, Mathf.Abs(hsp), solid);
            if (dist1 <= dist2) hsp = -dist1;
            else hsp = -dist2;
            transform.position = new Vector2(transform.position.x + hsp, transform.position.y);
            hsp = 0;
        }

        if(vsp == 0)
        {
            jumped = false;
        }

        transform.position = new Vector2(transform.position.x + hsp, transform.position.y + vsp);
    }

    private void Shoot()
    {
        if(keyAction && shootDelayCounter <= 0)
        {
            if((currentProjectile == basicProjectile) && FindObjectsOfType<Projectile>().Length < 4)
            {
                Instantiate(currentProjectile, currentShootPoint.position, rot);
                shootDelayCounter = shootDelay;
            }
        }
        shootDelayCounter -= Time.deltaTime;
    }

    void CalculateDirection()
    {
        if(keyUp && !keyRight && !keyLeft && !keyDown) direction = 8;
        else if (jumped && keyDown && !keyRight && !keyLeft) direction = 2;
        else if (transform.localScale.x > 0)
        {
            if(keyUp && keyRight) direction = 9;
            else if(keyDown && keyRight) direction = 3;
            else if( keyDown && !keyRight) direction = 6;
            else direction = 6;
        }
        else if (transform.localScale.x < 0)
        {
            if (keyUp && keyLeft) direction = 7;
            else if (keyDown && keyLeft) direction = 1;
            else if (keyDown && !keyLeft) direction = 4;
            else direction = 4;
        }
    }
    void CalculateShootAngles()
    {
        if(direction == 8) rot = Quaternion.Euler(transform.rotation.x, transform.rotation.y, shootAngles[0]);
        if (direction == 9) rot = Quaternion.Euler(transform.rotation.x, transform.rotation.y, -shootAngles[1]);
        if (direction == 6) rot = Quaternion.Euler(transform.rotation.x, transform.rotation.y, -shootAngles[2]);
        if (direction == 3) rot = Quaternion.Euler(transform.rotation.x, transform.rotation.y, -shootAngles[3]);
        if (direction == 2) rot = Quaternion.Euler(transform.rotation.x, transform.rotation.y, shootAngles[4]);
        if (direction == 7) rot = Quaternion.Euler(transform.rotation.x, transform.rotation.y, shootAngles[1]);
        if (direction == 4) rot = Quaternion.Euler(transform.rotation.x, transform.rotation.y, shootAngles[2]);
        if (direction == 1) rot = Quaternion.Euler(transform.rotation.x, transform.rotation.y, shootAngles[3]);
    }

    void CalculateShootPoint()
    {
        if (onGround && direction == 8) currentShootPoint = shootPoints[0];
        if (!jumped && (direction == 9 || direction == 7)) currentShootPoint = shootPoints[1];
        if (!jumped && (direction == 4 || direction == 6)) currentShootPoint = shootPoints[2];
        if (!jumped && (direction == 1 || direction == 3)) currentShootPoint = shootPoints[3];
        if (!jumped && keyDown && !moving) currentShootPoint = shootPoints[4];
        if (!onGround && keyDown) currentShootPoint = shootPoints[2];
        if (jumped) currentShootPoint = transform;
    }
    private bool CheckCollision(Vector2 raycastOrigin, Vector2 direction, float distance, LayerMask layer)
    {
        return Physics2D.Raycast(raycastOrigin, direction, distance, layer);
    }

    private float CheckCollisionDistance(Vector2 raycastOrigin, Vector2 direction, float distance, LayerMask layer)
    {
        int i = 0;
        while (Physics2D.Raycast(raycastOrigin, direction, distance, layer))
        {
            i++;

            if (distance > pixelSize)
                distance -= pixelSize;
            else
                distance = pixelSize;

            if (i > 1000)
                return 0;
        }
        return distance;
    }
    private void CalculateBounds()
    {

        Bounds b = GetComponent<BoxCollider2D>().bounds;
        topLeft = new Vector2(b.min.x, b.max.y);
        botLeft = new Vector2(b.min.x, b.min.y);
        topRight = new Vector2(b.max.x, b.max.y);
        botRight = new Vector2(b.max.x, b.min.y);
    }

    //Các biến dùng trong Animator
    private void Animate()
    {
        for (int i = 0; i < animators.Length; i++)
        {
            animators[i].SetBool("OnGround",onGround);
            animators[i].SetBool("Jumped", jumped);
            animators[i].SetBool("Moving", moving);
            animators[i].SetFloat("VSP", vsp);
            animators[i].SetInteger("Direction",direction);
            animators[i].SetBool("Shooting",keyAction);
            animators[i].SetBool("KeyDown", keyDown);
        }
    }
}
