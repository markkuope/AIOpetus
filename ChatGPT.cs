using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine.Events;

namespace OpenAI
{
    public class ChatGPT : MonoBehaviour
    {
        [SerializeField] private InputField inputField;
        [SerializeField] private Button button;
        [SerializeField] private ScrollRect scroll;
        
        [SerializeField] private RectTransform sent;
        [SerializeField] private RectTransform received;

        private float height;
        private OpenAIApi openai = new OpenAIApi();

        private List<ChatMessage> messages = new List<ChatMessage>();
        private string prompt = "Speak Finnish. Your name is Linda, you live in a chat room and reply to the questions. Don't break character. Don't ever mention that you are an AI model.";

        //animaation aloitusmuuttuja
        public UnityEvent OnReplyReceived;

        //TextToSpeech -scriptin kutsumiseksi tarvitaan
        public TextToSpeech textToSpeech;
        public string _message;

        //Whisper -scriptin kutsumiseksi tarvitaan
        public Whisper whisper;

        // oman puheen vientiin Whisperille tarvitaan tätä
        private string userInput;


        private void Start()
        {
            button.onClick.AddListener(SendReply);
        }

        private void AppendMessage(ChatMessage message)
        {
            scroll.content.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 0);

            var item = Instantiate(message.Role == "user" ? sent : received, scroll.content);
            item.GetChild(0).GetChild(0).GetComponent<Text>().text = message.Content;

            // tässä kohdassa message.Content sisältää ensin lähetetyn, sitten vastaanotetun viestin
            //viedään tämä teksti TextToSpeech scriptiin kohtaan MakeAudioRequest

            _message = message.Content;


            item.anchoredPosition = new Vector2(0, -height);
            LayoutRebuilder.ForceRebuildLayoutImmediate(item);
            height += item.sizeDelta.y;
            scroll.content.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, height);
            scroll.verticalNormalizedPosition = 0;
        }

        public async void SendReply()
        {
            //nyt käytetään Whisper - scriptistä saatava transskriptoitu arvo

            userInput = whisper.whisperText;


            var newMessage = new ChatMessage()
            {
                Role = "user",
                //Content = inputField.text
                Content = userInput
            };
            
            AppendMessage(newMessage);

            if (messages.Count == 0) newMessage.Content = prompt + "\n" + inputField.text; 
            
            messages.Add(newMessage);
            
            button.enabled = false;
            inputField.text = "";
            inputField.enabled = false;
            
            // Complete the instruction
            var completionResponse = await openai.CreateChatCompletion(new CreateChatCompletionRequest()
            {
                Model = "gpt-3.5-turbo-0613",
                Messages = messages
            });

            //aloitetaan animaatio jos sellaista tarvitaan
            OnReplyReceived.Invoke();




            if (completionResponse.Choices != null && completionResponse.Choices.Count > 0)
            {
                var message = completionResponse.Choices[0].Message;
                message.Content = message.Content.Trim();
                
                messages.Add(message);
                AppendMessage(message);
            }
            else
            {
                Debug.LogWarning("No text was generated from this prompt.");
            }

            button.enabled = true;
            inputField.enabled = true;

            //kutsutaan TextToSpeech - scriptistä MakeAudioRequest funktiota 


            textToSpeech.MakeAudioRequest(_message);





        }
    }
}
