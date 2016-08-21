using System;

namespace Sws.Spinvoke.Core
{
	public class NativeDelegateDefinition
	{
		private readonly string _fileName;
		private readonly string _functionName;
		private readonly DelegateSignature _delegateSignature;
		private readonly Type _explicitDelegateType;

		public NativeDelegateDefinition(string fileName, string functionName, DelegateSignature delegateSignature, Type explicitDelegateType = null)
		{
			if (fileName == null)
				throw new ArgumentNullException ("fileName");

			if (functionName == null)
				throw new ArgumentNullException ("functionName");

			if (delegateSignature == null)
				throw new ArgumentNullException ("delegateSignature");

			_fileName = fileName;
			_functionName = functionName;
			_delegateSignature = delegateSignature;
			_explicitDelegateType = explicitDelegateType;
		}

		public string FunctionName { get { return _functionName; } }

		public DelegateSignature DelegateSignature { get { return _delegateSignature; } }

		public string FileName { get { return _fileName; } }

		public Type ExplicitDelegateType { get { return _explicitDelegateType; } }
	}
}

