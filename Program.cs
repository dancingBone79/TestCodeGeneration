using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace TestCodeGeneration
{
	class Program
	{
		static void Main(string[] args)
		{
			List<string> compositionOrder = new List<string>();
			compositionOrder.Add("ManageStoreCRUDService::createStore");
			compositionOrder.Add("CoCoMESystem::openStore");
			compositionOrder.Add("ManageCashDeskCRUDService::createCashDesk");
			compositionOrder.Add("CoCoMESystem::openCashDesk");

			string code = Generate(compositionOrder, "python");
			Console.WriteLine(code);
			using (StreamWriter sw = new StreamWriter("program.py"))
				sw.WriteLine(code);

		}

		static List<string> QueryParameters(string functionName)
		{
			//查询 remodel， 取出参数的类型的列表

			return new List<string>(new string[] { "Integer", "String", "String", "Boolean" });
		}

		static string GetValueOfType(string type)
		{
			switch (type)
			{
				case "String":
					return "\"a\"";
				case "Integer":
					return "1";
				case "Boolean":
					return "False";
				default:
					throw new NotSupportedException($"We don't know how to generate a value of type {type}.");
			}
		}


		static string GenerateFunctionCall(string functionName)
		{
			string code = functionName + "(";
			var parameters = QueryParameters(functionName);

			code += string.Join(", ", parameters.Select(parameterType => GetValueOfType(parameterType)));

			return code + ")\n";
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="compositionOrder"></param>
		/// <returns>符合 targetLanguage的代码 </returns>
		public static string Generate(List<string> compositionOrder, string targetLanguage)
		{

			if (targetLanguage == "python")
			{
				string code = "";
				foreach (var function in compositionOrder)
				{
					var parts = function.Split("::");
					string functionName = parts[1];
					code += GenerateFunctionCall(functionName);
				}
				return code;
			}
			else if (targetLanguage == "cpp")
			{

				throw new NotSupportedException($"Generating code in {targetLanguage} is out of scope of this paper.");
				return "#include <std>";
			}
			else
				throw new NotSupportedException($"Generating code in {targetLanguage} is out of scope of this paper.");
		}
	}
}
