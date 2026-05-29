namespace ETL.Core;

public static class ProcessNameConvention
{
    // Derives "Group/JobName" from the type's namespace and class name.
    // ETL.Processes.Gemensam.DummyDataProcess → "Gemensam/DummyData"
    public static string Derive(Type processType)
    {
        var namespaceParts = processType.Namespace?.Split('.') ?? [];
        var processesIdx = Array.IndexOf(namespaceParts, "Processes");

        var group = processesIdx >= 0 && processesIdx < namespaceParts.Length - 1
            ? string.Join('/', namespaceParts[(processesIdx + 1)..])
            : processType.Namespace ?? "Default";

        var jobName = processType.Name.EndsWith("Process", StringComparison.Ordinal)
            ? processType.Name[..^"Process".Length]
            : processType.Name;

        return $"{group}/{jobName}";
    }
}
