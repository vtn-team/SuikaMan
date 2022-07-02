/************************************************************************************
Copyright : Copyright (c) Facebook Technologies, LLC and its affiliates. All rights reserved.

Your use of this SDK or tool is subject to the Oculus SDK License Agreement, available at
https://developer.oculus.com/licenses/oculussdk/

Unless required by applicable law or agreed to in writing, the Utilities SDK distributed
under the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF
ANY KIND, either express or implied. See the License for the specific language governing
permissions and limitations under the License.
************************************************************************************/

using System;
using UnityEngine;

/// <summary>
/// Controls the player's movement in virtual reality.
/// </summary>
[RequireComponent(typeof(CharacterController))]
public class OVRPlayerController : MonoBehaviour
{
	/// <summary>
	/// The rate acceleration during movement.移動加速度
	/// </summary>
	public float Acceleration = 0.1f;

	/// <summary>
	/// The rate of damping on movement.動作時の減衰率
	/// </summary>
	public float Damping = 0.3f;

	/// <summary>
	/// The rate of additional damping when moving sideways or backwards.後方横移動時の負荷減衰率
	/// </summary>
	public float BackAndSideDampen = 0.5f;

	/// <summary>
	/// The force applied to the character when jumping.ジャンプ力
	/// </summary>
	public float JumpForce = 0.3f;

	/// <summary>
	/// The rate of rotation when using a gamepad.ゲームパッドを使用するときの回転速度。
	/// </summary>
	public float RotationAmount = 1.5f;

	/// <summary>
	/// The rate of rotation when using the keyboard.キーボード使用時の回転速度。
	/// </summary>
	public float RotationRatchet = 45.0f;

	/// <summary>
	/// The player will rotate in fixed steps if Snap Rotation is enabled.
	/// </summary>
	[Tooltip("スナップ回転が有効になっている場合、プレーヤーは固定ステップで回転します")]
	public bool SnapRotation = true;

	/// <summary>
	/// [Deprecated] When enabled, snap rotation will happen about the guardian rather
	/// than the player/camera viewpoint.
	/// </summary>
	[Tooltip("[非推奨] 有効な場合、スナップ回転はプレイヤー/カメラの視点ではなく、ガーディアンについて行われます。回転するようになります。")]
	public bool RotateAroundGuardianCenter = false;

	/// <summary>
	/// How many fixed speeds to use with linear movement? 0=linear control
	/// </summary>
	[Tooltip("リニア制御で使用する固定速度はいくつですか？0＝リニア制御")]
	public int FixedSpeedSteps;

	/// <summary>
	/// trueの場合、Hmdポーズが再センターされたときのプレーヤーコントローラの初期ヨーをリセットします。
	/// </summary>
	public bool HmdResetsY = true;

	/// <summary>
	///true の場合、子 OVRCameraRig からのトラッキングデータは、DIR を更新します。
	/// </summary>
	public bool HmdRotatesY = true;

	/// <summary>
	/// 重力の強さを変更します。
	/// </summary>
	public float GravityModifier = 0.379f;

	/// <summary>
	/// true の場合、各 OVRPlayerController はプレーヤーの物理的な高さを使用します。
	/// </summary>
	public bool useProfileData = true;

	/// <summary>
	/// CameraHeight は HMD の実際の高さであり、キャラクタ コントローラの高さに影響します。
	/// これは、天井の低い場所にキャラクターが移動する能力に影響します。
	/// </summary>
	[NonSerialized]
	public float CameraHeight;

	/// <summary>
	/// このイベントは、キャラクターコントローラが移動された後に発生します。これは OVRAvatarLocomotion スクリプトによって使用され、アバター変換の同期を維持します。
	/// OVRPlayerController と同期させるために使用されます。
	/// </summary>
	public event Action<Transform> TransformUpdated;

	/// <summary>
	///このboolは、プレーヤーコントローラーがテレポートされたときにtrueに設定されます。これは毎フレーム後にリセットされます。のようなシステムもあります。
	/// キャラクタカメラ制約（CharacterCameraConstraint）などの一部のシステムでは、キャラクタコントローラをテレポート直後に移動させるロジックを無効にするためにこのブール値をテストする。
	/// テレポートの直後にキャラクタコントローラを移動させるロジックを無効にするために、このブール値をテストする。
	/// </summary>
	[NonSerialized] // This doesn't need to be visible in the inspector.
	public bool Teleported;

