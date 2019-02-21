using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float moveSpeed;
    public float jumpHeight;
    public float gravity;
    //horizontal
    private float hsp;
    //vertical
    private float vsp;

    public float pixelSize;

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
    private bool onPlatform;
    private bool obsticlenOnLeft;
    private bool obsticlenOnRight;

    private Vector2 botLeft;
    private Vector2 botRight;
    private Vector2 topLeft;
    private Vector2 topRight;
    // Use this for initialization
    void Start()
    {

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
        Move();
    }

    void getInput()
    {
        keyLeft = Input.GetKey(KeyCode.LeftArrow);
        keyRight = Input.GetKey(KeyCode.RightArrow);
        keyUp = Input.GetKey(KeyCode.UpArrow);
        keyDown = Input.GetKey(KeyCode.DownArrow);
        keyJump = Input.GetKeyDown(KeyCode.Z);
        keyAction = Input.GetKey(KeyCode.LeftControl);
        keyJumpOff = keyDown && keyJump;
    }

    void Move()
    {
        if (keyLeft && !obsticlenOnLeft) hsp = -moveSpeed * Time.deltaTime;
        if (keyRight && !obsticlenOnRight) hsp = moveSpeed * Time.deltaTime;
        if ((!keyLeft && !keyRight) || (keyLeft && keyRight)) hsp = 0;

        if (onPlatform && keyJumpOff)
        {
            transform.position = new Vector2(transform.position.x, transform.position.y - pixelSize);
            onGround = false;
        }

        if (keyJump && onGround)
        {
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

        transform.position = new Vector2(transform.position.x + hsp, transform.position.y + vsp);
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
}
