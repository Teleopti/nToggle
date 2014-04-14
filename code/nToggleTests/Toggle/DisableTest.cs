﻿using System.Collections.Generic;
using nToggle.Internal;
using nToggle.Specifications;
using NUnit.Framework;
using SharpTestsEx;

namespace nToggleTests.Toggle
{
	public class DisableTest
	{
		[Test]
		public void ShouldBeDisabled()
		{
			const string flag = "someFlag";

			var nToggle = new ToggleChecker(new HashSet<Feature>
			{
				new Feature(flag, new FalseSpecification())
			});

			nToggle.IsEnabled(flag)
				.Should().Be.False();
		}

		[Test]
		public void ShouldBeDisableIfAnySpecificationReturnFalse()
		{
			const string flag = "someFlag";

			var nToggle = new ToggleChecker(new HashSet<Feature>
			{
				new Feature(flag, new FalseSpecification(), new TrueSpecification())
			});

			nToggle.IsEnabled(flag)
				.Should().Be.False();
		}

		[Test]
		public void ShouldBeDisabledIfNotDefined()
		{
			var nToggle = new ToggleChecker(new HashSet<Feature>());

			nToggle.IsEnabled("non existing")
				.Should().Be.False();
		}

		[Test]
		public void ShouldBeDisabledIfNoSpecification()
		{
			const string flag = "someFlag";

			var nToggle = new ToggleChecker(new HashSet<Feature>
			{
				new Feature(flag)
			});

			nToggle.IsEnabled(flag)
				.Should().Be.False();
		}
	}
}