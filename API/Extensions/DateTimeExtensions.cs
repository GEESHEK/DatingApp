namespace API.Extensions;

public static class DateTimeExtensions
{
    public static int CalcuateAge(this DateOnly dob)
    {   //not accounted for leap year etc, not 100%
        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        var age = today.Year - dob.Year;
        //take a year off them if their birthday hasn't passed
        if (dob > today.AddYears(-age)) age--;

        return age;
    } 
}