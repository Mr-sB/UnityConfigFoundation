# UnityConfigFoundation
Automatically generate data objects according to CSV configuration table.

## Feature
* `TextAssetLoader` can help you easily load `TextAsset` from `StreamAssets` or `Resources` or `PersistentDataPath`.
* `CSVConverter` can convert CSV text to some frequently-used data objects.
* `CVSGenerator` can convert the definition of CSV and Class.
* Support convert Types:

| Primitive | 1D Array | List | ValueTuple | Frequently-used UnityEngine Type |
| :-------: | :------: | :--: | :--------: | :------------------------------: |
|     √     |     √    |   √  |      √     |                 √                |

## CSV form
* First line are field names.
* Second line are field types.
* Third line are descriptions.
* Others are records.

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

## Plugin
* [TinyCSV](https://github.com/Mr-sB/TinyCSV)(Open source)
