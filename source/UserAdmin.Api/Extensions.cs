using System.Linq;
using System.Web.Http.ModelBinding;

namespace System.Web.Http
{
    static class Extensions
    {
        public static string GetErrorMessage(this ModelStateDictionary modelState)
        {
            if (modelState == null) return null;

            var errors =
                from error in modelState
                where error.Value.Errors.Any()
                from err in error.Value.Errors
                select err.ErrorMessage;

            return errors.FirstOrDefault();
        }

        public static dynamic GetErrorMessages(this ModelStateDictionary modelState)
        {
            if (modelState == null) return null;

            var errors =
                from error in modelState
                where error.Value.Errors.Any()
                from err in error.Value.Errors
                select err.ErrorMessage;

            if (!errors.Any())
            {
                return null;
            }

            if (errors.Skip(1).Any())
            {
                return errors.ToArray();
            }

            return errors.First();
        }
    }
}
