using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Outsider;

namespace Projectile
{
    public class SeekingProjectile : BaseProjectile, IProjectile
    {
        public Transform targetLockon;
        [SerializeField] private float turnSpeed = 20;
        private Vector3 seekingVelocity;

        public override void InitiateProjectile(Projectile.ProjectileObject Data, TeamIndex teamIndex, Vector3 origin, Vector3 direction, bool isCrit)
        {
            base.InitiateProjectile(Data, teamIndex, origin, direction, isCrit);

            targetLockon = null;
            seekingVelocity = Vector3.zero;

            SeekHoming Guide = transform.GetChild(0).GetComponent<SeekHoming>();
            Guide.Initiate(this);
        }
        void Update()
        {
            if (targetLockon != null)
            {
                aimDirection = (targetLockon.transform.position - transform.position).normalized;
                Quaternion Target = Quaternion.LookRotation(Vector3.forward, aimDirection);

                Vector3 DIR_VELOCITY = aimDirection * (speed * 3);
                Vector3 STEER = DIR_VELOCITY - seekingVelocity;
                seekingVelocity += STEER / 10;

                transform.rotation = Quaternion.RotateTowards(transform.rotation, Target, turnSpeed);

                transform.position += seekingVelocity * Time.deltaTime;
            }
            else
            {
                transform.position += (speed * aimDirection * Time.deltaTime);
            }
        }
        public void ReceiveTarget(Transform target)
        {
            targetLockon = target;
        }
        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (collision.CompareTag("Unit"))
            {
                UnitBaseModule colliedUnit = collision.gameObject.GetComponent<UnitBaseModule>();
                if (teamIndex == TeamIndex.Ally && colliedUnit.teamIndex == TeamIndex.Enemy)
                {
                    UnitBaseModule unitHit = collision.gameObject.GetComponent<UnitBaseModule>();
                    unitHit.ReceiveDamage(CalculateDamage(),penetrationRate);
                    ProjectileExpire();
                }
            }
        }

        #region ENABLE/DISABLE
        public override void OnDisable()
        {
            base.OnDisable();
            targetLockon = null;
            seekingVelocity = Vector3.zero;
        }
        #endregion
    }
}