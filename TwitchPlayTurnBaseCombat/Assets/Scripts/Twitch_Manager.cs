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
            //if something is available in the TCP that we can grab with the stream reader
            string message = Reader.ReadLine(); //reads the next available line in our TCP

            if (message.Contains("PRIVMSG"))    //code that will come with any message that's written by a user
            {
                //so this comment below here is all the info that's sent in a message... it's a lot so we have to parse it.
                // :soomoh!soomoh@soomoh.tmi.twitch.tv PRIVMSG #soomoh :hello world


                int splitPoint = message.IndexOf("!");  //notice the '!' in front of the message - we can use this '!' to isolate the username
                string chatter = message.Substring(1, splitPoint - 1);  //extracts the first word... in this case, 'soomoh'
                                                                        //aka the person speaking

                splitPoint = message.IndexOf(":", 1);   //so everything after that colon is the message that the user typed... in this case, 'hello world'
                string msg = message.Substring(splitPoint + 1);    //anything that's passed that colon, bring it in to that string

                //This UnityEvent is what will look at the messager, and their message - then it will invoke a method from another script that we assign! 
                //You can assign the method it invokes in the inspector.
                OnChatMessage?.Invoke(chatter, msg);    //side note - 'Invoke' is a method that calls another method.


                //now you can simply put a series of if statements in the method that you assign to the UnityEvent... 
                //if msg == "A", do something... 
                //if msg == "B", do something...
                //etc, etc

                print(msg);

                //if(msg == "a")
                //{
                //    Debug.Log("someone pressed A");
                //    GameObject.FindObjectOfType<GameManager>().GetComponent<GameManager>().A_Pressed();
                //}

                //if (msg == "d")
                //{
                //    Debug.Log("someone pressed D");
                //    GameObject.FindObjectOfType<GameManager>().GetComponent<GameManager>().D_Pressed();
                //}

                //if (msg == "w")
                //{
                //    Debug.Log("someone pressed W");
                //    GameObject.FindObjectOfType<GameManager>().GetComponent<GameManager>().W_Pressed();
                //}

                //if (msg == "s")
                //{
                //    Debug.Log("someone pressed S");
                //    GameObject.FindObjectOfType<GameManager>().GetComponent<GameManager>().S_Pressed();
                //}
            }
        }
    }
}
