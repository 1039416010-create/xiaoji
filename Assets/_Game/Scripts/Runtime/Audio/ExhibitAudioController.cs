using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

namespace GroundChickenKing.Audio
{
    public sealed class ExhibitAudioController : MonoBehaviour
    {
        [SerializeField] private AudioMixer _mixer;
        [SerializeField] private AudioMixerGroup _musicGroup;
        [SerializeField] private AudioMixerGroup _sfxGroup;
        private AudioSource _music;
        private AudioSource _sfx;

        public void Configure(AudioMixer mixer, AudioMixerGroup musicGroup, AudioMixerGroup sfxGroup)
        { _mixer = mixer; _musicGroup = musicGroup; _sfxGroup = sfxGroup; }

        private void Awake()
        {
            _music = gameObject.AddComponent<AudioSource>(); _music.loop = true; _music.playOnAwake = false; _music.outputAudioMixerGroup = _musicGroup;
            _sfx = gameObject.AddComponent<AudioSource>(); _sfx.playOnAwake = false; _sfx.outputAudioMixerGroup = _sfxGroup;
            _music.clip = CreateTone("BGM_ExhibitLoop", 110f, 2f, 0.025f); _music.Play();
        }

        private void Start() => BindVisibleButtons();

        public void BindVisibleButtons()
        {
            foreach (var button in FindObjectsByType<Button>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            {
                button.onClick.RemoveListener(PlayButton);
                button.onClick.AddListener(PlayButton);
            }
        }

        public void SetMasterVolume(float linear)
        {
            var value = Mathf.Clamp01(linear);
            AudioListener.volume = value;
        }

        public void PlayButton() => PlayTone(520f, 0.07f, 0.12f);
        public void PlayCountdown() => PlayTone(660f, 0.12f, 0.16f);
        public void PlayGo() => PlayTone(990f, 0.24f, 0.18f);
        public void PlayFinish() => PlayTone(784f, 0.35f, 0.2f);
        public void PlaySettlement() => PlayTone(880f, 0.45f, 0.16f);

        private void PlayTone(float frequency, float duration, float amplitude)
        { if (_sfx != null) _sfx.PlayOneShot(CreateTone($"SFX_{frequency:0}", frequency, duration, amplitude)); }

        private static AudioClip CreateTone(string name, float frequency, float duration, float amplitude)
        {
            const int sampleRate = 22050; var count = Mathf.Max(1, Mathf.RoundToInt(sampleRate * duration)); var data = new float[count];
            for (var i = 0; i < count; i++) { var fade = 1f - i / (float)count; data[i] = Mathf.Sin(2f * Mathf.PI * frequency * i / sampleRate) * amplitude * fade; }
            var clip = AudioClip.Create(name, count, 1, sampleRate, false); clip.SetData(data, 0); return clip;
        }
    }
}
