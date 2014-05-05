﻿using System;
using NUnit.Framework;
using SharpTestsEx;
using Toggle.Net.Internal;
using Toggle.Net.Specifications;

namespace Toggle.Net.Tests.Other
{
	public class IllegalFeatureStateTest
	{
		[Test]
		public void ShouldNotAcceptNullAsFeature()
		{
			Assert.Throws<ArgumentNullException>(() => 
					new Feature(null)
			);
		}

		[Test]
		public void ShouldNotAcceptNullWhenAddingFeature()
		{
			var feature = new Feature(new FalseSpecification());
			Assert.Throws<ArgumentNullException>(() =>
				feature.AddSpecification(null)
			);
		}
	}
}