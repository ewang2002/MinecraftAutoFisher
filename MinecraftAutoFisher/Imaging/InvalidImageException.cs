using System;

namespace MinecraftAutoFisher.Imaging
{
	public class InvalidImageException : Exception
	{
		public InvalidImageException(string msg) : base(msg)
		{
		}
	}
}