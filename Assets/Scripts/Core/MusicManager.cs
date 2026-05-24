using UnityEngine;

namespace TowerDefenseMVP
{
    public sealed class MusicManager : MonoBehaviour
    {
        public static MusicManager Instance { get; private set; }

        private const string MusicVolumeKey = "TD_MUSIC_VOLUME";
        private const string MusicMutedKey = "TD_MUSIC_MUTED";

        private AudioSource audioSource;
        private float musicVolume = 0.35f;
        private bool musicMuted;

        public float MusicVolume => musicVolume;
        public bool MusicMuted => musicMuted;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;

            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
            audioSource.loop = true;

            musicVolume = PlayerPrefs.GetFloat(MusicVolumeKey, 0.35f);
            musicMuted = PlayerPrefs.GetInt(MusicMutedKey, 0) == 1;

            ApplyVolume();

            AudioClip music = Resources.Load<AudioClip>("Audio/Music/BGM");

            if (music == null)
            {
                Debug.LogWarning("MusicManager: BGM not found. Expected: Assets/Resources/Audio/Music/BGM.wav or .ogg");
                return;
            }

            audioSource.clip = music;
            audioSource.Play();
        }

        private void OnDestroy()
        {
            if (Instance == this)
                Instance = null;
        }

        public void SetVolume(float volume)
        {
            musicVolume = Mathf.Clamp01(volume);
            PlayerPrefs.SetFloat(MusicVolumeKey, musicVolume);
            PlayerPrefs.Save();
            ApplyVolume();
        }

        public void SetMuted(bool muted)
        {
            musicMuted = muted;
            PlayerPrefs.SetInt(MusicMutedKey, muted ? 1 : 0);
            PlayerPrefs.Save();
            ApplyVolume();
        }

        public void StopMusic()
        {
            if (audioSource != null)
                audioSource.Stop();
        }

        public void PlayMusic()
        {
            if (audioSource != null && !audioSource.isPlaying)
                audioSource.Play();
        }

        private void ApplyVolume()
        {
            if (audioSource == null)
                return;

            audioSource.volume = musicMuted ? 0f : musicVolume;
        }

        public static void SetMusicVolume(float volume)
        {
            if (Instance == null)
                return;

            Instance.SetVolume(volume);
        }

        public static void SetMusicMuted(bool muted)
        {
            if (Instance == null)
                return;

            Instance.SetMuted(muted);
        }
    }
}
