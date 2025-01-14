using UnityEngine;
using System.Collections;

// ----- Low Poly FPS Pack Free Version -----
public class GrenadeScript : MonoBehaviour {

	[Header("Timer")]
	//Time before the grenade explodes
	[Tooltip("Time before the grenade explodes")]
	public float grenadeTimer = 5.0f;

	[Header("Explosion Prefabs")]
	//Explosion prefab
	public Transform explosionPrefab;

	[Header("Explosion Options")]
	//Radius of the explosion
	[Tooltip("The radius of the explosion force")]
	public float radius = 25.0F;
	//Intensity of the explosion
	[Tooltip("The intensity of the explosion force")]
	public float power = 350.0F;

	[Header("Throw Force")]
	[Tooltip("Minimum throw force")]
	public float minimumForce = 1500.0f;
	[Tooltip("Maximum throw force")]
	public float maximumForce = 2500.0f;
	private float throwForce;

	[Header("Audio")]
	public AudioSource impactSound;

	private void Awake () 
	{
		GetComponent<Rigidbody>().AddRelativeTorque 
		   (Random.Range(500, 1500), //X Axis
			Random.Range(0,0), 		 //Y Axis
			Random.Range(0,0)  		 //Z Axis
			* Time.deltaTime * 5000);
	}

	public void Throw(float force)
	{
		throwForce = Mathf.Lerp(minimumForce, maximumForce, force / 2);
		GetComponent<Rigidbody>().AddForce(gameObject.transform.forward * throwForce);
		StartCoroutine (ExplosionTimer ());
	}

	private void OnCollisionEnter (Collision collision) => impactSound.Play ();

	private IEnumerator ExplosionTimer () 
	{
		yield return new WaitForSeconds(grenadeTimer);
		if (Physics.Raycast(transform.position, Vector3.down, out var checkGround, 50))
		{
			//Instantiate metal explosion prefab on ground
			Instantiate (explosionPrefab, checkGround.point, 
				Quaternion.FromToRotation (Vector3.forward, checkGround.normal)); 
		}

		Vector3 explosionPos = transform.position;
		Collider[] colliders = Physics.OverlapSphere(explosionPos, radius);
		foreach (Collider hit in colliders) {
			Rigidbody rb = hit.GetComponent<Rigidbody> ();

			if (rb)
				rb.AddExplosionForce (power * 5, explosionPos, radius, 3.0F);
			
			if (hit.GetComponent<Collider>().CompareTag("Target")
			    	&& hit.gameObject.GetComponent<TargetScript>().isHit == false) 
			{
				hit.gameObject.GetComponent<Animation> ().Play("target_down");
				hit.gameObject.GetComponent<TargetScript>().isHit = true;
			}

			if (hit.GetComponent<Collider>().CompareTag("ExplosiveBarrel"))
				hit.gameObject.GetComponent<ExplosiveBarrelScript> ().explode = true;
		}

		Destroy (gameObject);
	}
}
