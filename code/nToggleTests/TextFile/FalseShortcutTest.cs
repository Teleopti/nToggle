﻿using nToggle.Internal;
using NUnit.Framework;
using SharpTestsEx;

namespace nToggleTests.TextFile
{
	public class FalseShortcutTest
	{
		[Test]
		public void ShouldBeDisabled()
		{
			var content = new[] { "someflag=false" };
			var toggleChecker = new ToggleChecker(new FileProviderForTest(content));
			toggleChecker.IsEnabled("someflag")
				.Should().Be.False();
		} 
	}
}