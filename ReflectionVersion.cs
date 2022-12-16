using Newtonsoft.Json;
using System.ComponentModel;
using System.Dynamic;

namespace ConsoleApp
{
    static public class ReflectionVersion
    {
        static public void Test()
        {
            List<Employee> Items = new List<Employee>(){
                new Employee { Id = 1, Class = "A", SerialNo = "001", Name = "John", TempId = "1"},
                new Employee { Id = 2, Class = "A", SerialNo = "002", Name = "Mary", TempId = "2"},
                new Employee { Id = 3, Class = "A", SerialNo = "003", Name = "Tom", TempId = "3"},
				new Employee { Id = 4, Class = "A", SerialNo = "004", Name = "Eric", TempId = "4"},
				new Employee { Id = 5, Class = "A", SerialNo = "005", Name = "Alan", TempId = "5"},
                new Employee { Id = 6, Class = "A", SerialNo = "006", Name = "Jack", TempId = "6"}
            };

            List<Employee> InsertItems = new List<Employee>(){
                new Employee { Id = 7, Class = "A", SerialNo = "007", Name = "Stuart", TempId = ""},
				new Employee { Id = 8, Class = "A", SerialNo = "008", Name = "Owen", TempId = ""},
				new Employee { Id = 9, Class = "A", SerialNo = "003", Name = "Sam", TempId = ""}, //	Deuplicate Data
			};

            List<Employee> UpdateItems = new List<Employee>(){
                new Employee { Id = 5, Class = "A", SerialNo = "005", Name = "Robin", TempId = "5"},  //	Deuplicate Data
				new Employee { Id = 6, Class = "A", SerialNo = "006", Name = "Tina", TempId = "6"},
				new Employee { Id = 4, Class = "A", SerialNo = "005", Name = "Eric", TempId = "4"}, //	Deuplicate Data
			};

            List<Employee> DeleteItems = new List<Employee>(){
                new Employee { Id = 1, Class = "A", SerialNo = "001", Name = "John", TempId = "1"},
                new Employee { Id = 2, Class = "A", SerialNo = "002", Name = "Mary", TempId = "2"},
            };


            var checkMergeData = CheckAndGetMergeData<Employee>(null, null, null, null, null);

            var checkMergeData2 = CheckAndGetMergeData(Items, InsertItems, UpdateItems, DeleteItems, new List<string> { "Class", "SerialNo" }, "TempId", excludeOrigin: true);
        }

        /// <summary>
        /// 檢查重複 並 取得合併後的資料。
        /// 
        /// 1.  CheckAndGetMergeData 方法回傳一個物件內含三個屬性:
        ///     1.1 HasDuplicate(是否有重複的資料): 若資料有重複，HasDuplicate 為 true。
        ///     1.2 MergeData(合併後的資料): 將 原始資料(originItems)、新增的資料(insertItems)、編輯的資料(updateItems) 合併，並且移除 刪除的資料(deleteItems)，模擬進入DB後的資料集合，以利後續資料的計算、加總...等等的複雜業務邏輯。
        ///     1.3 DuplicateData(重複的資料): 依照 複合主鍵集合(compositeKeys)，將 新增的資料(insertItems)、編輯的資料(updateItems) 和 原始資料(originItems)比對，回傳有重複的資料。
        ///     
        /// 2.  excludeOrigin 欄位說明: 
        ///     2.1 excludeOrigin 預設為 true，當資料有重複時，會把DB資料排除，僅提供當前異動的資料。
        ///     2.2 excludeOrigin 若為 false，當資料有重複時，會把DB資料加入回傳，提供 原始資料 和 當前異動資料 的集合。
        ///     
        /// 3.  需確保傳入的 原始資料(originItems), 更新資料(updateItems), 刪除資料(deleteItems)的[不重複鍵]不得為空值或是NULL。
        /// 
        /// 4.  需確保傳入的 複合主鍵集合(compositeKeys) 存在於泛型T的Class中。
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="originItems">原始資料(From Db)</param>
        /// <param name="insertItems">新增的資料</param>
        /// <param name="updateItems">編輯的資料</param>
        /// <param name="deleteItems">刪除的資料</param>
        /// <param name="compositeKeys">複合主鍵集合</param>
        /// <param name="uniKey">[選填]不重複鍵(TempID, Seq_No)</param>
        /// <param name="excludeOrigin">[選填]重複資料是否排除原始資料(DB)</param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        static public CheckRtnMergeDataModel<T> CheckAndGetMergeData<T>(IEnumerable<T> originItems, IEnumerable<T> insertItems, IEnumerable<T> updateItems, IEnumerable<T> deleteItems, IEnumerable<string> compositeKeys, string uniKey = null, bool excludeOrigin = true)
        {
            CheckKeysExist<T>(compositeKeys, uniKey);

            int salt = new Random().Next(100, 1000);
            var dynamicItems = GetDynamicObjList(originItems, compositeKeys, uniKey, salt, isOrigin: true);
            var dynamicInsertItems = GetDynamicObjList(insertItems, compositeKeys, uniKey, salt, isInsert: true);
            var dynamicUpdateItems = GetDynamicObjList(updateItems, compositeKeys, uniKey, salt);
            var dynamicDeleteItems = GetDynamicObjList(deleteItems, compositeKeys, uniKey, salt);

            var editItemIds = dynamicInsertItems.Concat(dynamicUpdateItems).Select(x => x.__UniKey ?? x.__CompositeHashKey).ToList();
            var removeItemIds = dynamicDeleteItems.Concat(dynamicUpdateItems).Select(x => x.__UniKey ?? x.__CompositeHashKey).ToList();


            var mergeData = dynamicItems;
            if (string.IsNullOrWhiteSpace(uniKey) == false)
            {
                mergeData = mergeData.Where(p => removeItemIds.Contains(p.__UniKey ?? p.__CompositeHashKey) == false).ToList();
            }
            mergeData = mergeData.Concat(dynamicUpdateItems).Concat(dynamicInsertItems).ToList();


            var duplicateData = mergeData.GroupBy(p => new { p.__CompositeHashKey }).Where(p => p.Count() > 1).SelectMany(p => p);
            if (excludeOrigin == true && string.IsNullOrWhiteSpace(uniKey) == false)
            {
                duplicateData = duplicateData.Where(p => editItemIds.Contains(p.__UniKey ?? p.__CompositeHashKey)).Where(p => p.__IsOrigin == false);
            }

            return new CheckRtnMergeDataModel<T>()
            {
                HasDuplicate = duplicateData?.Any() == true,
                MergeData = JsonConvert.DeserializeObject<List<T>>(JsonConvert.SerializeObject(mergeData.ToList())),
                DuplicateData = JsonConvert.DeserializeObject<List<T>>(JsonConvert.SerializeObject(duplicateData.ToList())),
            };
        }

