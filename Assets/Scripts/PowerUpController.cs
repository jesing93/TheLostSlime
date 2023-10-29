using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PowerUpController : MonoBehaviour
{
    //Variables
    private Animator itemAnimator;
    [SerializeField]
    private SlimeType type = SlimeType.basic;

    public SlimeType Type { get => type; set => type = value; }

    private void Awake()
    {
        itemAnimator = GetComponentInChildren<Animator>();
        updateType(type);
    }

    /// <summary>
    /// Switch my type to the new type
    /// </summary>
    /// <param name="slimeType">The new type</param>
    public void updateType(SlimeType slimeType = SlimeType.basic)
    {
        //Pick new color id
        int typeId = 0;
        switch (slimeType)
        {
            case SlimeType.teleport: 
                typeId = 1; 
                break;
        }
        //Update animator
        itemAnimator.SetInteger("Color", typeId);
        itemAnimator.SetTrigger("ChangeColor");
        type = slimeType;
    }
}
