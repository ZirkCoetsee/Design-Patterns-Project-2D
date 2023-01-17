using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

public class Bullet : MonoBehaviour
{
    //Externalize this to create custom bullet interactions
    public float fireForce = 10f;
    private Rigidbody2D rb;
    //private TrailRenderer trail;
    private IObjectPool<Bullet> bulletPool;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        //trail = GetComponent<TrailRenderer>();
    }

    private void OnEnable()
    {
        //trail.enabled = true;
        //trail.emitting = true;

    }

    private void Start()
    {

    }

    private void OnDisable()
    {
        //trail.enabled = false;
        //trail.emitting = false;

    }

    public void SetPool(IObjectPool<Bullet> pool)
    {
        bulletPool = pool;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        bulletPool.Release(this);
    }

    private void FixedUpdate()
    {
        rb.AddForce(transform.up * fireForce, ForceMode2D.Impulse);
    }

    private void OnBecameInvisible()
    {
        bulletPool.Release(this);
    }
}
