using System;
using System.ComponentModel.DataAnnotations;

namespace CheapFlights.CustomDataAnnotations
{
    public class FutureDateAttribute : ValidationAttribute
    {
        //Trenutno onemogučuje pretragu za točno današnji dan TODO
        public override bool IsValid(object value)
        {
            return value != null && (DateTime)value > DateTime.Now;
        }
    }
}
