using System.Collections;
using UnityEngine;

public class Player : MonoBehaviour
{
    public Bullet bulletPrefab;

    public float thrustSpeed = 1.0f;
    public float turnSpeed = 1.0f;
    private float fireRate = 0.1f;
    private float nextFireTime = 0f;
    private bool rapidFireMode = false;
    private bool hiddenAutoFire = false;
    private bool immortal = false;

    private Rigidbody2D _rigibody;
    private float thrustDirection;
    private float _turnDirection;

    // Buff 1: Auto Fire
    private bool autoFireActive = false;
    private float autoFireCooldown = 60f;
    private float nextAutoFireTime = 0f;

    // Buff 2: Immortal
    private bool immortalBuff = false;
    private float immortalCooldown = 60f;
    private float nextImmortalTime = 0f;

    private void Awake()
    {
        _rigibody = GetComponent<Rigidbody2D>();
    }

    private void Update()
    {
        if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow))
            thrustDirection = 1.0f;
        else if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow))
            thrustDirection = -1.0f;
        else
            thrustDirection = 0.0f;

        if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))
        {
            _turnDirection = 1.0f;
        }
        else if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow))
        {
            _turnDirection = -1.0f;
        }
        else
        {
            _turnDirection = 0.0f;
        }

        if (Input.GetKeyDown(KeyCode.M))
        {
            rapidFireMode = !rapidFireMode;
        }

        if (Input.GetKey(KeyCode.LeftShift) && Input.GetKeyDown(KeyCode.B))
        {
            hiddenAutoFire = !hiddenAutoFire;
        }

        if (autoFireActive || hiddenAutoFire)
        {
            // auto fire không cần nhấn Space
            if (Time.time >= nextFireTime)
            {
                Shoot();
                nextFireTime = Time.time + fireRate;
            }
        }
        else if (rapidFireMode)
        {
            // rapid fire phải nhấn Space
            if ((Input.GetKey(KeyCode.Space)) && Time.time >= nextFireTime)
            {
                Shoot();
                nextFireTime = Time.time + fireRate;
            }
        }
        else
        {
            // bắn 1 phát khi nhấn Space hoặc click chuột
            if (Input.GetKeyDown(KeyCode.Space))
            {
                Shoot();
            }
        }

        if (Input.GetKey(KeyCode.LeftControl) && Input.GetKey(KeyCode.LeftShift) && Input.GetKeyDown(KeyCode.I))
        {
            immortal = !immortal;
        }


        // Buff 1: auto bắn
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            if (Time.time >= nextAutoFireTime)
            {
                StartCoroutine(ActivateAutoFire(10f));
                nextAutoFireTime = Time.time + autoFireCooldown;
            }
            else
            {
                Debug.Log("Auto fire on cooldown");
            }
        }

        // Buff 2: Immortal
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            if (Time.time >= nextImmortalTime)
            {
                StartCoroutine(ActivateImmortal(10f));
                nextImmortalTime = Time.time + immortalCooldown;
            }
            else
            {
                Debug.Log("Immortal on cooldown");
            }
        }
    }

    private void FixedUpdate()
    {
        if(thrustDirection != 0f)
        {
            _rigibody.AddForce(transform.up * thrustSpeed * thrustDirection);
        }
        
        if(_turnDirection != 0.0f)
        {
            _rigibody.AddTorque(_turnDirection * turnSpeed);
        }
    }

    private void Shoot()
    {
        Bullet bullet = Instantiate(bulletPrefab, transform.position, transform.rotation);
        bullet.Project(transform.up);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Asteroid"))
        {
            if (immortal || immortalBuff)
            {
                // chỉ reset velocity nhưng không chết
                _rigibody.linearVelocity = Vector3.zero;
                _rigibody.angularVelocity = 0.0f;
            }
            else
            {
                _rigibody.linearVelocity = Vector3.zero;
                _rigibody.angularVelocity = 0.0f;

                gameObject.SetActive(false);

                FindAnyObjectByType<GameManager>().PlayerDied();
            }
        }
    }


    private IEnumerator ActivateAutoFire(float duration)
    {
        autoFireActive = true;
        Debug.Log("Auto fire activated!");

        yield return new WaitForSeconds(duration);

        autoFireActive = false;
        Debug.Log("Auto fire expired!");
    }

    private IEnumerator ActivateImmortal(float duration)
    {
        immortalBuff = true;
        Debug.Log("Immortal activated!");

        yield return new WaitForSeconds(duration);

        immortalBuff = false;
        Debug.Log("Immortal expired!");
    }
}
