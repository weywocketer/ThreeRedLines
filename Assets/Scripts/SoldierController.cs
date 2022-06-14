using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoldierController : MonoBehaviour
{
    Animator animator;
    RegimentController regimentController;
    private AudioSource audioSource;
    [SerializeField] GameObject smoke;
    SpriteRenderer spriteRenderer;
    int deathAnimationsNumber = 1;
    int meeleAnimationsNumber = 2;

    public float speed = 1;
    float animatorSpeed;

    float walkingPrecision;

    public Vector3 basePosition; // local postion in the formation, without any offset
    public float xOffset;
    public float zOffset; // used for ReformLine purposes
    private float xScatter;
    private float zScatter;

    // Start is called before the first frame update
    void Start()
    {
        animator = GetComponent<Animator>();
        regimentController = GetComponentInParent<RegimentController>();
        audioSource = GetComponent<AudioSource>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        animatorSpeed = Random.Range(0.9f, 1.1f);
        animator.speed = animatorSpeed;

        basePosition = transform.localPosition;

        xScatter = regimentController.xScatter;
        zScatter = regimentController.zScatter;
        transform.Translate(transform.right * Random.Range(-xScatter, xScatter));
        transform.Translate(transform.forward * Random.Range(-zScatter, zScatter));
        xOffset = transform.localPosition.x;
        zOffset = transform.localPosition.z;

        regimentController.animationChanged.AddListener(UpdateAnimationState);

        walkingPrecision = regimentController.walkingPrecision;
    }

    // Update is called once per frame
    void Update()
    {
        // consider managing animations from RegimentController through funcion called only when needed (or maybe event listener?)
        // consider seting animantion states through event listeners...
        //animator.SetBool("walk", regimentController.walking);
        //animator.SetBool("present", regimentController.fireing);

        animator.SetBool("walk", regimentController.walking);

        if (xOffset - transform.localPosition.x > walkingPrecision)
        {
            animator.SetBool("walk", true);
            transform.Translate(Vector3.right * Time.deltaTime * speed);
        }
        else if (xOffset - transform.localPosition.x < -walkingPrecision)
        {
            animator.SetBool("walk", true);
            transform.Translate((-1) * Vector3.right * Time.deltaTime * speed);
        }

        if (zOffset - transform.localPosition.z > walkingPrecision)
        {
            animator.SetBool("walk", true);
            transform.Translate(Vector3.forward * Time.deltaTime * speed);
        }
        else if (zOffset - transform.localPosition.z < -walkingPrecision)
        {
            animator.SetBool("walk", true);
            transform.Translate((-1) * Vector3.forward * Time.deltaTime * speed);
        }

    }

    public void Shoot()
    {
        Instantiate(smoke, transform.position, transform.rotation);
        audioSource.Play();
    }

    public void Die()
    {
        //transform.Rotate(Vector3.forward, 90);
        //animator.SetBool("walk", false);

        if (Random.Range(0,2) == 1)
        {
            spriteRenderer.flipX = true;
        }

        animator.SetInteger("die", Random.Range(1, deathAnimationsNumber+1));

        transform.SetParent(GameObject.Find("Corpses").transform);
        gameObject.isStatic = true;
        
        Destroy(this);
    }

    public void Flee()
    {
        xOffset += Random.Range(-3.0f, 3.0f);
        StartCoroutine(FlipX());
    }

    public void Reorganise()
    {
        //xOffset = basePosition.x + Random.Range(-0.15f, 0.15f);
        StartCoroutine(FlipX());
    }


    IEnumerator FlipX()
    {
        yield return new WaitForSeconds(Random.Range(0.1f, 2.0f));
        spriteRenderer.flipX = !spriteRenderer.flipX;
    }

    public void SetBasePositionAndOffsets(Vector3 position) // used for ReformLine purposes
    {
        basePosition = position;
        xOffset = basePosition.x + Random.Range(-xScatter, xScatter);
        zOffset = basePosition.z + Random.Range(-zScatter, zScatter);
    }

    private void UpdateAnimationState()
    {
        animator.SetBool("walk", regimentController.walking);
        animator.SetBool("present", regimentController.fireing);
        animator.SetBool("reload", regimentController.reloading);
        if (regimentController.meele)
        {
            animator.SetInteger("meele", Random.Range(1, meeleAnimationsNumber + 1));
        }
        else
        {
            animator.SetInteger("meele", 0);
        }
        animator.speed = animatorSpeed;
    }

    public void AdjustReloadAnimatorSpeed(float reloadTime)
    {
        animator.speed = ((20+2)/reloadTime)*animatorSpeed;
    }
}
