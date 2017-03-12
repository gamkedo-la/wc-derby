using UnityEngine;
using System.Collections;

public class robotAI : MonoBehaviour {
	public GameObject nearestGoldBox = null;
	public GameObject tractorBeam;
	public Transform headToLook;
	public float robotWalkSpeed = 12.0f;
	public float tractorTimeToCaptureBlock = 3.0f;
	// Use this for initialization

	ParticleSystem tractorEffect;
	Vector3 tractorStartPos;
	Vector3 tractorStartScale;
	float tractorProgressPerc;
	ParticleSystem.MinMaxCurve effectSpawnRate;
	bool isTractorBeamActive = false;
	void Start () {
		StartCoroutine (PickNearestGoldBlock ());
		tractorEffect = tractorBeam.GetComponentInChildren<ParticleSystem> ();
		tractorEffect.Stop ();
	}

	IEnumerator PickNearestGoldBlock(){
		while (true){
			if (isTractorBeamActive == false || nearestGoldBox == null) {
				nearestGoldBox = GoldGoalTracker.NearestTargetToPoint (gameObject.transform.position);	
				if (nearestGoldBox == null && isTractorBeamActive) {
					tractorEffect.Stop ();
					isTractorBeamActive = false;
				}
			}
			yield return new WaitForSeconds (0.3f);
		}
	}

	void StartTractorBeam(){
		tractorEffect.Play ();
		Rigidbody blockRB = nearestGoldBox.GetComponent<Rigidbody> ();
		blockRB.useGravity = false;
		tractorStartPos = nearestGoldBox.transform.position;
		tractorStartScale = nearestGoldBox.transform.localScale;
		tractorProgressPerc = 0.0f;
		isTractorBeamActive = true;
		SoundSet.PlayClipByName("TractorBeam", 0.6f);
	}

	void CaptureBlock(GameObject goldBlock){
		GoldGoalTracker.AddEnemyGold (nearestGoldBox);
		Destroy (goldBlock);
//		Debug.Log ("Destroyed gold box");
	}

	void FixedUpdate() {
		if(nearestGoldBox != null) {
			Vector3 oppositePos = nearestGoldBox.transform.position - headToLook.position;

			Vector3 nextStare = (headToLook.position - oppositePos - Vector3.up * 100.0f);
			headToLook.rotation =
				Quaternion.Slerp(headToLook.rotation,
				Quaternion.LookRotation(nextStare - headToLook.position), Time.deltaTime);
		}
	}

	// Update is called once per frame
	void Update () {
		if (nearestGoldBox != null) {
			Vector3 eyeLevelTarget = nearestGoldBox.transform.position;
			eyeLevelTarget.y = transform.position.y;

			if (Input.GetKeyDown (KeyCode.O)) {
				CaptureBlock (nearestGoldBox);
				return;
			}
			float distToTarget = Vector3.Distance (transform.position, eyeLevelTarget);
//			Debug.Log (Mathf.FloorToInt (distToTarget));
			if (distToTarget < 90.0f) {

				if(isTractorBeamActive == false){
					StartTractorBeam ();
				}
				tractorProgressPerc += Time.deltaTime / tractorTimeToCaptureBlock;
				if (tractorProgressPerc > 0.85f) {
					CaptureBlock (nearestGoldBox);
				} else {
					nearestGoldBox.transform.position = 
						tractorProgressPerc * tractorBeam.transform.position +
						(1.0f - tractorProgressPerc) * tractorStartPos;
					nearestGoldBox.transform.localScale = tractorStartScale * (1.0f - tractorProgressPerc);
					tractorBeam.transform.LookAt (nearestGoldBox.transform.position);
				}

			} else {
				if (isTractorBeamActive) {
					tractorEffect.Stop ();
					isTractorBeamActive = false;
				}
				
				transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(eyeLevelTarget - transform.position), Time.deltaTime);
				transform.position += transform.forward * robotWalkSpeed * Time.deltaTime;
				// #TODO do we ever release a block?  e.g. robot dies
			} // end of destroyed button pressed
		} // end of nearestGoldBox != null test
	} // end update
}
