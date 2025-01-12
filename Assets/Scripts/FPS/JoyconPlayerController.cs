using System;
using System.Linq;
using Hmxs.Scripts;
using Sirenix.OdinInspector;
using UnityEngine;

namespace FPS
{
	[RequireComponent(typeof(Rigidbody), typeof(CapsuleCollider), typeof(AudioSource))]
	public class JoyconPlayerController : MonoBehaviour
	{
		[Title("Arms")]
		[SerializeField] private Transform arms;
		[SerializeField] private Vector3 armsPosition;

		[Title("Movement")]
		[SerializeField] private float walkingSpeed = 5.0f;
		[SerializeField] private float runningSpeed = 9.0f;
		[SerializeField] private float movementSmoothness = 0.125f;
		[SerializeField] private float jumpForce = 30f;

		[Title("Look")]
		[SerializeField] private float rotationSmoothing = 0.05f;
		[SerializeField] private float orientationVerticalSensitivity = 1.0f;
		[SerializeField] private float orientationHorizontalSensitivity = 1.0f;
		[SerializeField] private float gyroVerticalSensitivity = 0.5f;
		[SerializeField] private float gyroHorizontalSensitivity = 0.5f;
		[SerializeField] private float minVerticalAngle = -90.0f;
		[SerializeField] private float maxVerticalAngle = 90.0f;

		[Title("AudioClips")]
		[SerializeField] private AudioClip walkingSound;
		[SerializeField] private AudioClip runningSound;

		[Title("Input")]
		[SerializeField] private InputSetting inputSetting;

		private Rigidbody _rigidbody;
		private CapsuleCollider _collider;
		private AudioSource _audioSource;

		private bool _isGrounded;
		private readonly RaycastHit[] _groundCastResults = new RaycastHit[8];
		private readonly RaycastHit[] _wallCastResults = new RaycastHit[8];

		private SmoothVelocity _velocityX;
		private SmoothVelocity _velocityZ;
		private SmoothRotation _rotationX;
		private SmoothRotation _rotationY;

		private void Start()
		{
			_rigidbody = GetComponent<Rigidbody>();
			_rigidbody.constraints = RigidbodyConstraints.FreezeRotation;
			_collider = GetComponent<CapsuleCollider>();
			_audioSource = GetComponent<AudioSource>();
			_audioSource.clip = walkingSound;
			_audioSource.loop = true;
			arms.SetPositionAndRotation(transform.position, transform.rotation);
			ValidateRotationAngleRestrictions();
			_velocityX = new SmoothVelocity();
			_velocityZ = new SmoothVelocity();
			_rotationX = new SmoothRotation();
			_rotationY = new SmoothRotation();
		}

		private void ValidateRotationAngleRestrictions()
		{
			if (minVerticalAngle is < -90.0f or > 90.0f)
			{
				Debug.LogWarning("Minimum vertical angle should be between -90 and 90 degrees.");
				minVerticalAngle = Mathf.Clamp(minVerticalAngle, -90.0f, 90.0f);
			}

			if (maxVerticalAngle is < -90.0f or > 90.0f)
			{
				Debug.LogWarning("Maximum vertical angle should be between -90 and 90 degrees.");
				maxVerticalAngle = Mathf.Clamp(maxVerticalAngle, -90.0f, 90.0f);
			}

			if (maxVerticalAngle > minVerticalAngle) return;
			Debug.LogWarning("Maximum vertical angle should be greater than minimum vertical angle.");
			(maxVerticalAngle, minVerticalAngle) = (minVerticalAngle, maxVerticalAngle);
		}

		private void OnCollisionStay()
		{
			var bounds = _collider.bounds;
			var extents = bounds.extents;
			var radius = extents.x - 0.01f;
			Physics.SphereCastNonAlloc(bounds.center, radius, Vector3.down, _groundCastResults,
				extents.y - radius * 0.5f, ~0, QueryTriggerInteraction.Ignore);
			if (!_groundCastResults.Any(hit => hit.collider && hit.collider != _collider)) return;
			for (var i = 0; i < _groundCastResults.Length; i++)
				_groundCastResults[i] = new RaycastHit();
			_isGrounded = true;
		}

		private void Update()
		{
			arms.position = transform.position + transform.TransformVector(armsPosition);
			if (JoyconInput.instance.GetButtonDown(inputSetting.recenterButtonDesc))
				JoyconInput.instance.Recenter(0);
			Jump();
			PlayFootstepSounds();
		}

		private void FixedUpdate()
		{
			switch (inputSetting.rotateType)
			{
				case RotateType.Orientation:
					RotateCameraAndCharacterByOrientation();
					break;
				case RotateType.Gyro:
					RotateCameraAndCharacterByGyro();
					break;
				default:
					throw new ArgumentOutOfRangeException();
			}

			MoveCharacter();
			_isGrounded = false;
		}