	/// <summary>
	/// This event is raised immediately after the camera transform has been updated, but before movement is updated.
	/// </summary>
	public event Action CameraUpdated;

	/// <summary>
	/// This event is raised right before the character controller is actually moved in order to provide other systems the opportunity to
	/// move the character controller in response to things other than user input, such as movement of the HMD. See CharacterCameraConstraint.cs
	/// for an example of this.
	/// </summary>
	public event Action PreCharacterMove;

	/// <summary>
	/// When true, user input will be applied to linear movement. Set this to false whenever the player controller needs to ignore input for
	/// linear movement.
	/// </summary>
	public bool EnableLinearMovement = true;

	/// <summary>
	/// When true, user input will be applied to rotation. Set this to false whenever the player controller needs to ignore input for rotation.
	/// </summary>
	public bool EnableRotation = true;

	/// <summary>
	/// Rotation defaults to secondary thumbstick. You can allow either here. Note that this won't behave well if EnableLinearMovement is true.
	/// </summary>
	public bool RotationEitherThumbstick = false;

	protected CharacterController Controller = null;
	protected OVRCameraRig CameraRig = null;

	private float MoveScale = 1.0f;
	private Vector3 MoveThrottle = Vector3.zero;
	private float FallSpeed = 0.0f;
	private OVRPose? InitialPose;
	public float InitialYRotation { get; private set; }
	private float MoveScaleMultiplier = 1.0f;
	private float RotationScaleMultiplier = 1.0f;
	private bool SkipMouseRotation = true; // It is rare to want to use mouse movement in VR, so ignore the mouse by default.
	private bool HaltUpdateMovement = false;
	private bool prevHatLeft = false;
	private bool prevHatRight = false;
	private float SimulationRate = 60f;
	private float buttonRotation = 0f;
	private bool ReadyToSnapTurn; // Set to true when a snap turn has occurred, code requires one frame of centered thumbstick to enable another snap turn.
	private bool playerControllerEnabled = false;

	void Start()
	{
		// Add eye-depth as a camera offset from the player controller
		var p = CameraRig.transform.localPosition;
		p.z = OVRManager.profile.eyeDepth;
		CameraRig.transform.localPosition = p;
	}

	void Awake()
	{
		Controller = gameObject.GetComponent<CharacterController>();

		if (Controller == null)
			Debug.LogWarning("OVRPlayerController: No CharacterController attached.");

		// We use OVRCameraRig to set rotations to cameras,
		// and to be influenced by rotation
		OVRCameraRig[] CameraRigs = gameObject.GetComponentsInChildren<OVRCameraRig>();

		if (CameraRigs.Length == 0)
			Debug.LogWarning("OVRPlayerController: No OVRCameraRig attached.");
		else if (CameraRigs.Length > 1)
			Debug.LogWarning("OVRPlayerController: More then 1 OVRCameraRig attached.");
		else
			CameraRig = CameraRigs[0];

		InitialYRotation = transform.rotation.eulerAngles.y;
	}

	void OnEnable()
	{
	}

	void OnDisable()
	{
		if (playerControllerEnabled)
		{
			OVRManager.display.RecenteredPose -= ResetOrientation;

			if (CameraRig != null)
			{
				CameraRig.UpdatedAnchors -= UpdateTransform;
			}
			playerControllerEnabled = false;
		}
	}

	void Update()
	{
		if (!playerControllerEnabled)
		{
			if (OVRManager.OVRManagerinitialized)
			{
				OVRManager.display.RecenteredPose += ResetOrientation;

				if (CameraRig != null)
				{
					CameraRig.UpdatedAnchors += UpdateTransform;
				}
				playerControllerEnabled = true;
			}
			else
				return;
		}

		//todo: enable for Unity Input System
#if ENABLE_LEGACY_INPUT_MANAGER

		//Use keys to ratchet rotation
		if (Input.GetKeyDown(KeyCode.Q))
			buttonRotation -= RotationRatchet;

		if (Input.GetKeyDown(KeyCode.E))
			buttonRotation += RotationRatchet;
#endif
	}

