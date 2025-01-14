using System.Collections;
using Hmxs.Scripts;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;
using Rigidbody = UnityEngine.Rigidbody;

namespace FPS
{
	public class AutomaticGun : MonoBehaviour
	{
		private static readonly int Aim = Animator.StringToHash("Aim");
		private static readonly int Inspect = Animator.StringToHash("Inspect");
		private static readonly int Holster = Animator.StringToHash("Holster");
		private static readonly int Walk = Animator.StringToHash("Walk");
		private static readonly int Run = Animator.StringToHash("Run");

		[Title("Gun Camera")]
		[SerializeField] private Camera gunCamera; //Main gun camera

		[Title("Gun Camera Options")]
		[SerializeField] private float fovSpeed = 15.0f; //How fast the camera field of view changes when aiming
		[SerializeField] private float defaultFov = 40.0f; //Default camera field of view
		[SerializeField] private float aimFov = 25.0f;

		[Title("UI Weapon Name")]
		[SerializeField] private string weaponName;
		private string _storedWeaponName;

		[Title("Weapon Sway")]
		[SerializeField] private bool weaponSway; //Enables weapon sway
		[SerializeField] private float swayAmount = 0.02f;
		[SerializeField] private float maxSwayAmount = 0.06f;
		[SerializeField] private float swaySmoothValue = 4.0f;
		private Vector3 _initialSwayPosition;
		private float _lastFired; //Used for fire rate

		[Title("Weapon Settings")]
		[SerializeField] private float fireRate; //How fast the weapon fires, higher value means faster rate of fire
		[SerializeField] private bool autoReload; //Enables auto reloading when out of ammo
		[SerializeField] private float autoReloadDelay; //Delay between shooting last bullet and reloading

		private bool _isReloading; //Check if reloading
		private bool _hasBeenHolstered; //Holstering weapon
		private bool _holstered; //If weapon is holstered
		private bool _isRunning; //Check if running
		private bool _isAiming; //Check if aiming
		private bool _isWalking; //Check if walking
		private bool _isInspecting; //Check if inspecting weapon
		private int _currentAmmo; //How much ammo is currently left

		[SerializeField] private int ammo; //Total amount of ammo
		private bool _outOfAmmo; //Check if out of ammo

		[Title("Bullet Settings")]
		[SerializeField] private float bulletForce = 400.0f;
		[SerializeField] private float showBulletInMagDelay = 0.6f;
		[SerializeField] private SkinnedMeshRenderer bulletInMagRenderer;

		[Title("Grenade Settings")]
		[SerializeField] private float grenadeSpawnDelay = 0.35f;
		[SerializeField] private float grenadeCooldown = 1.5f;
		private bool _canThrowGrenade = true;

		[Title("Muzzle flash Settings")]
		[SerializeField] private bool randomMuzzleFlash;
		private const int minRandomValue = 1; //min should always bee 1

		[SerializeField, Range(2, 25)] private int maxRandomValue = 5;
		private int _randomMuzzleFlashValue;

		[SerializeField] private bool enableMuzzleFlash = true;
		[SerializeField] private ParticleSystem muzzleParticles;
		[SerializeField] private bool enableSparks = true;
		[SerializeField] private ParticleSystem sparkParticles;
		[SerializeField] private int minSparkEmission = 1;
		[SerializeField] private int maxSparkEmission = 7;

		[Title("Muzzle flash Light Settings")] [SerializeField] private Light muzzleFlashLight;
		[SerializeField] private float lightDuration = 0.02f;

		[Title("Audio Source")]
		[SerializeField] private AudioSource mainAudioSource; //Main audio source
		[SerializeField] private AudioSource shootAudioSource; //Audio source used for shoot sound

		[Title("UI Components")]
		[SerializeField] private Text currentWeaponText;
		[SerializeField] private Text currentAmmoText;
		[SerializeField] private Text totalAmmoText;
		[SerializeField] private GameObject crossHair;

