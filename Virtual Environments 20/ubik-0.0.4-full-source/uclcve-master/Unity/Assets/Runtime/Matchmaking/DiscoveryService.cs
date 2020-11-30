using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Collections.Concurrent;
using UnityEngine;
using Ubik.Messaging;
using Ubik.Networking;
using System.Linq;

namespace Ubik.Matchmaking
{
    /// <summary>
    /// Facilitates automatic discovery and negotiation on a local network.
    /// </summary>
    [RequireComponent(typeof(ConnectionManager))]
    public class DiscoveryService : MonoBehaviour
    {
        public int port = 32976;
        public string guid;

        public enum State
        {
            Waiting,
            Searching
        }

        [NonSerialized]
        public State state;

        [NonSerialized]
        public Dictionary<string, Response> Responses;

        private UdpClient udpClient;
        private IPEndPoint endpoint;

        private ConcurrentQueue<Message> announcements;

        private Response mystate;

        private ConnectionManager manager;

        private class UdpClientState
        {
            public UdpClient client;
            public IPEndPoint endpoint;
        }

        [Serializable]
        public class Message
        {
            public string type;
            public string payload;
        }

        [Serializable]
        public class Response
        {
            public string guid;
            public List<ConnectionDefinition> uris;
        }

        private void Awake()
        {
            announcements = new ConcurrentQueue<Message>();
            Responses = new Dictionary<string, Response>();

            manager = GetComponent<ConnectionManager>();

            state = State.Waiting;
            guid = Guid.NewGuid().ToString();
            mystate = new Response();

            endpoint = new IPEndPoint(IPAddress.Any, port);
            udpClient = new UdpClient();
            udpClient.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            udpClient.ExclusiveAddressUse = false;
            udpClient.Client.Bind(endpoint);
            udpClient.BeginReceive(
                OnReceive,
                new UdpClientState()
                {
                    client = udpClient,
                    endpoint = endpoint
                }
            );
        }

        private void OnReceive(IAsyncResult ar)
        {
            var state = ar.AsyncState as UdpClientState;
            var bytes = state.client.EndReceive(ar, ref state.endpoint);
            try
            {
                var message = JsonUtility.FromJson<Message>(Encoding.UTF8.GetString(bytes));
                announcements.Enqueue(message);
            }
            catch(Exception e)
            {
                Debug.LogException(e);
            }

            state.client.BeginReceive(OnReceive, state);
        }

        private void Broadcast(Message message)
        {
            var bytes = Encoding.UTF8.GetBytes(JsonUtility.ToJson(message));
            var endpoint = new IPEndPoint(IPAddress.Broadcast, port);
            udpClient.Send(bytes, bytes.Length, endpoint);
        }

        private void SendResponse()
        {
            //update my state
            mystate.guid = guid;
            mystate.uris = manager.PublicUris.ToList();

            var message = new Message();
            message.type = "response";
            message.payload = JsonUtility.ToJson(mystate);
            Broadcast(message);
        }

        public void StartSearch()
        {
            state = State.Searching;
            StartCoroutine(SearchCoroutine());
        }

        private IEnumerator SearchCoroutine()
        {
            while (state == State.Searching)
            {
                var message = new Message();
                message.type = "request";
                Broadcast(message);

                yield return new WaitForSeconds(0.25f);
            }
        }

        public void StopSearch()
        {
            state = State.Waiting;
        }

        // Update is called once per frame
        void Update()
        {
            Message m;
            while(announcements.TryDequeue(out m))
            {
                switch (m.type)
                {
                    case "request":
                        SendResponse();
                        break;
                    case "response":
                        {
                            var response = JsonUtility.FromJson<Response>(m.payload);
                            if (response.guid != mystate.guid)
                            {
                                Responses[response.guid] = response;
                            }
                        }
                        break;
                }
            }
        }

        public void Connect(string guid)
        {
            try
            {
                manager.Connect(Responses[guid].uris.First());
            }
            catch(Exception e)
            {
                Debug.LogException(e);
            }
        }
    }
}
