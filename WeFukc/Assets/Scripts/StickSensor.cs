using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StickSensor : MonoBehaviour
{
    int m_CollisionCounter = 0;
    float m_DisableTimer;
    bool isDisabled = false;

    // Count collisions.
    private void OnTriggerEnter2D(Collider2D collision)
    {
        m_CollisionCounter++;
    }
    private void OnTriggerExit2D(Collider2D collision)
    {
        m_CollisionCounter--;
    }

    // If there is any collision still exist, return true
    public bool State()
    {
        if (m_DisableTimer > 0)
            return false;
        return m_CollisionCounter > 0;
    }

    void Update()
    {
        if (m_DisableTimer > 0)
        {
            m_DisableTimer -= Time.deltaTime;
        }
        else
            isDisabled = false;
    }
    /*
    private void Start()
    {
        m_CollisionCounter = 0;
    }
    */
    public void Disable(float duration)
    {
        m_DisableTimer = duration;
        isDisabled = true;
    }

    public bool isSensorDisabled() { return isDisabled; }
}