	protected virtual void UpdateController()
	{
		if (useProfileData)
		{
			if (InitialPose == null)
			{
				// Save the initial pose so it can be recovered if useProfileData
				// is turned off later.
				InitialPose = new OVRPose()
				{
					position = CameraRig.transform.localPosition,
					orientation = CameraRig.transform.localRotation
				};
			}

			var p = CameraRig.transform.localPosition;
			if (OVRManager.instance.trackingOriginType == OVRManager.TrackingOrigin.EyeLevel)
			{
				p.y = OVRManager.profile.eyeHeight - (0.5f * Controller.height) + Controller.center.y;
			}
			else if (OVRManager.instance.trackingOriginType == OVRManager.TrackingOrigin.FloorLevel)
			{
				p.y = -(0.5f * Controller.height) + Controller.center.y;
			}
			CameraRig.transform.localPosition = p;
		}
		else if (InitialPose != null)
		{
			// Return to the initial pose if useProfileData was turned off at runtime
			CameraRig.transform.localPosition = InitialPose.Value.position;
			CameraRig.transform.localRotation = InitialPose.Value.orientation;
			InitialPose = null;
		}

		CameraHeight = CameraRig.centerEyeAnchor.localPosition.y;

		if (CameraUpdated != null)
		{
			CameraUpdated();
		}

		UpdateMovement();

		Vector3 moveDirection = Vector3.zero;

		float motorDamp = (1.0f + (Damping * SimulationRate * Time.deltaTime));

		MoveThrottle.x /= motorDamp;
		MoveThrottle.y = (MoveThrottle.y > 0.0f) ? (MoveThrottle.y / motorDamp) : MoveThrottle.y;
		MoveThrottle.z /= motorDamp;

		moveDirection += MoveThrottle * SimulationRate * Time.deltaTime;

		// Gravity
		if (Controller.isGrounded && FallSpeed <= 0)
			FallSpeed = ((Physics.gravity.y * (GravityModifier * 0.002f)));
		else
			FallSpeed += ((Physics.gravity.y * (GravityModifier * 0.002f)) * SimulationRate * Time.deltaTime);

		moveDirection.y += FallSpeed * SimulationRate * Time.deltaTime;


		if (Controller.isGrounded && MoveThrottle.y <= transform.lossyScale.y * 0.001f)
		{
			// Offset correction for uneven ground
			float bumpUpOffset = Mathf.Max(Controller.stepOffset, new Vector3(moveDirection.x, 0, moveDirection.z).magnitude);
			moveDirection -= bumpUpOffset * Vector3.up;
		}

		if (PreCharacterMove != null)
		{
			PreCharacterMove();
			Teleported = false;
		}

		Vector3 predictedXZ = Vector3.Scale((Controller.transform.localPosition + moveDirection), new Vector3(1, 0, 1));

		// Move contoller
		Controller.Move(moveDirection);
		Vector3 actualXZ = Vector3.Scale(Controller.transform.localPosition, new Vector3(1, 0, 1));

		if (predictedXZ != actualXZ)
			MoveThrottle += (actualXZ - predictedXZ) / (SimulationRate * Time.deltaTime);
	}





