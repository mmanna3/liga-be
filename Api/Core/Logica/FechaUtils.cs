namespace Api.Core.Logica;

public class FechaUtils
{
    private const string FormatoFechaBackup = "yyyy-MM-dd--HH-mm-ss";
    public static readonly string AhoraEnArgentinaFormatoBackup = $"{TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, TimeZoneInfoArg()).ToString(FormatoFechaBackup)}";
	
    private static TimeZoneInfo TimeZoneInfoArg()
    {
        var p = (int) Environment.OSVersion.Platform;
			
        if (p is 4 or 6 or 128) {
            // es Unix
            return TimeZoneInfo.FindSystemTimeZoneById("America/Argentina/Buenos_Aires");
        } 
			
        // Fallback es Windows
        return TimeZoneInfo.FindSystemTimeZoneById("Argentina Standard Time");
    }
}