DateTime ConvertLargeIntegerToDateTime(object comObject)
{
    if (comObject == null || comObject is DBNull)
        return DateTime.MinValue;

    var type = comObject.GetType();
    int highPart = (int)type.InvokeMember("HighPart", System.Reflection.BindingFlags.GetProperty, null, comObject, null);
    int lowPart = (int)type.InvokeMember("LowPart", System.Reflection.BindingFlags.GetProperty, null, comObject, null);

    long fileTime = ((long)highPart << 32) + (uint)lowPart;

    try
    {
        return DateTime.FromFileTimeUtc(fileTime);
    }
    catch
    {
        return DateTime.MinValue;
    }
}
