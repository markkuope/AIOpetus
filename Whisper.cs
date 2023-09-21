using UnityEngine;
using UnityEngine.UI;

namespace OpenAI
{
    public class Whisper : MonoBehaviour
    {
        [SerializeField] private Button recordButton;
        [SerializeField] private Image progress;
        [SerializeField] private Text message;
        [SerializeField] private Dropdown dropdown;
        
        private readonly string fileName = "output.wav";
        private readonly int duration = 15; //äänityksen kesto asetettu 15s, voi muuttaa täältä

        private AudioClip clip;
        private bool isRecording;
        private float time;
        private OpenAIApi openai = new OpenAIApi();

        private void Start()
        {
            #if UNITY_WEBGL && !UNITY_EDITOR
            dropdown.options.Add(new Dropdown.OptionData("Microphone not supported on WebGL"));
            #else
            foreach (var device in Microphone.devices)
            {
                dropdown.options.Add(new Dropdown.OptionData(device));
            }
            recordButton.onClick.AddListener(StartRecording);
            dropdown.onValueChanged.AddListener(ChangeMicrophone);
            
            var index = PlayerPrefs.GetInt("user-mic-device-index");
            dropdown.SetValueWithoutNotify(index);
            #endif
        }

        private void ChangeMicrophone(int index)
        {
            PlayerPrefs.SetInt("user-mic-device-index", index);
        }

        //private void StartRecording()
        //{
        //    isRecording = true;
        //    recordButton.enabled = false;

        //    var index = PlayerPrefs.GetInt("user-mic-device-index");

        //    #if !UNITY_WEBGL
        //    clip = Microphone.Start(dropdown.options[index].text, false, duration, 44100);
        //    #endif
        //}

        private async void StartRecording()
        {
            if (isRecording)
            {
                isRecording = false;
                Debug.Log("Stop recording...");

                time = 0;//aloitetaan äänityksen ajanlasku aina nollasta, tämä on lisäys Sargen koodiin
                

                Microphone.End(null);
                byte[] data = SaveWav.Save(fileName, clip);

                var req = new CreateAudioTranscriptionsRequest
                {
                    FileData = new FileData() { Data = data, Name = "audio.wav" },
                    // File = Application.persistentDataPath + "/" + fileName,
                    Model = "whisper-1",
                    Language = "fi"   // tässä määritellään kieli, fi = suomi, en = englanti
                };
                var res = await openai.CreateAudioTranscription(req);
                progress.fillAmount = 0;
                message.text = res.Text;   // message saa arvokseen OpenAI:lta  vastaanotetun tekstin
                recordButton.enabled = true;

                //tässä kohdassa message.text arvo on whisper AI:n transkriptio
                //print(message.text);
                // tämä arvo viedään AI:lle HelmiChatGPT scriptiin

                //whisperText = message.text;

                //tässä käynnistetään HelmiChatGPT SendReply -funkio
                //ettei tarvitse painaa Send -nappia

                //helmiChatGPT.SendReply();

            }
            else
            {
                //Debug.Log("Start recording...");
                isRecording = true;

                var index = PlayerPrefs.GetInt("user-mic-device-index");
                clip = Microphone.Start(dropdown.options[index].text, false, duration, 44100);
            }
        }

        // vaihtoehtoisesti audion kääntämispyyntö
        //var req = new CreateAudioTranslationRequest
        //{
        //    FileData = new FileData() { Data = data, Name = "audio.wav" },
        //    // File = Application.persistentDataPath + "/" + fileName,
        //    Model = "whisper-1",
        //};






        //private async void EndRecording()
        //{
        //    message.text = "Transcripting...";

        //    #if !UNITY_WEBGL
        //    Microphone.End(null);
        //    #endif

        //    byte[] data = SaveWav.Save(fileName, clip);

        //    var req = new CreateAudioTranscriptionsRequest
        //    {
        //        FileData = new FileData() {Data = data, Name = "audio.wav"},
        //        // File = Application.persistentDataPath + "/" + fileName,
        //        Model = "whisper-1",
        //        Language = "fi"
        //    };
        //    var res = await openai.CreateAudioTranscription(req);

        //    progress.fillAmount = 0;
        //    message.text = res.Text;
        //    recordButton.enabled = true;
        //}

        //private void Update()
        //{
        //    if (isRecording)
        //    {
        //        time += Time.deltaTime;
        //        progress.fillAmount = time / duration;

        //        if (time >= duration)
        //        {
        //            time = 0;
        //            isRecording = false;
        //            EndRecording();
        //        }
        //    }
        //}

        private void Update()
        {
            if (isRecording)
            {
                time += Time.deltaTime;
                progress.fillAmount = time / duration;

                if (time >= duration)
                {
                    time = 0;
                    progress.fillAmount = 0;
                    StartRecording();

                }
            }
        }



    }
}
