using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.PostProcessing;
using UnityEngine.Events;

public class CameraControls : MonoBehaviour {

	// public delegate void CameraAction();	
	public static UnityEvent CameraAtCloseRange;
	public static UnityEvent CameraAtFarRange;

	public float cameraScrollSpeed = 1000.0f;
	public float cameraMoveSpeed = 100.0f;
	public float cameraRotateSpeed = 100.0f;
	public float cameraSmooth = 0.3f;

	public const float CAMERA_MAX_HEIGHT = 50.0f;
	public const float CAMERA_MIN_HEIGHT = 2.0f;
	public const float CAMERA_BEGIN_CLOSE_TILT_DISTANCE = 0.5f;
	public const float CAMERA_CLOSE_TILT = 45.0f;
	public const float CAMERA_FAR_TILT = 80.0f;
	public const float PAN_THICKNESS = 5.0f;

	PostProcessingProfile postProcessingProfile;
	bool postProcessingDOFon = false;
	public float minAperture = 3.0f;
	public float maxAperture = 6.0f;

	// DEBUG SUFF
	public bool panEnabled = false;

	private Vector3 cameraDesiredPosition;
	private Vector3 cameraDesiredRotation;

	// Use this for initialization
	void Start () {
		CameraAtCloseRange = new UnityEvent();
		CameraAtFarRange = new UnityEvent();
		cameraDesiredPosition = transform.position;
		cameraDesiredRotation = transform.rotation.eulerAngles;
		UpdateCameraPitch();
	}

	void OnEnable()
	{
		var behaviour = GetComponent<PostProcessingBehaviour>();

		if (behaviour.profile == null) {
			return;
		}

		postProcessingProfile = behaviour.profile;
	}

	// Update is called once per frame
	void Update () {
		Vector3 cameraMove = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical")) * cameraMoveSpeed * Time.deltaTime;

		if (cameraMove == Vector3.zero) {
			cameraMove = GetPotentialPanCameraMove(Time.deltaTime);
		}

		Vector3 eulerCameraRotation = transform.rotation.eulerAngles;
		// cameraDesiredRotation = eulerCameraRotation;
		Quaternion cameraRotationYOnly = Quaternion.Euler(new Vector3(0, eulerCameraRotation.y, 0));
		cameraMove = cameraRotationYOnly * cameraMove;


		cameraMove.y += (Input.GetAxis("Mouse ScrollWheel") * Time.deltaTime * cameraScrollSpeed);

		if (cameraMove != Vector3.zero || (cameraDesiredPosition - transform.position) != Vector3.zero) {
			cameraDesiredPosition += cameraMove;
			
			if (cameraDesiredPosition.y > CAMERA_MAX_HEIGHT) {
				cameraDesiredPosition.y = CAMERA_MAX_HEIGHT;
			} else if (cameraDesiredPosition.y < CAMERA_MIN_HEIGHT) {
				cameraDesiredPosition.y = CAMERA_MIN_HEIGHT;
			}
			
			transform.position = Vector3.Lerp(transform.position, cameraDesiredPosition, cameraSmooth);
			
			UpdateCameraPitch();
		}

		if (Input.GetAxis("CameraRotate") != 0.0f) {
			cameraDesiredRotation.y += Input.GetAxis("CameraRotate") * Time.deltaTime * cameraRotateSpeed;

			// transform.rotation = Quaternion.Euler(eulerCameraRotation);
		}

		Quaternion cameraDesiredRotationQuaternion = Quaternion.Euler(cameraDesiredRotation);

		if (transform.rotation != cameraDesiredRotationQuaternion) {
			transform.rotation = Quaternion.Lerp(transform.rotation, cameraDesiredRotationQuaternion, cameraSmooth);
		}
	}

	Vector3 GetPotentialPanCameraMove(float deltaTime)
	{
		if (panEnabled) {
			Vector3 cameraMove = Vector3.zero;

			if (Input.mousePosition.x > Screen.width - PAN_THICKNESS) {
				cameraMove.x = cameraMoveSpeed * deltaTime;
			} else if (Input.mousePosition.x < PAN_THICKNESS) {
				cameraMove.x = cameraMoveSpeed * deltaTime * -1;
			}

			if (Input.mousePosition.y > Screen.height - PAN_THICKNESS) {
				cameraMove.z = cameraMoveSpeed * deltaTime;
			} else if (Input.mousePosition.y < PAN_THICKNESS) {
				cameraMove.z = cameraMoveSpeed * deltaTime * -1;
			}

			return cameraMove;
		}
		
		return Vector3.zero;
	}

	void UpdateCameraPitch() {
		Vector3 eulerCameraRotation = cameraDesiredRotation;
		eulerCameraRotation.x = CAMERA_FAR_TILT;

		float cameraRange = CAMERA_MAX_HEIGHT - CAMERA_MIN_HEIGHT;
		float cameraBeginTiltDistance = cameraRange * CAMERA_BEGIN_CLOSE_TILT_DISTANCE;
		float cameraCurrentDistance = transform.position.y - CAMERA_MIN_HEIGHT;

		if (cameraCurrentDistance < cameraBeginTiltDistance) {
			if (CameraAtCloseRange != null) {
				CameraAtCloseRange.Invoke();
			}
			
			float cameraTilt = (cameraCurrentDistance/cameraBeginTiltDistance);

			eulerCameraRotation.x = (cameraTilt * CAMERA_FAR_TILT) + ((1.0f - cameraTilt) * CAMERA_CLOSE_TILT);

			if (!postProcessingDOFon) {
				postProcessingDOFon = true;
				postProcessingProfile.depthOfField.enabled = true;
			}

			DepthOfFieldModel.Settings DOFSettings = postProcessingProfile.depthOfField.settings;
			DOFSettings.aperture = (cameraTilt * maxAperture) + ((1.0f - cameraTilt) * minAperture);

			postProcessingProfile.depthOfField.settings = DOFSettings;
		} else {
			if (CameraAtFarRange != null) {
				CameraAtFarRange.Invoke();
			}
			
			if (postProcessingDOFon) {
				postProcessingDOFon = false;
				postProcessingProfile.depthOfField.enabled = false;
			}
		}

		cameraDesiredRotation = eulerCameraRotation;
	}
}
