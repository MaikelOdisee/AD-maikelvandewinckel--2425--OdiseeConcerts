using System.ComponentModel.DataAnnotations;
using System.Globalization; // Nodig voor CultureInfo.InvariantCulture

namespace OdiseeConcerts.ValidationAttributes
{
    public class MemberCardNumberValidationAttribute : ValidationAttribute
    {
        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            // Lidkaartnummer is optioneel, dus als de waarde null of leeg is, is het geldig.
            if (value == null || string.IsNullOrWhiteSpace(value.ToString()))
            {
                return ValidationResult.Success;
            }

            var memberCardNumber = value.ToString();

            // 1. Moet starten met “ODI”
            if (!memberCardNumber.StartsWith("ODI"))
            {
                return new ValidationResult("Lidkaartnummer moet starten met 'ODI'.");
            }

            // 2. Moet in totaal 13 karakters bevatten (ODI (3) + 10 numerieke karakters)
            if (memberCardNumber.Length != 13)
            {
                return new ValidationResult("Lidkaartnummer moet 13 karakters bevatten (ODI + 10 cijfers).");
            }

            // 3. Moeten de laatste 10 karakters numeriek zijn
            var numericPart = memberCardNumber.Substring(3); // Haal de laatste 10 karakters op (vanaf index 3)

            // Gebruik TryParse om te controleren of het numeriek is
            if (!long.TryParse(numericPart, NumberStyles.None, CultureInfo.InvariantCulture, out _))
            {
                // Als TryParse mislukt, betekent dit dat het geen geldig lang getal is (dus niet volledig numeriek).
                return new ValidationResult("De laatste 10 karakters van het lidkaartnummer moeten numeriek zijn.");
            }

            // Als alle controles succesvol zijn, is de validatie geslaagd.
            return ValidationResult.Success;
        }
    }
}
