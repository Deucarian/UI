using UnityEngine;

namespace Deucarian.UI
{
    /// <summary>Deucarian-owned lazy UI follow. Assign a target explicitly; no XR package is required.</summary>
    [DisallowMultipleComponent]
    [AddComponentMenu("Deucarian/UI/Lazy Follow")]
    public sealed class DeucarianLazyFollow : MonoBehaviour
    {
        [SerializeField, Tooltip("The anchor to follow. No camera or singleton is discovered automatically.")]
        private Transform target;
        [SerializeField] private Vector3 localPositionOffset;
        [SerializeField] private Vector3 localRotationOffset;
        [SerializeField] private bool snapOnEnable = true;
        [SerializeField] private bool useUnscaledTime = true;
        [SerializeField] private DeucarianLazyFollowSettings settings = DeucarianLazyFollowSettings.Default;
        private readonly DeucarianLazyFollowState motion = new DeucarianLazyFollowState();

        public Transform Target
        {
            get => target;
            set { target = value; motion.Reset(new Pose(transform.position, transform.rotation)); }
        }
        public DeucarianLazyFollowSettings Settings { get => settings; set => settings = value.Sanitized(); }

        public void Configure(Transform anchor, Vector3 positionOffset, Vector3 rotationOffset,
            DeucarianLazyFollowSettings followSettings, bool recenter = true)
        {
            Target = anchor;
            localPositionOffset = positionOffset;
            localRotationOffset = rotationOffset;
            Settings = followSettings;
            if (recenter) Recenter();
        }

        public void Recenter()
        {
            if (target == null || target == transform || target.IsChildOf(transform)) return;
            var pose = DesiredPose();
            motion.Reset(pose);
            transform.SetPositionAndRotation(pose.position, pose.rotation);
        }

        private void OnEnable()
        {
            motion.Reset(new Pose(transform.position, transform.rotation));
            if (snapOnEnable) Recenter();
        }

        private void LateUpdate()
        {
            if (target == null || target == transform || target.IsChildOf(transform)) return;
            var pose = motion.Advance(DesiredPose(), settings, useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime);
            transform.SetPositionAndRotation(pose.position, pose.rotation);
        }

        private Pose DesiredPose() => new Pose(target.TransformPoint(localPositionOffset),
            target.rotation * Quaternion.Euler(localRotationOffset));
        private void OnValidate() => settings = settings.Sanitized();
    }
}
