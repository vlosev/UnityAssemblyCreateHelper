namespace Editor.Utils.AssemblyCreateHelper
{
	internal readonly struct Result
	{
		public readonly bool Successful;
		public readonly string Error;
			
		private Result(bool successful, string error)
		{
			Successful = successful;
			Error = error;
		}

		public static Result Success() => new Result(true, string.Empty);
		public static Result Failed(string reason) => new Result(false, reason);
	}
}