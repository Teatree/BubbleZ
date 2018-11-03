using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Box : MonoBehaviour {

	public enum Type {
		None = 0,

        // normal bubbles
        Color1,
		Color2,
		Color3,
		Color4,
		Color5,
		Color6,
		Color7,
		Color8

	};
	
	public Type type {
		get;
		set;
	}

    private static int maxNormalColorType = 8;
    public static int maxAmountOfHits = 12;
    public int amountOfHits = 0;
    private int amountOfHitsTaken = 0;
    public Collider2D coll;

    public static Type GetRandomColor()
    {
        int rand = Random.Range(1, maxNormalColorType);
        return (Type)rand;
    }

    public static int GetRandomHitsAmount() {
       return Random.Range(1, maxAmountOfHits);
    }

    private static Type _lastOne = Type.None;
    private static Type _lastTwo = Type.None;

    public static Box.Type GetColorByHits(int hits) {
        switch (hits) {
            case 1:
                return Box.Type.Color1;
            case 2:
                return Box.Type.Color2;
            case 3:
                return Box.Type.Color3;     
            case 4:
                return Box.Type.Color4;
            case 5:
                return Box.Type.Color5;
            case 6:
                return Box.Type.Color6;
            case 7:
                return Box.Type.Color7;
            case 8:
                return Box.Type.Color8;
        }

        return Box.Type.Color1;
    }

    void OnCollisionEnter2D(Collision2D collision) {
        //Debug.Log(">>> amountOfHitsTaken > " + this.amountOfHits);
        //Debug.Log(">>> amountOfHitsTaken > " + this.amountOfHitsTaken);
        this.amountOfHitsTaken++;
        if (this.amountOfHitsTaken >= this.amountOfHits) {
            Destroy(gameObject);
        } else {
            int hitsLeft = this.amountOfHits - this.amountOfHitsTaken;
            gameObject.transform.GetChild(0).transform.GetChild(0).gameObject.GetComponent<UnityEngine.UI.Text>().text = "" + hitsLeft;
        }
    }
}
