using UnityEngine;
using Ubik.Messaging;
using Random = System.Random;

namespace Transballer.NetworkedPhysics
{
    public class Ball : NetworkedRigidbody
    {
        [SerializeField] public Renderer ballRenderer;
        private Color[] colours = new Color[] { Color.green, Color.blue, Color.cyan, Color.green, Color.magenta, Color.red, Color.yellow };

        void Start()
        {
            ballRenderer = GetComponent<Renderer>();
            if (NetworkManager.roomOwner)
            {
                Random rand = new Random();
                OnSetColor(colours[rand.Next(colours.Length)]);

                ctx.Send(new ColourMessage(ballRenderer.material.color).Serialize());

                Debug.Log("ColourMessage sent.");
            }
            else
            {
                ctx.Send("askColour$");
            }

        }

        public override void ProcessMessage(ReferenceCountedSceneGraphMessage message)
        {
            string msgString = message.ToString();
            string messageType = Transballer.Messages.GetType(msgString);
            Debug.Log(msgString);

            switch (messageType)
            {
                case "colourMessage":
                    Debug.Log("Colour message received: " + msgString);
                    ColourMessage colour = ColourMessage.Deserialize(msgString);
                    OnSetColor(colour.colour);
                    break;
                case "askColour":
                    if (NetworkManager.roomOwner)
                    {
                        ctx.Send(new ColourMessage(ballRenderer.material.color).Serialize());
                    }
                    break;
                default:
                    base.ProcessMessage(message);
                    break;
            }

        }

        void OnSetColor(Color color)
        {
            ballRenderer.material.color = color;
            ballRenderer.material.SetColor("_EmissionColor", color);
            // ballRenderer.GetComponentInChildren<Light>().color = color;
        }
    }


    [System.Serializable]
    public class ColourMessage : Transballer.Messages.Message
    {
        public override string messageType => "colourMessage";

        public Color colour;

        public ColourMessage(Color colour)
        {
            this.colour = colour;
        }

        public override string Serialize()
        {
            return "colourMessage$" + colour[0].ToString() + "$" + colour[1].ToString() + "$" + colour[2].ToString() + "$" + colour[3].ToString() + "$";
        }

        public static ColourMessage Deserialize(string message)
        {
            string[] components = message.Split('$');

            return new ColourMessage(new Color(float.Parse(components[1]), float.Parse(components[2]), float.Parse(components[3]), float.Parse(components[4])));
        }

    }
}