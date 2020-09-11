# UnityConfigFoundation
Read/Write csv table and automatically generate data objects according to CSV configuration table.

## Feature
* Support table cells contain commas and double quotes.
* `CSVTableReader` can convert CSV text to a simple table object.
* `CSVTableWriter` can encode string to csv format.
* `TextAssetLoader` can help you easily load `TextAsset` from `StreamAssets` or `Resources` or `PersistentDataPath`.
* `CSVConverter` can convert CSV text to some frequently-used data objects.
* `CVSGenerator` can convert the definition of CSV and Class.

## Extend
If you want to add some data type converter, create a class and use `partial` keyword to be part of `FieldConverter` class.
```c#
namespace GameUtil.Config
{
    public static partial class FieldConverter
    {
        //Must add FieldConverterAttribute to convert method.
        //The method must be static, return type can not be void, and parameter is string.
        [FieldConverter]
        public static DataType Vector4IntConverter(this string fieldContent)
        {
            //Add convert logic.
        }
    }
}
```