using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class PlayerController : MonoBehaviour
{
    [Header("Initialize")]
    public GameObject activeModel;
    public int user_id = 0;
    public int health;
    public int max_hp;
    public int buff;

    [Header("Inputs")]
    public float vertical;  // stores vertical input.
    public float horizontal; // stores horizontal input.
    public float mouseX;
    public float mouseY;

    [Header("Net Inputs")]
    public Vector3 moveDir;     //stores the moving vector value of main character.
    public Vector3 lastLogicPosition;
    public Vector3 logicPosition;
    public Vector3 tmpPosition;
    public float acc_time, nextLogicTime;
    public float moveSpeed;
    public Vector3 targetDir;       //store for hero's rotation 

    // Start is called before the first frame update

    [Header("States")]
    public bool canMove;    //shows you can move or not
    public Vector3 attackDir;
    public bool isDead = false;
    public bool isHit = false;
    public bool isDizzy = false;

    public bool normalAttackDown;   //stores whether you do normal attack or not
    public bool comboAttackDown;       //stores whether you combo or not
    public bool normalAttack;   //stores whether you do normal attack or not
    public bool comboAttack;       //stores whether you combo or not
    public bool normalAttackUp;
    public bool comboAttackUp;

    [Header("Configs")]
    public float mouseSpeed = 20f;  //speed of mouse that control shooting
    public float offset = 0.7f; //distance between arrow or attack area and hero
    //float fixedDelta;        //stores Time.fixedDeltaTime

    [Header("GameObjects")]
    public GameObject arrowPrefab;
    public GameObject comboAreaPrefab;
    public GameObject normalAreaPrefab;
    public GameObject comboArea;
    public GameObject normalArea;
    public PlayerManager playerManager;

    [Header("Components")]
    public Rigidbody rigid;     //for caching Rigidbody component
    public Animator anim;      //for caching Animator component

    bool enableAttacksUpdate = false;
    bool enableHealthUpdate = false;

    void Start()
    {
        playerManager = GameObject.Find("MainCanvas").GetComponent<PlayerManager>();
        SetupAnimator();
        rigid = GetComponent<Rigidbody>();
        normalAreaPrefab = Resources.Load<GameObject>("Prefabs/normalArea");
        comboAreaPrefab = Resources.Load<GameObject>("Prefabs/comboArea");
        arrowPrefab = Resources.Load<GameObject>("Prefabs/Arrow");
        acc_time = nextLogicTime = 0.0f;
        enableAttacksUpdate = true;
        StartCoroutine(UpdateNormaAttacksCoroutine());
        enableHealthUpdate = true;
        StartCoroutine(UpdateHealthCorotine());
    }
    IEnumerator UpdateNormaAttacksCoroutine()
    {
        while (enableAttacksUpdate)
        {
            UpdateNormalNumbers();
            yield return new WaitForSeconds(3f);
        }
    }
    void UpdateNormalNumbers()
    {
        GameManager.CallNet("update_normal_numbers", new object[] { GameManager.user_id });
    }
    IEnumerator UpdateHealthCorotine()
    {
        while (enableHealthUpdate)
        {
            UpdateHealth();
            yield return new WaitForSeconds(1f);
        }
    }
    void UpdateHealth()
    {
        GameManager.CallNet("update_health", new object[] { GameManager.user_id });
    }
    void SetupAnimator()//Setting up Animator component in the hierarchy.
    {
        if (activeModel == null)
        {
            anim = GetComponentInChildren<Animator>();//Find animator component in the children hierarchy.
            if (anim == null)
            {
                Debug.Log("No model");
            }
            else
            {
                activeModel = anim.gameObject; //save this gameobject as active model.
            }
        }

        if (anim == null)
            anim = activeModel.GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        acc_time = acc_time + Time.deltaTime;
        if (gameObject.tag == "myself" && playerManager.startResultFlag == false)
        {
            GetInput();     //getting control input from keyboard        
            UpdateStates();   //Updating anything related to character's actions.  
            while (acc_time >= nextLogicTime)
            {
                // game logic
                FixedTick(0.05f);
                nextLogicTime += 0.05f; // 100ms
            }
        }
        float interp = (acc_time + 0.05f - nextLogicTime) / 0.05f;
        Render(interp);
    }

    void GetInput()
    {
        vertical = Input.GetAxis("Vertical");    //for getting vertical input.
        horizontal = Input.GetAxis("Horizontal");    //for getting horizontal input.

        normalAttackDown = Input.GetMouseButtonDown(0); //for getting normal attack input.
        comboAttackDown = Input.GetMouseButtonDown(1);

        normalAttack = Input.GetMouseButton(0);
        comboAttack = Input.GetMouseButton(1);

        normalAttackUp = Input.GetMouseButtonUp(0); //for getting normal attack input.
        comboAttackUp = Input.GetMouseButtonUp(1);

        mouseX = Input.GetAxis("Mouse X");
        mouseY = Input.GetAxis("Mouse Y");
    }
    void UpdateStates() //updates hero's movement(rotation), attack targeting 
    {
        //update moveSpeed
        float targetSpeed = moveSpeed / 10;  //set run speed as target speed

        // update moveDir locally
        Vector3 v = vertical * Vector3.forward;
        Vector3 h = horizontal * Vector3.right;
        moveDir = (v + h).normalized * targetSpeed;

        //update normal_attack_area rotation on client
        if (normalAttackDown)
        {
            normalArea = Instantiate(
                        normalAreaPrefab,
                        transform.position + Vector3.up * offset,
                        Quaternion.Euler(new Vector3(0, transform.eulerAngles.y, 0)),
                        transform
                    );
            // Debug.Log("normalArea: " + normalArea);
        }
        if (normalAttack)
        {
            normalArea.transform.Rotate(0f, mouseSpeed * (mouseX), 0f, Space.Self);
        }
        // sync normal_attack performance
        if (normalAttackUp)
        {
            var arrow_rotation = normalArea.transform.eulerAngles;

            Destroy(normalArea);
            GameManager.CallNet("normal_attack", new object[] { GameManager.user_id, arrow_rotation.y });
        }
        //update combo_attack_area rotation on client
        if (comboAttackDown)
        {

            comboArea = Instantiate(
                       comboAreaPrefab,
                       transform.position + Vector3.up * offset,
                       Quaternion.Euler(new Vector3(0, transform.rotation.eulerAngles.y, transform.rotation.eulerAngles.z)),
                       transform
                   );
        }
        if (comboAttack)
        {
            comboArea.transform.Rotate(0f, mouseSpeed * (mouseX), 0f, Space.World);
        }
        //sync combo attack performance
        if (comboAttackUp)
        {
            var arrow_rotation = comboArea.transform.rotation.eulerAngles;
            Destroy(comboArea);
            GameManager.CallNet("combo_attack", new object[] { GameManager.user_id, arrow_rotation.x, arrow_rotation.y, arrow_rotation.z });
        }
    }
    void FixedTick(float d)
    {
        if (moveDir == Vector3.zero)
        {
            lastLogicPosition = logicPosition;
            return;
        }
        // lastLogicPosition = logicPosition;
        tmpPosition = lastLogicPosition + moveDir * d;
        GameManager.CallNet("move_to", new object[] {
            GameManager.user_id,
            tmpPosition.x, tmpPosition.z,
            lastLogicPosition.x, lastLogicPosition.z
        });
    }

    void Render(float interpolation)
    {
        if (logicPosition.x - lastLogicPosition.x != 0 || logicPosition.z - lastLogicPosition.z != 0)
        {
            anim.SetBool("canMove", true);
        }
        else
        {
            anim.SetBool("canMove", false);
        }
        var tmp = Vector3.Lerp(lastLogicPosition, logicPosition, interpolation);
        //rigid.MovePosition(tmp);
        transform.position = tmp;
        if (anim.GetBool("canMove"))
        {
            // Rotate
            Vector3 logic_v = (logicPosition.z - lastLogicPosition.z) * Vector3.forward;
            Vector3 logic_h = (logicPosition.x - lastLogicPosition.x) * Vector3.right;
            targetDir = (logic_v + logic_h).normalized * moveSpeed;
            targetDir.y = 0;
            if (targetDir != Vector3.zero)
            {
                Quaternion tr = Quaternion.LookRotation(targetDir);
                Quaternion targetRotation = Quaternion.Slerp(transform.rotation, tr, Time.deltaTime * moveSpeed * 5);
                transform.rotation = targetRotation;
            }
        }
        lastLogicPosition = logicPosition;
    }
    public void ExecuteMove(float x, float z)
    {
        
        Debug.Log("execute move: " + x.ToString() + " " + z.ToString());
        logicPosition = new Vector3(x, 0, z);
    }
    
      
    public void ExecuteNormalAttack(float y)
    {
        anim.SetBool("canMove", false);        
        anim.SetTrigger("normal");
        var arrow = Instantiate(arrowPrefab);
        // arrow.transform.SetParent(transform);
        arrow.transform.position = transform.position + transform.up * offset;
        transform.rotation = Quaternion.Euler(0, y, 0);
        arrow.transform.rotation = Quaternion.Euler(0, y-90, 0);
        // transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.Euler(0, y, 0), Time.deltaTime * moveSpeed * 5);
        // transform.eulerAngles = new Vector3(0, y, 0);
        var arrowController = arrow.GetComponent<ArrowController>();
        arrowController.isMine = (gameObject.tag == "myself");
        arrowController.isCombo = false;
        var arrowRigid = arrow.GetComponent<Rigidbody>();
        arrowRigid.velocity = Vector3.zero;
        arrowRigid.AddForce(
            Quaternion.Euler(0, y, 0) * new Vector3(0, 0, 10f),
            ForceMode.VelocityChange
        );
        Destroy(arrow, 0.5f);
    }
    public void ExecuteDead()
    {
        enableAttacksUpdate = false;
        enableHealthUpdate = false;
        anim.SetBool("canMove", false);
        anim.SetTrigger("isDie");
        Destroy(gameObject, 1f);
    }
    public void ExecuteComboAttack(float x, float y, float z)
    {
        attackDir = new Vector3(x, y, z);
        anim.SetTrigger("combo");
        for (int i = -2; i <= 2; i++)
        {
            GameObject arrow = Instantiate(arrowPrefab);
            // arrow.transform.SetParent(transform);
            arrow.transform.position = transform.position + transform.up * offset;
            transform.rotation = Quaternion.Euler(attackDir);
            arrow.transform.rotation = Quaternion.Euler(attackDir + new Vector3(0, i * 15-90, 0));
            var arrowController = arrow.GetComponent<ArrowController>();
            arrowController.isMine = (gameObject.tag == "myself");
            arrowController.isCombo = true;
            arrowController.shooterPos = transform.position;
            var arrowRigid = arrow.GetComponent<Rigidbody>();
            arrowRigid.velocity = Vector3.zero;
            arrowRigid.AddForce(
                Quaternion.Euler(attackDir + new Vector3(0, i * 15, 0)) * new Vector3(0, 0, 10f),
                ForceMode.VelocityChange
            );
            Destroy(arrow, 0.5f);
        }
    }
    public void ExecuteRepel(float x, float z)
    {        
        var targetPos = new Vector3(x, 0, z);
        if(transform.position == targetPos) return;
        anim.SetBool("canMove", false);
        anim.SetTrigger("isHit");
        logicPosition = targetPos;
        lastLogicPosition = logicPosition;    
        Debug.Log("execute repel: " + x.ToString() + " " + z.ToString());
    }
    void OnTriggerEnter(Collider collision)
    {
        if (collision.gameObject.tag == "grass")
        {
            gameObject.GetComponent<Renderer>().material.color = new Color(1f, 1f, 1f, 0.5f);
        }
    }
    // void OnTriggerStay(Collider collision)
    // {
    //     if (collision.gameObject.tag == "grass")
    //     {
    //         gameObject.GetComponent<Renderer>().material.color = new Color(1f, 1f, 1f, 0.5f);
    //     }
    // }

    //These mecanic detects whether you are on ground or not.
    void OnTriggerExit(Collider collision)
    {
        // if (collision.gameObject.tag == "Ground")
        // {
        //     onGround = false;
        //     anim.SetBool("onGround", false);
        // }
    }
}