		[SerializeField, FoldoutGroup("Prefabs"), AssetsOnly] private Transform bulletPrefab;
		[SerializeField, FoldoutGroup("Prefabs"), AssetsOnly] private Transform casingPrefab;
		[SerializeField, FoldoutGroup("Prefabs"), AssetsOnly] private Transform grenadePrefab;

		[SerializeField, FoldoutGroup("SpawnPoints")] private Transform casingSpawnPoint;
		[SerializeField, FoldoutGroup("SpawnPoints")] private Transform bulletSpawnPoint;
		[SerializeField, FoldoutGroup("SpawnPoints")] private Transform grenadeSpawnPoint;

		[SerializeField, FoldoutGroup("Audios"), AssetsOnly] private AudioClip shootSound;
		[SerializeField, FoldoutGroup("Audios"), AssetsOnly] private AudioClip takeOutSound;
		[SerializeField, FoldoutGroup("Audios"), AssetsOnly] private AudioClip holsterSound;
		[SerializeField, FoldoutGroup("Audios"), AssetsOnly] private AudioClip reloadSoundOutOfAmmo;
		[SerializeField, FoldoutGroup("Audios"), AssetsOnly] private AudioClip reloadSoundAmmoLeft;
		[SerializeField, FoldoutGroup("Audios"), AssetsOnly] private AudioClip aimSound;
		private bool _soundHasPlayed;

		[Title("Input")]
		[SerializeField] private InputSetting inputSetting;
		[SerializeField] private bool firstShoot = true;

		private Animator _animator; //Animator component attached to weapon

		private void Awake()
		{
			_animator = GetComponent<Animator>();
			_currentAmmo = ammo;
			muzzleFlashLight.enabled = false;
		}

		private void Start()
		{
			_storedWeaponName = weaponName; //Save the weapon name
			currentWeaponText.text = weaponName; //Get weapon name from string to text
			totalAmmoText.text = ammo.ToString(); //Set total ammo text from total ammo int
			_initialSwayPosition = transform.localPosition; //Weapon sway
			shootAudioSource.clip = shootSound; //Set the shoot sound to audio source
		}

