/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;
using FluentAssertions;
using OneScript.DebugProtocol.TcpServer;
using OneScript.DebugServices;
using Xunit;

namespace OneScript.DebugProtocol.Test
{
    /// <summary>
    /// Tests for debug adapter reconnection functionality
    /// </summary>
    public class ReconnectionTest
    {
        [Fact]
        public void DefaultDebugService_ShouldNotStopOnDisconnectWithoutTerminate()
        {
            // Arrange
            var breakpointManager = new DefaultBreakpointManager();
            var threadManager = new ThreadManager();
            var visualizer = new DefaultVariableVisualizer();
            var service = new DefaultDebugService(breakpointManager, threadManager, visualizer);

            // Set up some breakpoints to verify cleanup
            breakpointManager.SetBreakpoints("/test/file.os", new[] { (1, ""), (2, "") });
            
            // Verify breakpoints were set
            var hasBreakpointBefore = breakpointManager.FindBreakpoint("/test/file.os", 1);
            hasBreakpointBefore.Should().BeTrue("breakpoint should exist before disconnect");

            // Act - Disconnect without termination
            Action act = () => service.Disconnect(terminate: false);

            // Assert - Should not throw StopServiceException
            act.Should().NotThrow<StopServiceException>("disconnect without terminate should not throw");
            
            // Verify cleanup happened - breakpoints should be cleared
            var hasBreakpointAfter = breakpointManager.FindBreakpoint("/test/file.os", 1);
            hasBreakpointAfter.Should().BeFalse("breakpoints should be cleared on disconnect");
        }

        [Fact]
        public void DefaultDebugService_ShouldStopOnDisconnectWithTerminate()
        {
            // Arrange
            var breakpointManager = new DefaultBreakpointManager();
            var threadManager = new ThreadManager();
            var visualizer = new DefaultVariableVisualizer();
            var service = new DefaultDebugService(breakpointManager, threadManager, visualizer);

            // Act & Assert - Disconnect with termination should throw StopServiceException
            Action act = () => service.Disconnect(terminate: true);
            act.Should().Throw<StopServiceException>("disconnect with terminate should throw to stop the server");
        }
        
        [Fact]
        public void DefaultDebugService_ShouldClearBreakpointsOnDisconnect()
        {
            // Arrange
            var breakpointManager = new DefaultBreakpointManager();
            var threadManager = new ThreadManager();
            var visualizer = new DefaultVariableVisualizer();
            var service = new DefaultDebugService(breakpointManager, threadManager, visualizer);
            
            // Set multiple breakpoints in different files
            breakpointManager.SetBreakpoints("/path/to/file1.os", new[] { (1, "x > 0"), (5, "") });
            breakpointManager.SetBreakpoints("/path/to/file2.os", new[] { (10, ""), (20, "y < 100") });
            
            // Verify breakpoints exist
            breakpointManager.FindBreakpoint("/path/to/file1.os", 1).Should().BeTrue();
            breakpointManager.FindBreakpoint("/path/to/file2.os", 10).Should().BeTrue();

            // Act - Disconnect without terminate
            service.Disconnect(terminate: false);

            // Assert - All breakpoints should be cleared
            breakpointManager.FindBreakpoint("/path/to/file1.os", 1).Should().BeFalse();
            breakpointManager.FindBreakpoint("/path/to/file1.os", 5).Should().BeFalse();
            breakpointManager.FindBreakpoint("/path/to/file2.os", 10).Should().BeFalse();
            breakpointManager.FindBreakpoint("/path/to/file2.os", 20).Should().BeFalse();
        }
    }
}
