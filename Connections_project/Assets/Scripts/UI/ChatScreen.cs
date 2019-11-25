﻿using System.IO;
using UnityEngine.UI;
using UnityEngine;

public class TextPacket : NetworkPacket<string>
{
	public TextPacket() : base(PacketType.User) { }

	protected override void OnSerialize (Stream stream)
	{
		BinaryWriter binaryWriter = new BinaryWriter(stream);
		binaryWriter.Write(payload);
	}

	protected override void OnDeserialize (Stream stream)
	{
		BinaryReader binaryReader = new BinaryReader(stream);
		payload = binaryReader.ReadString();
	}
}


public class ChatScreen : MonoBehaviourSingleton<ChatScreen>
{
    public Text messages;
    public InputField inputMessage;
	public uint objectID = 1;

    protected override void Initialize()
    {
        inputMessage.onEndEdit.AddListener(OnEndEdit);        

        this.gameObject.SetActive(false);

		PacketManager.Instance.AddListener(objectID, OnReceiveDataEvent);
    }

    void OnReceiveDataEvent(uint packetID, ushort packetTypeID, Stream stream)
    {
		Debug.Log("On recieve data Event");

		switch (packetTypeID)
		{
			case (ushort)UserPacketType.Text_Message:
				TextPacket textPacket = new TextPacket();
				textPacket.Deserialize(stream);
				messages.text += textPacket.payload + System.Environment.NewLine;

				if (NetworkManager.Instance.isServer)
					PacketManager.Instance.SendPacket(textPacket, objectID);

				break;
		}
    }

    void OnEndEdit(string str)
    {
        if (inputMessage.text != "")
        {
			TextPacket textPacket = new TextPacket();
			textPacket.payload = inputMessage.text;

			PacketManager.Instance.SendPacket(textPacket, objectID);

          	if (NetworkManager.Instance.isServer)
            	messages.text += inputMessage.text + System.Environment.NewLine;

            inputMessage.ActivateInputField();
            inputMessage.Select();        
            inputMessage.text = "";
        }
    }
}
