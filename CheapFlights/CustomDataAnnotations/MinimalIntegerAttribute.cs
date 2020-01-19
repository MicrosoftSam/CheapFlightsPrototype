using System.ComponentModel.DataAnnotations;

namespace CheapFlights.CustomDataAnnotations
{
    public class MinimalIntegerAttribute : ValidationAttribute
    {
        //provjera da je broj putnika barem 1
        public override bool IsValid(object value)
        {
            return value != null && (int)value >= 1;
        }
    }
}
