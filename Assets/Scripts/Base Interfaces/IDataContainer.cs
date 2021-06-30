namespace JoyGodot.Assets.Scripts.Base_Interfaces
{
    public interface IDataContainer
    {
        bool AddData(object key, object value);
        bool RemoveData(object key);
        
        bool HasDataKey(object search);
        bool HasDataValue(object search);

        object[] GetDataValues(object key);
        object[] GetDataKeysForValue(object value);
    }
}