		private void Update()
		{
			//Aiming: Toggle camera FOV when right click is held down
			if (JoyconInput.instance.GetButton(inputSetting.aimButtonDesc) && !_isReloading && !_isRunning && !_isInspecting)
			{	//Start aiming
				_isAiming = true;
				_animator.SetBool(Aim, true);
				gunCamera.fieldOfView = Mathf.Lerp(gunCamera.fieldOfView, aimFov, fovSpeed * Time.deltaTime);
				inputSetting.rotateType = RotateType.Gyro;
				crossHair.SetActive(false);
				if (!_soundHasPlayed)
				{
					mainAudioSource.clip = aimSound;
					mainAudioSource.Play();
					_soundHasPlayed = true;
				}
			}
			else //Stop aiming
			{
				gunCamera.fieldOfView = Mathf.Lerp(gunCamera.fieldOfView, defaultFov, fovSpeed * Time.deltaTime);
				_isAiming = false;
				_animator.SetBool(Aim, false);
				_soundHasPlayed = false;
				inputSetting.rotateType = RotateType.Orientation;
				crossHair.SetActive(true);
			}


			//If randomize muzzleFlash is true, generate random int values
			if (randomMuzzleFlash)
				_randomMuzzleFlashValue = Random.Range(minRandomValue, maxRandomValue);

			currentAmmoText.text = _currentAmmo.ToString(); //Set current ammo text from ammo int

			//Continuously check which animation
			//is currently playing
			AnimationCheck();

			//Throw grenade
			var accel = JoyconInput.instance.GetAccel(0);
			if (accel.magnitude > 2.5f && JoyconInput.instance.GetButton(inputSetting.throwGrenadeButtonDesc) && _canThrowGrenade && !_isInspecting)
			{
				StartCoroutine(GrenadeSpawnDelay(accel.magnitude));
				_animator.Play("GrenadeThrow", 0, 0.0f); //Play grenade throw animation
			}

			if (_currentAmmo == 0) //If out of ammo
			{
				currentWeaponText.text = "OUT OF AMMO";
				_outOfAmmo = true;
				if (autoReload && !_isReloading) //Auto reload if true
					StartCoroutine(AutoReload());
			}
			else
			{
				currentWeaponText.text = _storedWeaponName; //When ammo is full, show weapon name again
				_outOfAmmo = false;
			}

			//Automatic fire
			//Left click hold
			if (JoyconInput.instance.GetButton(inputSetting.fireButtonDesc) && !_outOfAmmo && !_isReloading && !_isInspecting && !_isRunning)
			{
				if (firstShoot) { firstShoot = false; return; }

				//Shoot automatic
				if (Time.time - _lastFired > 1 / fireRate)
				{
					_lastFired = Time.time;
					_currentAmmo -= 1;
					shootAudioSource.clip = shootSound;
					shootAudioSource.Play();
					if (!_isAiming) //if not aiming
					{
						_animator.Play("Fire", 0, 0f);
						if (!randomMuzzleFlash && enableMuzzleFlash) //If random muzzle is false
						{
							muzzleParticles.Emit(1);
							StartCoroutine(MuzzleFlashLight());
						}
						else if (randomMuzzleFlash)
						{
							if (_randomMuzzleFlashValue == 1) //Only emit if random value is 1
							{
								if (enableSparks) //Emit random amount of spark particles
									sparkParticles.Emit(Random.Range(minSparkEmission, maxSparkEmission));
								if (enableMuzzleFlash)
								{
									muzzleParticles.Emit(1); //Light flash start
									StartCoroutine(MuzzleFlashLight());
								}
							}
						}
					}
					else //if aiming
					{
						_animator.Play("Aim Fire", 0, 0f);
						if (!randomMuzzleFlash) //If random muzzle is false
							muzzleParticles.Emit(1);
						else if (randomMuzzleFlash) //If random muzzle is true
						{
							if (_randomMuzzleFlashValue == 1) //Only emit if random value is 1
							{
								if (enableSparks) //Emit random amount of spark particles
									sparkParticles.Emit(Random.Range(minSparkEmission, maxSparkEmission));
								if (enableMuzzleFlash)
								{
									muzzleParticles.Emit(1); //Light flash start
									StartCoroutine(MuzzleFlashLight());
								}
							}
						}
					}

					//Spawn bullet from bullet spawn point
					var bullet = Instantiate(bulletPrefab, bulletSpawnPoint.transform.position, bulletSpawnPoint.transform.rotation);
					//Add velocity to the bullet
					bullet.GetComponent<Rigidbody>().velocity = bullet.transform.forward * bulletForce;
					//Spawn casing prefab at spawn point
					Instantiate(casingPrefab, casingSpawnPoint.transform.position, casingSpawnPoint.transform.rotation);
				}
			}

			//Inspect weapon
			if (JoyconInput.instance.GetButtonDown(inputSetting.inspectButtonDesc))
				_animator.SetTrigger(Inspect);

			//Reload
			if (JoyconInput.instance.GetButtonDown(inputSetting.reloadButtonDesc) && !_isReloading && !_isInspecting) Reload();

			var stick = JoyconInput.instance.GetStick(0);
			//Walking
			_animator.SetBool(Walk, (stick[0] != 0 || stick[1] != 0) && !_isRunning);
			//Running
			_isRunning = stick[1] > 0 && JoyconInput.instance.GetButton(inputSetting.runButtonDesc);
			_animator.SetBool(Run, _isRunning);
		}

		private void LateUpdate() => SwayWeapon();

