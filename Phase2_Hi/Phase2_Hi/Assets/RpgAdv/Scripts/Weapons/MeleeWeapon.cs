using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = System.Random;

namespace RpgAdv
{
    public class MeleeWeapon : MonoBehaviour
    {
       

        [System.Serializable]
        public class AttackPoint
        {
            public float radius;
            public Vector3 offset;
            public Transform rootTransform;
        }
        public LayerMask targetLayers;
        public int damage = 10;
        public AttackPoint[] attackPoints = new AttackPoint[0];

        private bool m_IsAttack = false;
        private Vector3[] m_OriginalAttackPos;
        private RaycastHit[] m_RayCastHitCache = new RaycastHit[32];
        private GameObject m_Owner;

  
        private void FixedUpdate()
        {
            if (m_IsAttack)
            {
                for (int i = 0; i < attackPoints.Length; i++)
                {
                    AttackPoint ap = attackPoints[i];
                    Vector3 worldPos = ap.rootTransform.position + ap.rootTransform.TransformVector(ap.offset);
                    Vector3 attackVector = (worldPos - m_OriginalAttackPos[i]).normalized;

                    Ray ray = new Ray(worldPos, attackVector);
                    Debug.DrawRay(worldPos, attackVector.normalized, Color.red, 4.0f);

                    int contacts = Physics.SphereCastNonAlloc(
                        ray,
                        ap.radius,
                        m_RayCastHitCache,
                        attackVector.magnitude,
                        ~0,
                        QueryTriggerInteraction.Ignore);

                    for (int k = 0; k < contacts; k++)
                    {
                        Collider collider = m_RayCastHitCache[k].collider;

                        if (collider != null)
                        {
                            CheckDamage(collider, ap);
                        }
                    }

                    m_OriginalAttackPos[0] = worldPos;
                }
            }
        }

        private void CheckDamage(Collider other, AttackPoint ap)
        {
            

            if ((targetLayers.value & (1 << other.gameObject.layer)) == 0)
            {
                return;
            }

            Damageable damageable = other.GetComponent<Damageable>();


            if (damageable != null)
            {
                Damageable.DamageMessage data;
                data.amount = damage;
                data.damager = this;
                data.damageSource = m_Owner.transform.position;
               
           
                damageable.ApplyDamage(data);
            }
        }

        public void SetOwner(GameObject owner)
        {
            m_Owner = owner;
        }
        public void BeginAttack()
        {
            m_IsAttack = true;
            m_OriginalAttackPos = new Vector3[attackPoints.Length];

            for (int i = 0; i < attackPoints.Length; i++)
            {
                AttackPoint ap = attackPoints[i];
                m_OriginalAttackPos[i] = 
                    ap.rootTransform.position + ap.rootTransform.TransformDirection(ap.offset);
            }

        }

        public void EndAttack()
        {
            m_IsAttack =false;
        }
        
#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {

            foreach (AttackPoint attackPoint in attackPoints)
            {
                if (attackPoint.rootTransform != null)
                {
                    Vector3 worldPosition = attackPoint.rootTransform.TransformVector(attackPoint.offset);
                    
                    Gizmos.color = new Color(1.0f, 1.0f, 1.0f, 0.6f);
                    Gizmos.DrawSphere(attackPoint.rootTransform.position + worldPosition, attackPoint.radius);
                }
            }
        }
        #endif
    }
}

