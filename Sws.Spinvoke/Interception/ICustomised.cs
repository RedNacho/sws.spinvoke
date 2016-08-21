using System;

namespace Sws.Spinvoke.Interception
{
	public interface ICustomised<out TCustomisation>
	{
		TCustomisation Customisation { get; }
	}
}

