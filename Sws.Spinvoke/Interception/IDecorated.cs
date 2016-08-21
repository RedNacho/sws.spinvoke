using System;

namespace Sws.Spinvoke.Interception
{
	public interface IDecorated<out TDecoration>
	{
		TDecoration Decoration { get; }
	}
}