		private void SwayWeapon()
		{
			if (!weaponSway) return;
			var gyro = JoyconInput.instance.GetGyro(0);
			float movementX = -gyro.z * swayAmount;
			float movementY = -gyro.y * swayAmount;
			//Clamp movement to min and max values
			movementX = Mathf.Clamp(movementX, -maxSwayAmount, maxSwayAmount);
			movementY = Mathf.Clamp(movementY, -maxSwayAmount, maxSwayAmount);
			//Lerp local pos
			Vector3 finalSwayPosition = new Vector3(movementX, movementY, 0);
			transform.localPosition = Vector3.Lerp(
				transform.localPosition,
				finalSwayPosition + _initialSwayPosition,
				Time.deltaTime * swaySmoothValue);
		}

		private IEnumerator GrenadeSpawnDelay(float accel)
		{
			_canThrowGrenade = false;
			yield return new WaitForSeconds(grenadeSpawnDelay);
			var obj = Instantiate(grenadePrefab, grenadeSpawnPoint.transform.position, grenadeSpawnPoint.transform.rotation);
			var grenadeScript = obj.GetComponent<GrenadeScript>();
			grenadeScript.Throw(accel);
			yield return new WaitForSeconds(grenadeCooldown);
			_canThrowGrenade = true;
		}

		private IEnumerator AutoReload()
		{
			yield return new WaitForSeconds(autoReloadDelay);
			if (_outOfAmmo)
			{
				_animator.Play("Reload Out Of Ammo", 0, 0f);
				mainAudioSource.clip = reloadSoundOutOfAmmo;
				mainAudioSource.Play();
				//If out of ammo, hide the bullet renderer in the mag
				//Do not show if bullet renderer is not assigned in inspector
				if (bulletInMagRenderer)
				{
					bulletInMagRenderer.GetComponent<SkinnedMeshRenderer>().enabled = false;
					StartCoroutine(ShowBulletInMag());
				}
			}
			//Restore ammo when reloading
			_currentAmmo = ammo;
			_outOfAmmo = false;
		}

		//Reload
		private void Reload()
		{
			if (_outOfAmmo)
			{
				_animator.Play("Reload Out Of Ammo", 0, 0f);
				mainAudioSource.clip = reloadSoundOutOfAmmo;
				mainAudioSource.Play();
				//If out of ammo, hide the bullet renderer in the mag
				//Do not show if bullet renderer is not assigned in inspector
				if (bulletInMagRenderer)
				{
					bulletInMagRenderer.GetComponent<SkinnedMeshRenderer>().enabled = false;
					StartCoroutine(ShowBulletInMag());
				}
			}
			else
			{
				_animator.Play("Reload Ammo Left", 0, 0f);
				mainAudioSource.clip = reloadSoundAmmoLeft;
				mainAudioSource.Play();
				//If reloading when ammo left, show bullet in mag
				//Do not show if bullet renderer is not assigned in inspector
				if (bulletInMagRenderer)
					bulletInMagRenderer.GetComponent<SkinnedMeshRenderer>().enabled = true;
			}
			//Restore ammo when reloading
			_currentAmmo = ammo;
			_outOfAmmo = false;
		}

		//Enable bullet in mag renderer after set amount of time
		private IEnumerator ShowBulletInMag()
		{
			//Wait set amount of time before showing bullet in mag
			yield return new WaitForSeconds(showBulletInMagDelay);
			bulletInMagRenderer.GetComponent<SkinnedMeshRenderer>().enabled = true;
		}

		//Show light when shooting, then disable after set amount of time
		private IEnumerator MuzzleFlashLight()
		{
			muzzleFlashLight.enabled = true;
			yield return new WaitForSeconds(lightDuration);
			muzzleFlashLight.enabled = false;
		}

		//Check current animation playing
		private void AnimationCheck()
		{
			//Check if reloading
			//Check both animations
			_isReloading = _animator.GetCurrentAnimatorStateInfo(0).IsName("Reload Out Of Ammo") ||
			               _animator.GetCurrentAnimatorStateInfo(0).IsName("Reload Ammo Left");
			//Check if inspecting weapon
			_isInspecting = _animator.GetCurrentAnimatorStateInfo(0).IsName("Inspect");
		}
	}
}
