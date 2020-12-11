using UnityEngine;
using Ubik.Messaging;
using Random = System.Random;

namespace Transballer.NetworkedPhysics
{
    public class Ball : NetworkedObject
    {
        [SerializeField] public Renderer ball;
        private Color[] colours = new Color[] { Color.green, Color.blue, Color.cyan, Color.green, Color.magenta, Color.red, Color.yellow };

        void Start()
        {
            if (NetworkManager.roomOwner)
            {
                Random rand = new Random();
                ball.material.color = colours[rand.Next(colours.Length)];

                ctx.Send(new ColourMessage(ball.material.color).Serialize());

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
                    ball.material.color = colour.colour;
                    break;
                case "askColour":
                    if (NetworkManager.roomOwner)
                    {
                        ctx.Send(new ColourMessage(ball.material.color).Serialize());
                    }
                    break;
                default:
                    base.ProcessMessage(message);
                    break;
            }

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