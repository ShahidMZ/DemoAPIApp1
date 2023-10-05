namespace API.Extensions;

public static class DateTimeExtensions
{
    // (this DateOnly dob) means this method is extending the DateOnly class.
    public static int CalculateAge(this DateOnly dob)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var age = today.Year - dob.Year;

        // If the user hasn't had their birthday yet this year, reduce their age by 1.
        if (dob > today.AddYears(-age)) age--;

        return age;
    }
}
