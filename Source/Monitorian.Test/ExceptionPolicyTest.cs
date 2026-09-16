using System;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Monitorian.Core;

namespace Monitorian.Test;

[TestClass]
public class ExceptionPolicyTest
{
	[TestMethod]
	public void OrdinaryExceptionsAreRecoverable()
	{
		Assert.IsTrue(AppKeeper.IsRecoverableException(new InvalidOperationException()));
		Assert.IsTrue(AppKeeper.IsRecoverableException(
			new AggregateException(new InvalidOperationException(), new ArgumentException())));
	}

	[TestMethod]
	public void FatalRuntimeExceptionsAreNotRecoverable()
	{
		Assert.IsFalse(AppKeeper.IsRecoverableException(new OutOfMemoryException()));
		Assert.IsFalse(AppKeeper.IsRecoverableException(
			new AggregateException(new InvalidOperationException(), new AccessViolationException())));
	}
}
