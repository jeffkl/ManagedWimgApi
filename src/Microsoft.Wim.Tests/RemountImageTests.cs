// Copyright (c). All rights reserved.
//
// Licensed under the MIT license.

using Shouldly;
using System;
using System.Diagnostics;
using System.IO;
using Xunit;

namespace Microsoft.Wim.Tests
{
    public class RemountImageTests : TestBase
    {
        private const string WimServProcessName = "wimserv";

        public RemountImageTests(TestWimTemplate template)
            : base(template)
        {
        }

        [Fact]
        public void RemountImageTest()
        {
            using (WimHandle imageHandle = WimgApi.LoadImage(TestWimHandle, 1))
            {
                WimgApi.MountImage(imageHandle, MountPath, WimMountImageOptions.Fast | WimMountImageOptions.DisableDirectoryAcl | WimMountImageOptions.DisableFileAcl | WimMountImageOptions.DisableRPFix | WimMountImageOptions.ReadOnly);

                try
                {
                    VerifyMountState(imageHandle, WimMountPointState.Mounted);

                    KillWimServ();

                    VerifyMountState(imageHandle, WimMountPointState.Remountable);

                    WimgApi.RemountImage(MountPath);

                    VerifyMountState(imageHandle, WimMountPointState.Mounted);
                }
                finally
                {
                    WimgApi.UnmountImage(imageHandle);
                }
            }
        }

        [Fact]
        public void RemountImageTest_ThrowsArgumentNullException_mountPath()
        {
            ShouldThrow<ArgumentNullException>("mountPath", () =>
                WimgApi.RemountImage(mountPath: null!));
        }

        [Fact]
        public void RemountImageTest_ThrowsDirectoryNotFoundException_mountPath()
        {
            Should.Throw<DirectoryNotFoundException>(() =>
                WimgApi.RemountImage(Path.Combine(TestDirectory, Guid.NewGuid().ToString())));
        }

        private void KillWimServ()
        {
            Process[] wimServProcesses = Process.GetProcessesByName(WimServProcessName);

            wimServProcesses.Length.ShouldNotBe(0);

            foreach (Process process in wimServProcesses)
            {
                process.Kill();
            }
        }

        private void VerifyMountState(WimHandle imageHandle, WimMountPointState expectedMountPointState)
        {
            WimMountInfo mountedImageInfo = WimgApi.GetMountedImageInfoFromHandle(imageHandle);

            (mountedImageInfo.State | expectedMountPointState).ShouldBe(expectedMountPointState);
        }
    }
}