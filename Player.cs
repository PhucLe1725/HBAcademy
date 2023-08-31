using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using UnityEngine;

public class Player : Character
{
    [SerializeField] private Rigidbody rb;
    [SerializeField] private DynamicJoystick joystick;
    [SerializeField] private float _moveSpeed;
    [SerializeField] private WeaponData weaponData;
    [SerializeField] private BulletData bulletData;
    [SerializeField] private HatData hatData;
    [SerializeField] private PantsData pantsData;
    [SerializeField] private Transform weaponHolder;
    [SerializeField] private Transform hatHolder;
    [SerializeField] private Transform attackPoint;

    private bool isAttack = false;
    [SerializeField] private Weapon curWeapon;
    [SerializeField] private Hat curHat;
    [SerializeField] private Bullet curBullet;
    [SerializeField] private Renderer curMeshPants;

    public PantsType pantsColor;

    private bool isAttackCooldown = false;
    private float attackCooldownDuration = 1.0f; // Thời gian giữa các lần tấn công
    private bool isRotating = false; // Biến kiểm tra xem đang xoay hay không

    private Coroutine currentCoroutine;



    private void FixedUpdate()
    {
        rb.velocity = new Vector3(joystick.Horizontal * _moveSpeed, rb.velocity.y, joystick.Vertical * _moveSpeed);
        if (joystick.Horizontal != 0 || joystick.Vertical != 0)
        {
            transform.rotation = Quaternion.LookRotation(rb.velocity);
            ChangeAnim("run");
            if (currentCoroutine != null)
            {
                StopCoroutine(currentCoroutine);
            }
            isAttackCooldown = false;
            isAttack = false;
            isRotating = false; // Đặt lại biến xoay khi di chuyển
        }
        else
        {
         
            if (currentAnim != "attack")
            {
                ChangeAnim("idle");
            }

            if (!isAttack && targets.Count > 0)
            {
                Character nearestTarget = FindNearestTarget();
                if (nearestTarget != null)
                {
                    Vector3 targetDirection = nearestTarget.transform.position - transform.position;
                    Quaternion targetRotation = Quaternion.LookRotation(targetDirection);

                    if (!isRotating)
                    {
                        currentCoroutine = StartCoroutine(RotateAndAttack(targetRotation, nearestTarget));
                    }
                }
            }
        }
    }
    private IEnumerator RotateAndAttack(Quaternion targetRotation, Character target)
    {
        isRotating = true; // Bắt đầu quá trình xoay

        float rotationSpeed = 10.0f; // Tốc độ xoay, có thể điều chỉnh
        float minAngle = 0.01f; // Góc nhỏ nhất để dừng xoay

        while (Quaternion.Angle(transform.rotation, targetRotation) > minAngle)
        {
            transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
            yield return null;
        }

        // Xoay xong, thực hiện tấn công
        if (!isAttack && !isAttackCooldown)
        {
            isAttack = true; // Bắt đầu tấn công
            ChangeAnim("attack");
            Attack();

            // Bắt đầu tính thời gian cooldown
            StartCoroutine(AttackCooldown());
        }

        isAttack = false; // Hoàn thành tấn công
        isRotating = false;
    }
    private IEnumerator AttackCooldown()
    {
        isAttackCooldown = true;
        yield return new WaitForSeconds(attackCooldownDuration);
        isAttackCooldown = false;
        ChangeAnim("idle");
    }
    public override void OnInit()
    {
        base.OnInit();
        ChangeAnim("idle");
        ChangeWeapon(WeaponType.Knife);
        ChangeHat(HatType.Horn);
        ChangePants(PantsType.Batman);
    }
    private void ChangeWeapon(WeaponType type)
    {
        if (curWeapon != null)
        {
            Destroy(curWeapon.gameObject);
        }
        curWeapon = Instantiate(weaponData.GetWeapon(type), weaponHolder);
    }
    private void ChangeHat(HatType type)
    {
        if (curHat != null)
        {
            Destroy(curHat.gameObject);
        }
        curHat = Instantiate(hatData.GetHat(type), hatHolder);
    }
    private void ChangePants(PantsType type)
    {
        pantsColor = type;
        curMeshPants.material = pantsData.GetPants(pantsColor);
    }
    private void Attack()
    {
        ChangeAnim("attack");
        isAttack = true;
        Vector3 playerLookDirection = transform.forward;
        Fire(playerLookDirection, attackPoint);
    }

    public void Fire(Vector3 playerLookDirection, Transform attackPoint)
    {
        curWeapon.Fire(playerLookDirection, attackPoint);
    }

}

