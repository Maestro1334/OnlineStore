using System;

namespace Service.Validators
{
    public class GuidValidator
    {
        public static bool VerifyGuidType(string id, out Guid guid)
        {
			guid = Guid.Empty;
			try
			{
				guid = new Guid(id);
				return true;
			}
			catch
			{
				return false;
			}
		}
    }
}
