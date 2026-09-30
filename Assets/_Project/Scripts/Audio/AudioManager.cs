using UnityEngine;

namespace KitchenInvasion {
    public class AudioManager : ManagerBase<AudioManager> {
        [SerializeField] AudioSource sfxSource;
        [SerializeField] AudioSource musicSource;
        [SerializeField] AudioClip playerShoot;
        [SerializeField] AudioClip playerDeath;
        [SerializeField] AudioClip enemySpawn;
        [SerializeField] AudioClip enemyKilled;
        [SerializeField] AudioClip pepperShoot;
        [SerializeField] AudioClip tomatoHit;
        [SerializeField] AudioClip buttonClick;
        [SerializeField] AudioClip backgroundMusic;

        void Start() {
            musicSource.clip = backgroundMusic;
            musicSource.loop = true;
            musicSource.Play();
        }

        void OnEnable() {
            GameEvents.OnPlayerShot += PlayPlayerShoot;
            GameEvents.OnPlayerDied += PlayPlayerDeath;
            GameEvents.OnEnemySpawned += PlayEnemySpawn;
            GameEvents.OnEnemyKilled += PlayEnemyKilled;
            GameEvents.OnEnemyShot += PlayPepperShoot;
            GameEvents.OnMeleeHit += PlayTomatoHit;
            GameEvents.OnButtonClicked += PlayButtonClick;
        }

        void OnDisable() {
            GameEvents.OnPlayerShot -= PlayPlayerShoot;
            GameEvents.OnPlayerDied -= PlayPlayerDeath;
            GameEvents.OnEnemySpawned -= PlayEnemySpawn;
            GameEvents.OnEnemyKilled -= PlayEnemyKilled;
            GameEvents.OnEnemyShot -= PlayPepperShoot;
            GameEvents.OnMeleeHit -= PlayTomatoHit;
            GameEvents.OnButtonClicked -= PlayButtonClick;
        }

        void Play(AudioClip clip, float volume = 1f) {
            if (clip != null) sfxSource.PlayOneShot(clip, volume);
        }

        void PlayPlayerShoot() { Play(playerShoot, 0.6f); }
        void PlayPlayerDeath() { Play(playerDeath); }
        void PlayEnemySpawn() { Play(enemySpawn, 0.7f); }
        void PlayEnemyKilled(Enemy enemy) { Play(enemyKilled); }
        void PlayPepperShoot() { Play(pepperShoot, 0.7f); }
        void PlayTomatoHit() { Play(tomatoHit); }
        void PlayButtonClick() { Play(buttonClick); }
    }
}
