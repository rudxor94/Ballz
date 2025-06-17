using UnityEngine;

public class AddBall : MonoBehaviour
{
    public float moveSpeed = 10f;

    // Update is called once per frame
    private void Update()
    {
        if (transform.position.y > -4.3f)
        {
            transform.Translate(new Vector3(0, -Time.deltaTime * moveSpeed, 0));
            if (transform.position.y <= -4.3f)
            {
                var position = transform.position;
                transform.position = new Vector3(position.x, -4.3f, position.z);
            }
        }
    }
}