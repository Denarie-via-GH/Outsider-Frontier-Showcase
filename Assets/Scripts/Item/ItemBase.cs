using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemBase : MonoBehaviour
{
    public ItemPointer pointer;

    private void Start()
    {
        pointer = UI_Manager.Instance.SpawnItemPointer(this.transform);
        pointer.Initiate(transform.position);
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Unit"))
        {
            ActiveItem();
        }
    }
    protected virtual void ActiveItem()
    {
        Audio_Manager.Instance.PlayGlobal(16);
        Particle_Manager.Instance.CreateParticle(11, transform.position);
    }

    private void OnDisable()
    {
        Destroy(pointer.gameObject);
    }
}
