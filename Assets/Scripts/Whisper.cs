


using UnityEngine;
using TMPro;
using System.Collections;
using System.IO;
using UnityEngine.Networking;
using UnityEngine.Events;
#if PLATFORM_ANDROID
using UnityEngine.Android;
#endif


[System.Serializable] public class StringEvent : UnityEvent<string> { }

public class Whisper : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] TMP_Text transcriptText;

    [Header("Audio")]
    [SerializeField] int sampleRate = 44100;
    [SerializeField] int maxSeconds = 15;

    AudioClip clip;
    bool isRecording;
    bool isUploading;

    /* Cool-down to avoid 429 */
    float whisperCooldown = 10f;
    float lastRequestTime = -100f;
    public StringEvent onTranscriptionFinished;
    string microphoneDevice;

    void Start()
    {
        transcriptText.text = "<i>Waiting…</i>";
#if PLATFORM_ANDROID
        if (!Permission.HasUserAuthorizedPermission(Permission.Microphone)){
            Permission.RequestUserPermission(Permission.Microphone);
        }
#endif

        if (Microphone.devices.Length > 0)
        {
            microphoneDevice = Microphone.devices[0];

        }
        else
        {
            transcriptText.text = "<color=red>Microphone not found</color>";
        }
    }

    /* ─────────────────────────────────────────────
     * PUBLIC-API  ➜  call these from other scripts
     * ──────────────────────────────────────────── */

    public void BeginRecording()
    {
        if (isRecording || isUploading) return;

        clip = Microphone.Start(microphoneDevice, false, maxSeconds, sampleRate);
        isRecording = true;
        transcriptText.text = "<i>Recording…</i>";
    }

    public void EndRecording(bool saveWavLocally = true)
    {
        if (!isRecording) return;

        Microphone.End(microphoneDevice);
        transcriptText.text = "Mic deactivated";
        isRecording = false;

        if (saveWavLocally)
        {
            transcriptText.text = "trying to save clip";
            SaveClipAsWav(clip);
            transcriptText.text = "clip saved";
        }

        transcriptText.text = "<i>Recording done. Ready to upload.</i>";
    }

    public void UploadLastClip()
    {
        if (clip == null || isUploading) return;
        transcriptText.text = "<i>Analysing…</i>";
        StartCoroutine(SendToWhisper(clip));
    }

    /* ───────────────────────────────────────────── */

    void SaveClipAsWav(AudioClip ac)
    {
        string folder = Path.Combine(Application.persistentDataPath, "DebugRecordings");
        transcriptText.text = "creating folder";
        Directory.CreateDirectory(folder);
        transcriptText.text = "created folder, saving clip";
        string name = System.DateTime.UtcNow.ToString("yyMMdd-HHmmss-fff") + ".wav";
        File.WriteAllBytes(Path.Combine(folder, name), WavUtility.FromAudioClip(ac));
        Debug.Log($"🎙 Saved WAV ➜ Assets/DebugRecordings/{name}");
    }

    IEnumerator SendToWhisper(AudioClip recorded)
    {
        /* cool-down */
        if (Time.time - lastRequestTime < whisperCooldown)
        {
            float wait = whisperCooldown - (Time.time - lastRequestTime);
            transcriptText.text = $"<color=orange>Wait {wait:F1}s…</color>";
            yield break;
        }
        lastRequestTime = Time.time;
        isUploading = true;

        /* build request */
        var form = new WWWForm();
        form.AddBinaryData("file", WavUtility.FromAudioClip(recorded),
                           "speech.wav", "audio/wav");
        form.AddField("model", "whisper-1");

        var req = UnityWebRequest.Post(
            "https://api.openai.com/v1/audio/transcriptions", form);
        req.SetRequestHeader("Authorization", "Bearer " + Secrets.OPENAI_KEY);

        yield return req.SendWebRequest();
        isUploading = false;

        if (req.result == UnityWebRequest.Result.Success)
        {
            string text = JsonUtility.FromJson<WhisperJson>(req.downloadHandler.text).text;
            /*
            transcriptText.text = string.IsNullOrWhiteSpace(text)
                                  ? "<i>(no speech detected)</i>"
                                  : "<b>Transcription:</b>\n" + text;

            */

            if (!string.IsNullOrWhiteSpace(text))
            {

                onTranscriptionFinished?.Invoke(text);
            }
            else
            {
                onTranscriptionFinished?.Invoke("no speech detected");
            }
        }
        else
        {
            // transcriptText.text = $"<color=red>HTTP {req.responseCode}</color>";

            onTranscriptionFinished?.Invoke(req.responseCode.ToString());
        }
    }

    [System.Serializable] class WhisperJson { public string text; }
}

