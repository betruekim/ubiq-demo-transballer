using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Transballer.Levels
{
    public class HoopCollider : MonoBehaviour
    {
        public GameObject hoop;
        public int colliderNumber;


        private Hoop hoopRef;
        private List<string> gate1;
        private LevelManager levelManager;

        void Start()
        {
            hoop.transform.Find("BallDisplay").GetComponent<TextMesh>().text = string.Format("{0}", hoop.GetComponent<Hoop>().ballsToGo);
            hoopRef = hoop.GetComponent<Hoop>();
            gate1 = hoopRef.gate1;
        }

        void OnTriggerEnter(Collider collision)
        {
            // Check if object name is ball 
            if (collision.gameObject.layer == LayerMask.NameToLayer("Ball"))
            {
                Hoop hoopRef = hoop.GetComponent<Hoop>();
                if (colliderNumber == 1)
                {
                    gate1.Add(collision.gameObject.name);
                }
                else if (colliderNumber == 2)
                {
                    if (gate1.Contains(collision.gameObject.name))
                    {
                        // Decrement balls left 
                        hoopRef.ballsToGo -= 1;
                        hoop.transform.Find("BallDisplay").GetComponent<TextMesh>().text = string.Format("{0}", hoopRef.ballsToGo);
                        collision.gameObject.SetActive(false);

                        // Play sound
                        gameObject.GetComponent<AudioSource>().Play();

                        if (hoopRef.ballsToGo == 0)
                        {
                            // GameObject.FindObjectOfType<LevelManager>().levelComplete();
                            hoopRef.Complete();
                        }
                    }
                }
            }
        }
    }
}
