using System.Data;

public class ReportDataSourceComponents
{
    public DataTable Table { get; private set; }
    public string DataSetName { get; private set; }

    public ReportDataSourceComponents(string dataSetName, DataTable table)
    {
        Table = table;
        DataSetName = dataSetName;
    }
}
