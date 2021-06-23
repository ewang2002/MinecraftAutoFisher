using System;

namespace AutoFisher.Imaging
{
	public class InvalidImageException : Exception
	{
		public InvalidImageException(string msg) : base(msg)
		{
		}
	}
}