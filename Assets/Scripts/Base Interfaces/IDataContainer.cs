namespace JoyGodot.Assets.Scripts.Base_Interfaces
{
    public interface IDataContainer
    {
        bool AddData(string key, object value);
        bool RemoveData(string key);
        
        bool HasDataKey(string search);
        bool HasDataValue(object search);

        object[] GetDataValues(string key);
    }
}