        static private IList<dynamic> GetDynamicObjList<T>(IEnumerable<T> objList, IEnumerable<string> compositeKeys, string unikey, int salt, bool isOrigin = false, bool isInsert = false)
        {
            List<dynamic> dynamicObjList = new List<dynamic>();

            if (objList == null)
                return dynamicObjList;

            objList.ToList().ForEach(obj =>
            {
                if (obj != null)
                {
                    dynamic dObject = new ExpandoObject();
                    dObject = obj.DataMergeToDynamic();
                    dynamic unikeyElseVal = isInsert ? Guid.NewGuid().ToString()?.GetHashCode().ToString() : null;
                    dObject.__CompositeHashKey = GetCompositeHashKeyValue(obj, compositeKeys, salt);
                    dObject.__UniKey = !string.IsNullOrWhiteSpace(unikey) ? typeof(T).GetProperty(unikey)?.GetValue(obj)?.ToString()?.GetHashCode().ToString() : unikeyElseVal;
                    dObject.__IsOrigin = isOrigin;
                    dynamicObjList.Add(dObject);

                    if (string.IsNullOrWhiteSpace(unikey) == false && string.IsNullOrWhiteSpace(dObject.__UniKey))
                    {
                        throw new Exception($"The uniKey:[{unikey}] can not be null or whiteSpace within originItems, updateItems and deleteItems.");
                    }
                }
            });

            return dynamicObjList;
        }

        static private void CheckKeysExist<T>(IEnumerable<string> keys, string uniKey = null)
        {
            Type type = typeof(T);
            var properties = type.GetProperties();

            List<string> undefindColumnList = new List<string>();

            #region Check UniKey
            if (!string.IsNullOrWhiteSpace(uniKey) && properties.Select(p => p.Name).Contains(uniKey) == false)
            {
                undefindColumnList.Add($"[{uniKey}]");
            }
            #endregion

            #region Check Keys
            var keysExceptList = keys?.Except(properties.Select(p => p.Name)).Select(p => $"[{p}]").ToList();
            if (keysExceptList?.Any() == true)
            {
                undefindColumnList.AddRange(keysExceptList);
            }
            #endregion

            if (undefindColumnList?.Any() == true)
            {
                throw new Exception($"Can not find columns from <{type.Name}>: {string.Join(", ", undefindColumnList)} .");
            }
        }

        static private int GetCompositeHashKeyValue<T>(T obj, IEnumerable<string> keys, int salt)
        {
            unchecked
            {
                int result = 0;
                Type type = typeof(T);
                foreach (var prop in type.GetProperties())
                {
                    if (keys != null && keys.Contains(prop.Name))
                    {
                        var currVal = prop.GetValue(obj)?.ToString();
                        result += (result * salt) ^ (currVal != null ? currVal.GetHashCode() : 0);
                    }
                };
                return result;
            }
        }

        static dynamic DataMergeToDynamic(this object value)
        {
            IDictionary<string, object> expando = new ExpandoObject();

            foreach (PropertyDescriptor property in TypeDescriptor.GetProperties(value.GetType()))
                expando.Add(property.Name, property.GetValue(value));

            return expando as ExpandoObject;
        }
    }
}

public class CheckRtnMergeDataModel<T>
{
	/// <summary>
	/// 是否有重複的資料
	/// </summary>
	public bool HasDuplicate { get; set; }

	/// <summary>
	/// 合併後的資料
	/// </summary>
	public List<T>? MergeData { get; set; }

	/// <summary>
	/// 重複的資料
	/// </summary>
	public List<T>? DuplicateData { get; set; }
}


