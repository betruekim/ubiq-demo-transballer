using UnityEngine;

namespace Transballer.PlaceableObjects
{
    public class Splitter : Placeable
    {
        public override int materialCost => 10;
        public BoxCollider splitCollider;
        public Transform[] splits;

        int splitIndex = 0;

        private void FixedUpdate()
        {
            if (!placed || !owner)
            {
                return;
            }

            LayerMask ballMask = (1 << LayerMask.NameToLayer("Ball"));
            Collider[] balls = Physics.OverlapBox(splitCollider.transform.position, splitCollider.size / 2, splitCollider.transform.rotation, ballMask);
            foreach (Collider col in balls)
            {
                Transballer.NetworkedPhysics.NetworkedRigidbody ball = col.gameObject.GetComponent<Transballer.NetworkedPhysics.NetworkedRigidbody>();
                if (ball.graspedRemotely || ball.graspingController)
                {
                    continue;
                }

                if (!ball.owner)
                {
                    ball.TakeControl();
                    continue;
                }

                ball.transform.position = splits[splitIndex].transform.position;
                ball.rb.velocity = splits[splitIndex].transform.forward * (2 + ball.rb.velocity.magnitude);
                splitIndex++;
                splitIndex %= splits.Length;
                HintManager.SetComplete(HintManager.splitter, true);
            }
        }
    }
}