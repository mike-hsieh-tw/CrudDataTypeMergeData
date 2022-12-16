using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp
{
    static class OriginVersion
    {
        static public void Test()
        {
            List<Employee> Items = new List<Employee>(){
                new Employee { Id = 1, Class = "A", SerialNo = "001", Name = "John"},
                new Employee { Id = 2, Class = "A", SerialNo = "002", Name = "Mary"},
                new Employee { Id = 3, Class = "A", SerialNo = "003", Name = "Tom"},
                new Employee { Id = 4, Class = "A", SerialNo = "004", Name = "Eric"},
                new Employee { Id = 5, Class = "A", SerialNo = "005", Name = "Alan"},
                new Employee { Id = 6, Class = "A", SerialNo = "006", Name = "Jack"}
            };

            List<Employee> InsertItems = new List<Employee>(){
                new Employee { Id = 7, Class = "A", SerialNo = "007", Name = "Stuart"},
                new Employee { Id = 8, Class = "A", SerialNo = "008", Name = "Owen"},
                new Employee { Id = 9, Class = "A", SerialNo = "003", Name = "Sam"},		//	Deuplicate Data
	        };

            List<Employee> UpdateItems = new List<Employee>(){
                new Employee { Id = 5, Class = "A", SerialNo = "005", Name = "Robin"},
                new Employee { Id = 6, Class = "A", SerialNo = "006", Name = "Tina"},
                new Employee { Id = 4, Class = "A", SerialNo = "005", Name = "Eric"},	//	Deuplicate Data
	        };

            List<Employee> DeleteItems = new List<Employee>(){
                new Employee { Id = 1, Class = "A", SerialNo = "001", Name = "John"},
                new Employee { Id = 2, Class = "A", SerialNo = "002", Name = "Mary"},
            };

            List<Employee> ResultItems = new List<Employee>();

            #region 完整版
            //var insertItemIds = InsertItems.Select(x => x.Id).ToList();
            //var deleteItemIds = DeleteItems.Select(x => x.Id).ToList();
            //var updateItemIds = UpdateItems.Select(x => x.Id).ToList();

            ////	刪除數據
            //ResultItems = Items.Where(p => deleteItemIds.Contains(p.Id) == false).ToList();

            ////	更新數據
            //ResultItems = ResultItems.Where(p => updateItemIds.Contains(p.Id) == false).Concat(UpdateItems).ToList();

            ////	新增數據
            //ResultItems = ResultItems.Concat(InsertItems).ToList();

            ////	找出重複的資料
            //var duplicates = ResultItems
            //    .GroupBy(p => new { p.Class, p.SerialNo })
            //    .Where(p => p.Count() > 1)
            //    .SelectMany(p => p)
            //    .Where(p => updateItemIds.Contains(p.Id) || insertItemIds.Contains(p.Id));
            #endregion

            #region 精簡版
            var editItemIds = InsertItems.Concat(UpdateItems).Select(x => x.Id).ToList();
            var removeItemIds = DeleteItems.Concat(UpdateItems).Select(x => x.Id).ToList();

            ResultItems = Items
                //	以下為合併資料
                .Where(p => removeItemIds.Contains(p.Id) == false)
                .Concat(UpdateItems)
                .Concat(InsertItems)

                //	以下為取得重複
                .GroupBy(p => new { p.Class, p.SerialNo })
                .Where(p => p.Count() > 1)
                .SelectMany(p => p)
                .Where(p => editItemIds.Contains(p.Id))
                .ToList();
            #endregion
        }
    }
}
