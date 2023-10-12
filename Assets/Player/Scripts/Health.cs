using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Health : MonoBehaviour
{
    public delegate void OnChange(float change);
    [SerializeField]
    int MaxHealth;
    int CurrentHealth;

    public OnChange OnHealthChange;
    public OnChange OnHealthZero;
    public OnChange OnHealthLoss;
    public OnChange OnHealthGain;

    private void Start()
    {
        Reset();
    }

    public void Reset()
    {
        CurrentHealth = MaxHealth;
        OnHealthChange?.Invoke(CurrentHealth);
    }
    public void SetHealth(int Change)
    {
        CurrentHealth = (Change > MaxHealth) ? MaxHealth : (Change < 0) ? 0 : Change;
        OnHealthChange(CurrentHealth);
    }
    public void Apply(int Change) 
    {
        CurrentHealth = (CurrentHealth + Change > MaxHealth) ? MaxHealth : (CurrentHealth + Change < 0) ? 0 : CurrentHealth + Change;

        OnHealthChange?.Invoke(CurrentHealth);

        if(Change > 0) 
        {
            OnHealthGain?.Invoke(CurrentHealth);
        }
        else if (Change < 0) 
        {
            OnHealthLoss?.Invoke(CurrentHealth);
        }

        if (CurrentHealth == 0) 
        {
            OnHealthZero?.Invoke(0);
        }
    }
}