	public virtual void UpdateMovement()
	{
		//todo: enable for Unity Input System
#if ENABLE_LEGACY_INPUT_MANAGER
		if (HaltUpdateMovement)
			return;

		if (EnableLinearMovement)
		{
			bool moveForward = Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow);
			bool moveLeft = Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow);
			bool moveRight = Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow);
			bool moveBack = Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow);

			bool dpad_move = false;

			if (OVRInput.Get(OVRInput.Button.DpadUp))
			{
				moveForward = true;
				dpad_move = true;

			}

			if (OVRInput.Get(OVRInput.Button.DpadDown))
			{
				moveBack = true;
				dpad_move = true;
			}

			MoveScale = 1.0f;

			if ((moveForward && moveLeft) || (moveForward && moveRight) ||
				(moveBack && moveLeft) || (moveBack && moveRight))
				MoveScale = 0.70710678f;

			// No positional movement if we are in the air
			if (!Controller.isGrounded)
				MoveScale = 0.0f;

			MoveScale *= SimulationRate * Time.deltaTime;

			// Compute this for key movement
			float moveInfluence = Acceleration * 0.1f * MoveScale * MoveScaleMultiplier;

			// Run!
			if (dpad_move || Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
				moveInfluence *= 2.0f;

			Quaternion ort = transform.rotation;
			Vector3 ortEuler = ort.eulerAngles;
			ortEuler.z = ortEuler.x = 0f;
			ort = Quaternion.Euler(ortEuler);

			if (moveForward)
				MoveThrottle += ort * (transform.lossyScale.z * moveInfluence * Vector3.forward);
			if (moveBack)
				MoveThrottle += ort * (transform.lossyScale.z * moveInfluence * BackAndSideDampen * Vector3.back);
			if (moveLeft)
				MoveThrottle += ort * (transform.lossyScale.x * moveInfluence * BackAndSideDampen * Vector3.left);
			if (moveRight)
				MoveThrottle += ort * (transform.lossyScale.x * moveInfluence * BackAndSideDampen * Vector3.right);



			moveInfluence = Acceleration * 0.1f * MoveScale * MoveScaleMultiplier;

#if !UNITY_ANDROID // LeftTrigger not avail on Android game pad
			moveInfluence *= 1.0f + OVRInput.Get(OVRInput.Axis1D.PrimaryIndexTrigger);
#endif

			Vector2 primaryAxis = OVRInput.Get(OVRInput.Axis2D.PrimaryThumbstick);

			// If speed quantization is enabled, adjust the input to the number of fixed speed steps.
			if (FixedSpeedSteps > 0)
			{
				primaryAxis.y = Mathf.Round(primaryAxis.y * FixedSpeedSteps) / FixedSpeedSteps;
				primaryAxis.x = Mathf.Round(primaryAxis.x * FixedSpeedSteps) / FixedSpeedSteps;
			}

			if (primaryAxis.y > 0.0f)
				MoveThrottle += ort * (primaryAxis.y * transform.lossyScale.z * moveInfluence * Vector3.forward);

			if (primaryAxis.y < 0.0f)
				MoveThrottle += ort * (Mathf.Abs(primaryAxis.y) * transform.lossyScale.z * moveInfluence *
									   BackAndSideDampen * Vector3.back);

			if (primaryAxis.x < 0.0f)
				MoveThrottle += ort * (Mathf.Abs(primaryAxis.x) * transform.lossyScale.x * moveInfluence *
									   BackAndSideDampen * Vector3.left);

			if (primaryAxis.x > 0.0f)
				MoveThrottle += ort * (primaryAxis.x * transform.lossyScale.x * moveInfluence * BackAndSideDampen *
									   Vector3.right);
		}

		if (EnableRotation)
		{
			Vector3 euler = RotateAroundGuardianCenter ? transform.rotation.eulerAngles : Vector3.zero;
			float rotateInfluence = SimulationRate * Time.deltaTime * RotationAmount * RotationScaleMultiplier;

			bool curHatLeft = OVRInput.Get(OVRInput.Button.PrimaryShoulder);

			if (curHatLeft && !prevHatLeft)
				euler.y -= RotationRatchet;

			prevHatLeft = curHatLeft;

			bool curHatRight = OVRInput.Get(OVRInput.Button.SecondaryShoulder);

			if (curHatRight && !prevHatRight)
				euler.y += RotationRatchet;

			prevHatRight = curHatRight;

			euler.y += buttonRotation;
			buttonRotation = 0f;


#if !UNITY_ANDROID || UNITY_EDITOR
			if (!SkipMouseRotation)
				euler.y += Input.GetAxis("Mouse X") * rotateInfluence * 3.25f;
#endif

			if (SnapRotation)
			{
				if (OVRInput.Get(OVRInput.Button.SecondaryThumbstickLeft) ||
					(RotationEitherThumbstick && OVRInput.Get(OVRInput.Button.PrimaryThumbstickLeft)))
				{
					if (ReadyToSnapTurn)
					{
						euler.y -= RotationRatchet;
						ReadyToSnapTurn = false;
					}
				}
				else if (OVRInput.Get(OVRInput.Button.SecondaryThumbstickRight) ||
					(RotationEitherThumbstick && OVRInput.Get(OVRInput.Button.PrimaryThumbstickRight)))
				{
					if (ReadyToSnapTurn)
					{
						euler.y += RotationRatchet;
						ReadyToSnapTurn = false;
					}
				}
				else
				{
					ReadyToSnapTurn = true;
				}
			}
			else
			{
				Vector2 secondaryAxis = OVRInput.Get(OVRInput.Axis2D.SecondaryThumbstick);
				if (RotationEitherThumbstick)
				{
					Vector2 altSecondaryAxis = OVRInput.Get(OVRInput.Axis2D.PrimaryThumbstick);
					if (secondaryAxis.sqrMagnitude < altSecondaryAxis.sqrMagnitude)
					{
						secondaryAxis = altSecondaryAxis;
					}
				}
				euler.y += secondaryAxis.x * rotateInfluence;
			}

			if (RotateAroundGuardianCenter)
			{
				transform.rotation = Quaternion.Euler(euler);
			}
			else
			{
				transform.RotateAround(CameraRig.centerEyeAnchor.position, Vector3.up, euler.y);
			}
		}
