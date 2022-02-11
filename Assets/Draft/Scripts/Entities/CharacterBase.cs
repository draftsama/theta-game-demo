using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UniRx;

public class CharacterBase : MonoBehaviour
{
    [SerializeField] private float m_HealthPower;
    [SerializeField] private bool m_Immortal;
    [SerializeField] private CharacterType m_CharacterType;
    [SerializeField] private UnityEvent<CharacterBase> m_OnTerminated;
    public Transform m_Transform { get; private set; }
     
    public float Width { private set; get; }
    public float Height { private set; get; }
    
    public enum CharacterType
    {
       None,Player,Enemy 
    }
    
    protected virtual void Awake()
    {
        m_Transform = transform;
        var poly = GetComponent<PolygonCollider2D>();
        var pointXs = poly.points.Select(_ => _.x).ToArray();
        var pointYs = poly.points.Select(_ => _.y).ToArray();
        
        Width = pointXs.Max() - pointXs.Min();
        Height = pointYs.Max() - pointYs.Min();
    }
    protected virtual void Start()
    {
    }

    protected virtual  void Terminate()
    {
        
    }
    public void ReductHealthPower(float _damage)
    {
        m_HealthPower -= _damage;
        if(m_HealthPower <= 0)
        {
            Terminate();
            m_OnTerminated?.Invoke(this);
        }
    }

    public void SetHealthPower(float _hp)
    {
        m_HealthPower = _hp;
    }
    public float GetHealthPower() => m_HealthPower;

    public bool IsAlive()
    {
        return m_HealthPower > 0;
    }
  
    public bool IsOnScreen()
    {
        var areaHeight = 2f * Camera.main.orthographicSize;
        var areaWidth = areaHeight * Camera.main.aspect;     
        var topPos = (areaHeight / 2f) + Height/2f;
        var bottomPos = -(areaHeight / 2f) - Height/2f;
        var leftPos = -(areaWidth / 2f) - Width/2f;
        var rightPos = (areaWidth / 2f) + Width/2f;

        var position = m_Transform.position;
        return position.x > leftPos && position.x < rightPos && position.y >bottomPos && position.y < topPos;
    }
    private void OnTriggerEnter2D(Collider2D col)
    {
        var ammo = col.gameObject.GetComponent<AmmoBase>();
        if (ammo != null)
        {      

            if (ammo.GetShooterType() == CharacterType.Player && m_CharacterType == CharacterType.Enemy || 
                ammo.GetShooterType() == CharacterType.Enemy && m_CharacterType == CharacterType.Player)
            {
                ammo.Terminate();
                if(!m_Immortal)
                   ReductHealthPower(ammo.m_Damage);
            }
            

        }
    }

    public IObservable<CharacterBase> OnTerminatedAsObservable() => m_OnTerminated.AsObservable<CharacterBase>();
}