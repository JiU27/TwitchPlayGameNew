using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using System.Net.Sockets;
using System.IO;

public class Twitch_Manager : MonoBehaviour
{
    /// <summary>
    /// BASIC TERMS
    //TCP stands for transmission Control Protocol
    //It enables application programs and other computing devices to exchange messages over a network.
    //It's designed to send packets across the internet and allows for data and messages to be delivered over networks.
    //TCP is one of the most commonly used protocols used in network communications...
    //You can learn more about TCP here: https://www.fortinet.com/resources/cyberglossary/tcp-ip

    //Tutorial I used for this script:
    //https://www.youtube.com/watch?v=QG3VIUFcif0

    //BASIC SETUP/THINGS NOT EXPLAINED
    //YOU MUST INSTALL THE LATEST VERSION OF OBS
    //YOU MUST BE STREAMING TO HAVE CHAT OPEN TO TEST THINGS
    /// </summary>

    public UnityEvent<string, string> OnChatMessage;

    TcpClient Twitch;
    StreamReader Reader;
    StreamWriter Writer;

    const string URL = "irc.chat.twitch.tv";
    const int PORT = 6667;

    //put your twitch username here - make a new account for security reasons - i don't understand why but it's recommended
    string User = "mc_with_no_donald";

    //copy and paste OAuth from     twitchapps.com/tmi
    string OAuth = "oauth:xmjfdqtx7zw5dzgh3p605vfrh3wqa3";  //your OAuth is basically as good as a password, so you should make a new account before doing this

    //this is the channel you want to connect to
    string Channel = "mc_with_no_donald";

    float pingCounter;

    private void ConnectToTwitch()
    {
        Twitch = new TcpClient(URL, PORT);
        Reader = new StreamReader(Twitch.GetStream());
        Writer = new StreamWriter(Twitch.GetStream());

        Writer.WriteLine("PASS " + OAuth);
        Writer.WriteLine("NICK " + User.ToLower()); //"NICK" = nickname
        Writer.WriteLine("JOIN #" + Channel.ToLower());
        Writer.Flush(); // sends all the stuff you wrote to the tcp so it actually connects
    }

    void Awake()
    {
        ConnectToTwitch();
    }

    void Update()
    {
        pingCounter += Time.deltaTime;
        if (pingCounter > 60)
        {
            Writer.WriteLine("PING " + URL);
            Writer.Flush();
            pingCounter = 0;
        }

        if (!Twitch.Connected)
        {
            ConnectToTwitch();
        }

        if (Twitch.Available > 0)
        {
            string message = Reader.ReadLine();

            if (message.Contains("PRIVMSG"))
            {
                int splitPoint = message.IndexOf("!");
                string chatter = message.Substring(1, splitPoint - 1);

                splitPoint = message.IndexOf(":", 1);
                string msg = message.Substring(splitPoint + 1);

                // Add exclamation mark to messages that are not already prefixed
                if (!msg.StartsWith("!"))
                {
                    msg = "!" + msg;
                }

                // Invoke the event to handle the chat message
                OnChatMessage?.Invoke(chatter, msg);

                print(msg);
            }
        }
    }
}
