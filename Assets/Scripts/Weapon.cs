using UnityEngine;
using UnityEngine.Assertions;

public class Weapon : MonoBehaviour
{
    public int dmgValue = 100; //Damage of the weapon
    public Color dmgColor = Color.green; //Color of the text with the damage value

    private BoxCollider coll; //Collider of the weapon

    void Awake()
    {
        coll = GetComponent<BoxCollider>();
        Assert.IsNotNull(coll, "Collision is null");
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Enemy")
        {
            DmgInfo dmgInfo = new(dmgValue, dmgColor, transform.position);
            other.SendMessage("ApplyDmg", dmgInfo);
        }
    }
}