		private void RotateCameraAndCharacterByOrientation()
		{
			var orientation = JoyconInput.instance.GetOrientation(0);
			var look = Quaternion.Euler(
				orientation.eulerAngles.x * orientationVerticalSensitivity,
				orientation.eulerAngles.y * orientationHorizontalSensitivity,
				0);
			var verticalAngle = NormalizeAngle(look.eulerAngles.x);
			verticalAngle = Mathf.Clamp(verticalAngle, minVerticalAngle, maxVerticalAngle);
			var horizontalAngle = look.eulerAngles.y;
			transform.eulerAngles = new Vector3(0, horizontalAngle, 0);
			arms.eulerAngles = new Vector3(verticalAngle, horizontalAngle, 0);
		}

		private void RotateCameraAndCharacterByGyro()
		{
			var gyro = JoyconInput.instance.GetGyro(0);
			var rotationX = _rotationX.Update(gyro.z * gyroVerticalSensitivity, rotationSmoothing);
			var rotationY = _rotationY.Update(gyro.y * gyroHorizontalSensitivity, rotationSmoothing);
			var clampedY = RestrictVerticalRotation(rotationY);
			_rotationY.current = clampedY;
			var worldUp = arms.InverseTransformDirection(Vector3.up);
			var rotation = arms.rotation *
			               Quaternion.AngleAxis(rotationX, worldUp) *
			               Quaternion.AngleAxis(clampedY, Vector3.left);
			transform.eulerAngles = new Vector3(0f, rotation.eulerAngles.y, 0f);
			arms.rotation = rotation;
		}

		private float RestrictVerticalRotation(float mouseY)
		{
			var currentAngle = NormalizeAngle(arms.eulerAngles.x);
			var minY = minVerticalAngle + currentAngle;
			var maxY = maxVerticalAngle + currentAngle;
			return Mathf.Clamp(mouseY, minY + 0.01f, maxY - 0.01f);
		}

		private static float NormalizeAngle(float angleDegrees)
		{
			while (angleDegrees > 180f) angleDegrees -= 360f;
			while (angleDegrees <= -180f) angleDegrees += 360f;
			return angleDegrees;
		}

		private void MoveCharacter()
		{
			var stick = JoyconInput.instance.GetStick(0);
			var direction = new Vector3(stick[0], 0f, stick[1]).normalized;
			var worldDirection = transform.TransformDirection(direction);
			var velocity = worldDirection * (JoyconInput.instance.GetButton(inputSetting.runButtonDesc) ? runningSpeed : walkingSpeed);
			//Checks for collisions so that the character does not stuck when jumping against walls.
			var intersectsWall = CheckCollisionsWithWalls(velocity);
			if (intersectsWall)
			{
				_velocityX.current = _velocityZ.current = 0f;
				return;
			}

			var smoothX = _velocityX.Update(velocity.x, movementSmoothness);
			var smoothZ = _velocityZ.Update(velocity.z, movementSmoothness);
			var rigidbodyVelocity = _rigidbody.velocity;
			var force = new Vector3(smoothX - rigidbodyVelocity.x, 0f, smoothZ - rigidbodyVelocity.z);
			_rigidbody.AddForce(force, ForceMode.VelocityChange);
		}

		private bool CheckCollisionsWithWalls(Vector3 velocity)
		{
			if (_isGrounded) return false;
			var bounds = _collider.bounds;
			var radius = _collider.radius;
			var halfHeight = _collider.height * 0.5f - radius * 1.0f;
			var point1 = bounds.center;
			point1.y += halfHeight;
			var point2 = bounds.center;
			point2.y -= halfHeight;
			Physics.CapsuleCastNonAlloc(point1, point2, radius, velocity.normalized, _wallCastResults,
				radius * 0.04f, ~0, QueryTriggerInteraction.Ignore);
			var collides = _wallCastResults.Any(hit => hit.collider != null && hit.collider != _collider);
			if (!collides) return false;
			for (var i = 0; i < _wallCastResults.Length; i++)
				_wallCastResults[i] = new RaycastHit();
			return true;
		}

		private void Jump()
		{
			if (!_isGrounded || !JoyconInput.instance.GetButtonDown(inputSetting.jumpButtonDesc)) return;
			_isGrounded = false;
			_rigidbody.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
		}

		private void PlayFootstepSounds()
		{
			if (_isGrounded && _rigidbody.velocity.sqrMagnitude > 0.1f)
			{
				_audioSource.clip = JoyconInput.instance.GetButton(inputSetting.runButtonDesc) ? runningSound : walkingSound;
				if (!_audioSource.isPlaying)
					_audioSource.Play();
			}
			else if (_audioSource.isPlaying)
				_audioSource.Pause();
		}

		private class SmoothVelocity
		{
			private float _currentVelocity;

			/// Returns the smoothed velocity.
			public float Update(float target, float smoothTime) =>
				current = Mathf.SmoothDamp(current, target, ref _currentVelocity, smoothTime);

			public float current { get; set; }
		}

		private class SmoothRotation
		{
			private float _currentVelocity;

			/// Returns the smoothed rotation.
			public float Update(float target, float smoothTime) =>
				current = Mathf.SmoothDampAngle(current, target, ref _currentVelocity, smoothTime);

			public float current { get; set; }
		}
	}
}