#endif
	}


	/// <summary>
	/// Invoked by OVRCameraRig's UpdatedAnchors callback. Allows the Hmd rotation to update the facing direction of the player.
	/// </summary>
	public void UpdateTransform(OVRCameraRig rig)
	{
		Transform root = CameraRig.trackingSpace;
		Transform centerEye = CameraRig.centerEyeAnchor;

		if (HmdRotatesY && !Teleported)
		{
			Vector3 prevPos = root.position;
			Quaternion prevRot = root.rotation;

			transform.rotation = Quaternion.Euler(0.0f, centerEye.rotation.eulerAngles.y, 0.0f);

			root.position = prevPos;
			root.rotation = prevRot;
		}

		UpdateController();
		if (TransformUpdated != null)
		{
			TransformUpdated(root);
		}
	}

	/// <summary>
	/// Jump! Must be enabled manually.
	/// </summary>
	public bool Jump()
	{
		if (!Controller.isGrounded)
			return false;

		MoveThrottle += new Vector3(0, transform.lossyScale.y * JumpForce, 0);

		return true;
	}

	/// <summary>
	/// Stop this instance.
	/// </summary>
	public void Stop()
	{
		Controller.Move(Vector3.zero);
		MoveThrottle = Vector3.zero;
		FallSpeed = 0.0f;
	}

	/// <summary>
	/// Gets the move scale multiplier.
	/// </summary>
	/// <param name="moveScaleMultiplier">Move scale multiplier.</param>
	public void GetMoveScaleMultiplier(ref float moveScaleMultiplier)
	{
		moveScaleMultiplier = MoveScaleMultiplier;
	}

	/// <summary>
	/// Sets the move scale multiplier.
	/// </summary>
	/// <param name="moveScaleMultiplier">Move scale multiplier.</param>
	public void SetMoveScaleMultiplier(float moveScaleMultiplier)
	{
		MoveScaleMultiplier = moveScaleMultiplier;
	}

	/// <summary>
	/// Gets the rotation scale multiplier.
	/// </summary>
	/// <param name="rotationScaleMultiplier">Rotation scale multiplier.</param>
	public void GetRotationScaleMultiplier(ref float rotationScaleMultiplier)
	{
		rotationScaleMultiplier = RotationScaleMultiplier;
	}

	/// <summary>
	/// Sets the rotation scale multiplier.
	/// </summary>
	/// <param name="rotationScaleMultiplier">Rotation scale multiplier.</param>
	public void SetRotationScaleMultiplier(float rotationScaleMultiplier)
	{
		RotationScaleMultiplier = rotationScaleMultiplier;
	}

	/// <summary>
	/// Gets the allow mouse rotation.
	/// </summary>
	/// <param name="skipMouseRotation">Allow mouse rotation.</param>
	public void GetSkipMouseRotation(ref bool skipMouseRotation)
	{
		skipMouseRotation = SkipMouseRotation;
	}

	/// <summary>
	/// Sets the allow mouse rotation.
	/// </summary>
	/// <param name="skipMouseRotation">If set to <c>true</c> allow mouse rotation.</param>
	public void SetSkipMouseRotation(bool skipMouseRotation)
	{
		SkipMouseRotation = skipMouseRotation;
	}

	/// <summary>
	/// Gets the halt update movement.
	/// </summary>
	/// <param name="haltUpdateMovement">Halt update movement.</param>
	public void GetHaltUpdateMovement(ref bool haltUpdateMovement)
	{
		haltUpdateMovement = HaltUpdateMovement;
	}

	/// <summary>
	/// Sets the halt update movement.
	/// </summary>
	/// <param name="haltUpdateMovement">If set to <c>true</c> halt update movement.</param>
	public void SetHaltUpdateMovement(bool haltUpdateMovement)
	{
		HaltUpdateMovement = haltUpdateMovement;
	}

	/// <summary>
	/// Resets the player look rotation when the device orientation is reset.
	/// </summary>
	public void ResetOrientation()
	{
		if (HmdResetsY && !HmdRotatesY)
		{
			Vector3 euler = transform.rotation.eulerAngles;
			euler.y = InitialYRotation;
			transform.rotation = Quaternion.Euler(euler);
		}
	}
}
