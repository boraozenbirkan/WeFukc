using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FloatingDamage : MonoBehaviour
{
    Vector2 startPos;

    [SerializeField] private float speed0;
    [SerializeField] private float speed1;
    [SerializeField] private float speed2;
    [SerializeField] private float speed3;

    [SerializeField] private float height0;
    [SerializeField] private float height1;
    [SerializeField] private float height2;

    [SerializeField] public TMPro.TextMeshProUGUI damageText;

    // Start is called before the first frame update
    void Start()
    {
        startPos.x = transform.position.x;
        startPos.y = transform.position.y;
    }

    // Update is called once per frame
    void Update()
    {
        // Raise fast but little
        if (transform.position.y < startPos.y + height0)
        {
            transform.position = new Vector2(startPos.x, transform.position.y + (Time.deltaTime * speed0));
        }

        // Raise a bit slowly
        else if (transform.position.y < startPos.y + height1)
        {
            transform.position = new Vector2(startPos.x, transform.position.y + (Time.deltaTime * speed1));
        }

        // Raise way up
        else if (transform.position.y < startPos.y + height2)
        {
            transform.position = new Vector2(startPos.x, transform.position.y + (Time.deltaTime * speed2));
            if (transform.localScale.x > 0f)
            {
                transform.localScale =
                new Vector2(transform.localScale.x - (Time.deltaTime * speed3), transform.localScale.y - (Time.deltaTime * speed3));
            }
        }
        else // if gone enough, just die...
        {
            Destroy(gameObject);
            //transform.position = startPos;
            //transform.localScale = new Vector2(1f, 1f);
        }
    }
}
