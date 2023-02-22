using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Outsider;
using System;

public class TriggerProjectile : BaseProjectile, IProjectile
{
    public float delayTime;
    public int praticleIndex;

    private bool isTriggered = false;
    private List<UnitBaseModule> enemyInRadiusList = new List<UnitBaseModule>();
    private Queue<UnitBaseModule> enemyQueue = new Queue<UnitBaseModule>();
    
    private void Update()
    {
        transform.position += (speed * aimDirection * Time.deltaTime);
        enemyInRadiusList.RemoveAll(delegate (UnitBaseModule o) { return o == null; });
    }

    #region COLLISION
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Unit"))
        {
            UnitBaseModule detectedUnit = collision.gameObject.GetComponent<UnitBaseModule>();
            if (teamIndex == TeamIndex.Ally && detectedUnit.teamIndex == TeamIndex.Enemy || teamIndex == TeamIndex.Enemy && detectedUnit.teamIndex == TeamIndex.Ally)
            {
                if(!isTriggered)
                    Invoke("TriggerAction", delayTime);

                int check = enemyInRadiusList.FindIndex(x => x == detectedUnit);
                if (check == -1)
                {
                    enemyInRadiusList.Add(detectedUnit);
                }

            }
        }
    }
    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Unit"))
        {
            UnitBaseModule detectedUnit = collision.gameObject.GetComponent<UnitBaseModule>();
            if (teamIndex == TeamIndex.Ally && detectedUnit.teamIndex == TeamIndex.Enemy || teamIndex == TeamIndex.Enemy && detectedUnit.teamIndex == TeamIndex.Ally)
            {
                int check = enemyInRadiusList.FindIndex(x => x == detectedUnit);
                if (check != -1)
                {
                    enemyInRadiusList.RemoveAt(check);
                }
            }

        }
    }
    #endregion

    #region TRIGGER
    public override void ProjectileExpire()
    {
        TriggerAction();
        base.ProjectileExpire();
    }
    public virtual void TriggerAction()
    {
        if (!isTriggered)
        { 
            isTriggered = true;
            Audio_Manager.Instance.PlayAt(14, transform.position, Audio_Manager.Instance.SoundVolume);

            foreach (UnitBaseModule Unit in enemyInRadiusList)
            {
                enemyQueue.Enqueue(Unit);
            }
            int count = enemyQueue.Count;
            for (int i = 0; i < count; i++)
            {
                UnitBaseModule newTarget = enemyQueue.Dequeue();
                newTarget.ReceiveDamage(CalculateDamage(), penetrationRate);
            }

            Particle_Manager.Instance.CreateParticle(this, new Particle_Manager.ParticleArgs { index = praticleIndex, position = transform.position, parent = null });
            ProjectileExpire();
        }
    }
    #endregion
}
