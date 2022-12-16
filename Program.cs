// See https://aka.ms/new-console-template for more information

Main();

void Main()
{
	///	原始版本
	ConsoleApp.OriginVersion.Test();

	///	反射版本
	ConsoleApp.ReflectionVersion.Test();
}

public class Employee
{
	//	[PK] 序號

	public int Id { get; set; }

	//	[UK] 班級
	public string? Class { get; set; }

	//	[UK] 編號
	public string? SerialNo { get; set; }

	//	姓名
	public string? Name { get; set; }

	//	TempId
	public string? TempId { get; set; }

	//	Gid
	public string? Gid { get; set; }
}

