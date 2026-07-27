using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Windowing.GraphicsLibraryFramework;
using OpenTK.Mathematics;
using CorrinoEngine.Maps;

namespace CorrinoEngine.Cameras
{
    public class RTSCameraController : CameraController
    {
        private const float CameraMoveSpeed = 320f;
        private const float EdgeScrollThreshold = 18f;
        private const float ZoomStep = 96f;
        private const float MinCameraHeight = 72f;
        private const float MaxCameraHeight = 640f;

        private Vector2 mapMin = new Vector2(-256f, -256f);
        private Vector2 mapMax = new Vector2(256f, 256f);
        private float lastScrollY;
        private bool isScrollInitialized;

        public RTSCameraController(Camera camera) : base(camera)
        {
        }

        public void SetMapBounds(Vector2 min, Vector2 max)
        {
            mapMin = min;
            mapMax = max;
            ClampToBounds();
        }

        public override void Update(float deltaTime, MapHeightField mapHeightField)
        {
            Vector3 focusPoint = GetGroundFocusPoint();

            // 1. 防御性处理：防止因镜头剧烈移动导致 focusPoint 越界，从而引发采样崩溃
            float safeWorldX = MathHelper.Clamp(focusPoint.X, mapMin.X, mapMax.X);
            float safeWorldZ = MathHelper.Clamp(focusPoint.Z, mapMin.Y, mapMax.Y);

            // 2. 映射到 0~1 的 UV 空间
            float x01 = (focusPoint.X - mapMin.X) / (mapMax.X - mapMin.X);
            float y01 = (focusPoint.Z - mapMin.Y) / (mapMax.Y - mapMin.Y);

            // 3. 非空保护并动态更新参考高度
            if (mapHeightField != null)
            {
                ReferenceGroundHeight = mapHeightField.SampleHeight01(x01, y01);
            }

            HandleMovement(deltaTime);
            HandleZoom();
            ClampToBounds();

            base.Update(deltaTime, mapHeightField);
        }

        private void HandleMovement(float deltaTime)
        {
            if (ks == null || ms == null)
                return;

            Vector3 forward = new Vector3(camera.Direction.X, 0, camera.Direction.Z);
            if (forward.LengthSquared < float.Epsilon)
                forward = Vector3.UnitZ;
            else
                forward.Normalize();

            Vector3 right = new Vector3(forward.Z, 0, -forward.X);
            Vector3 moveDirection = Vector3.Zero;

            if (ks.IsKeyDown(Keys.W) || ks.IsKeyDown(Keys.Up))
                moveDirection += forward;
            if (ks.IsKeyDown(Keys.S) || ks.IsKeyDown(Keys.Down))
                moveDirection -= forward;
            if (ks.IsKeyDown(Keys.D) || ks.IsKeyDown(Keys.Right))
                moveDirection -= right;
            if (ks.IsKeyDown(Keys.A) || ks.IsKeyDown(Keys.Left))
                moveDirection += right;

            if (camera.Size.X > 0 && camera.Size.Y > 0)
            {
                if (ms.X <= EdgeScrollThreshold)
                    moveDirection += right;
                else if (ms.X >= camera.Size.X - EdgeScrollThreshold)
                    moveDirection -= right;

                if (ms.Y <= EdgeScrollThreshold)
                    moveDirection += forward;
                else if (ms.Y >= camera.Size.Y - EdgeScrollThreshold)
                    moveDirection -= forward;
            }

            if (moveDirection.LengthSquared > float.Epsilon)
            {
                moveDirection.Normalize();
                camera.Position += moveDirection * CameraMoveSpeed * deltaTime;
            }
        }

        private void HandleZoom()
        {
            if (ms == null)
                return;

            if (!isScrollInitialized)
            {
                lastScrollY = ms.Scroll.Y;
                isScrollInitialized = true;
                return;
            }

            float scrollDelta = ms.Scroll.Y - lastScrollY;
            lastScrollY = ms.Scroll.Y;
            if (MathF.Abs(scrollDelta) < float.Epsilon)
                return;

            float desiredHeight = MathHelper.Clamp(camera.Position.Y - scrollDelta * ZoomStep, MinCameraHeight, MaxCameraHeight);
            SetCameraHeight(desiredHeight);
        }

        private void SetCameraHeight(float height)
        {
            if (MathF.Abs(camera.Direction.Y) < float.Epsilon)
                return;

            float deltaAlongDirection = (height - camera.Position.Y) / camera.Direction.Y;
            camera.Position += camera.Direction * deltaAlongDirection;
        }

        private void ClampToBounds()
        {
            Vector3 focusPoint = GetGroundFocusPoint();
            float clampedX = MathHelper.Clamp(focusPoint.X, mapMin.X, mapMax.X);
            float clampedZ = MathHelper.Clamp(focusPoint.Z, mapMin.Y, mapMax.Y);
            if (MathF.Abs(clampedX - focusPoint.X) < float.Epsilon && MathF.Abs(clampedZ - focusPoint.Z) < float.Epsilon)
                return;

            // 传入修正后的 X 和 Z，Y 轴保持 ReferenceGroundHeight
            SetGroundFocusPoint(new Vector3(clampedX, ReferenceGroundHeight, clampedZ));
        }
        private Vector3 GetGroundFocusPoint()
        {
            if (MathF.Abs(camera.Direction.Y) < float.Epsilon)
                return camera.Position;

            // 核心修复：与真实的参考高度平面相交，而不是强行和 Y=0 相交
            float distance = (ReferenceGroundHeight - camera.Position.Y) / camera.Direction.Y;
            return camera.Position + camera.Direction * distance;
        }

        private void SetGroundFocusPoint(Vector3 focusPoint)
        {
            if (MathF.Abs(camera.Direction.Y) < float.Epsilon)
                return;

            // 核心修复：基于参考高度反推摄像机位置
            float heightDifference = camera.Position.Y - ReferenceGroundHeight;
            float distance = heightDifference / -camera.Direction.Y;

            // 确保 focusPoint 的 Y 轴被固定在参考高度上，防止产生额外的斜边误差
            Vector3 correctedFocusPoint = new Vector3(focusPoint.X, ReferenceGroundHeight, focusPoint.Z);
            camera.Position = correctedFocusPoint - camera.Direction * distance;
        }
    }
}
