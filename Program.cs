﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace TestCodeGeneration
{
	class Program
	//全部按照当前状态改完后运行
	{
		static void Main(string[] args)
		{
			List<string> compositionOrder = new List<string>();
			compositionOrder.Add("ManageStoreCRUDService::createStore");
			compositionOrder.Add("CoCoMESystem::openStore");
			compositionOrder.Add("ManageCashDeskCRUDService::createCashDesk");
			compositionOrder.Add("CoCoMESystem::openCashDesk");

			string code = Generate(compositionOrder, "bash");
			Console.WriteLine(code);
			//output file name(.java   .py   .cpp    .sh)
			using (StreamWriter sw = new StreamWriter("program3.sh"))
				sw.WriteLine(code);

		}

		static string ExtractParenthesesContent(string input)
		{
			// 提取括号及其内容的函数
			// 查找第一个左括号的索引
			int leftParenthesesIndex = input.IndexOf('(');
			if (leftParenthesesIndex == -1)
			{
				throw new Exception("Left parentheses not found.");
			}

			// 查找第一个右括号的索引
			int rightParenthesesIndex = input.IndexOf(')');
			if (rightParenthesesIndex == -1)
			{
				throw new Exception("Right parentheses not found.");
			}

			// 提取括号及其内容
			string content = input.Substring(leftParenthesesIndex, rightParenthesesIndex - leftParenthesesIndex + 1);
			return content;
		}

		static List<string> QueryParameters(string className, string functionName)
		{
			//查询 remodel， 取出参数的类型的列表
			string path = @"C:\Users\p2215981\Desktop\Liu.Lixue\coroutine-program\RequirementAnalysisTests\cocome.remodel";
			var reModelContent = File.ReadAllText(path);
			string[] lines = reModelContent.Split("\n");

			List<string> parameterTypes = new List<string>();
			for (int i = 0; i < lines.Length; i++)
			{
				if (lines[i].Contains(className + "::" + functionName))
				{
					string contentLine = ExtractParenthesesContent(lines[i]);

					var parameterEnum = contentLine.Split(",");
					for (int j = 0; j < parameterEnum.Length; j++)
					{
						parameterTypes.Add(parameterEnum[j].Split(":")[1].Trim(' ', '(', ')'));
					}

					return parameterTypes;
				}
			}
			//如果前面找到了，直接走上面的return，这里需要输出在remodel文件中找不到该函数的状态。
			throw new ArgumentException($"remodel file donnot contain this function: {className + "::" + functionName}");

		}

		static string GetValueOfType(string type)
		{
			switch (type)
			{
				case "String":
					return "\"a\"";
				case "Integer":
					return "\"1\"";
				case "Boolean":
					return "\"false\"";
				default:
					throw new NotSupportedException($"We don't know how to generate a value of type {type}.");
			}
		}


		static string GenerateSmartContractCall(string className, string functionName)
		{
			string code = GenerateLanguageStype();

			code += GenerateDefinePCI();

			code += GeneratePCISmartContractCall(className, functionName);


			return code + "\n";
		}


		static string GenerateLanguageStype() 
		{
			string code = "#!/bin/bash";

			return code + "\n";
		}


		static string GenerateDefinePCI()
		{
			string code = @"function pci {
											if [[ ""$ZSH_NAME"" ]]; then
												setopt local_options SH_WORD_SPLIT
											fi

											peer chaincode invoke -o localhost:7050 --ordererTLSHostnameOverride orderer.example.com --tls --cafile ${PWD}/organizations/ordererOrganizations/example.com/orderers/orderer.example.com/msp/tlscacerts/tlsca.example.com-cert.pem -C mychannel \
											>---  --peerAddresses localhost:7051 --tlsRootCertFiles ${PWD}/organizations/peerOrganizations/org1.example.com/peers/peer0.org1.example.com/tls/ca.crt \
											>---  --peerAddresses localhost:9051 --tlsRootCertFiles ${PWD}/organizations/peerOrganizations/org2.example.com/peers/peer0.org2.example.com/tls/ca.crt \
												  ""$@""
											}";
			return code + "\n";
		}


		static string GeneratePCISmartContractCall(string className, string functionName)
		{

			string code = "pci -C mychannel -n cocome --waitForEvent -c \'{\"function\":\"" + className + ":" + functionName + "\",\"Args\":[";
			var parameters = QueryParameters(className, functionName);

			code += string.Join(", ", parameters.Select(parameterType => GetValueOfType(parameterType)));

			return code + "]}' || fail || return\n";
		}


		static string GenerateDockSmartContractCall()
		{
			string code = "docker stop \"$(docker ps -n --filter \'name=dev\' --format \'{{.ID}}\')\"";

			return code + "\n";
		}


		/// <summary>
		/// 
		/// </summary>
		/// <param name="compositionOrder"></param>
		/// <returns>符合 targetLanguage的代码 </returns>
		public static string Generate(List<string> compositionOrder, string targetLanguage)
		{
			if (new HashSet<string>(compositionOrder).Count < compositionOrder.Count)
				throw new NotSupportedException(
					"The composition order generated by paper \"Typing Requirement Model as Coroutines\" contains no duplicate coroutines. "+
					"We do not intent to handle duplicate coroutines either. You can call it a limitation of our paper.");


			if (targetLanguage == "bash")
			{
				string code = @"#!/bin/bash

function pci {
  if [[ ""$ZSH_NAME"" ]]; then
    setopt local_options SH_WORD_SPLIT
  fi

  peer chaincode invoke -o localhost:7050 --ordererTLSHostnameOverride orderer.example.com --tls --cafile ${PWD}/organizations/ordererOrganizations/example.com/orderers/orderer.example.com/msp/tlscacerts/tlsca.example.com-cert.pem -C mychannel \
>---  --peerAddresses localhost:7051 --tlsRootCertFiles ${PWD}/organizations/peerOrganizations/org1.example.com/peers/peer0.org1.example.com/tls/ca.crt \
>---  --peerAddresses localhost:9051 --tlsRootCertFiles ${PWD}/organizations/peerOrganizations/org2.example.com/peers/peer0.org2.example.com/tls/ca.crt \
      ""$@""
}


export PATH=/home/liushuaixue/liulixue/hyper/fabric-samples/bin:$PATH

export FABRIC_CFG_PATH=$PWD/../config/

if [[ -z ""$GITHUB_WORKSPACE"" ]]; then
>---GITHUB_WORKSPACE=~/cocome-hyperledger/
fi

source $GITHUB_WORKSPACE/src/test/shell/as-org1.sh


";
				foreach (var function in compositionOrder)
				{
					var parts = function.Split("::");
					string className = parts[0];
					string functionName = parts[1];
					code += GeneratePCISmartContractCall(className, functionName);
				}
				return code;
			}
			else if (targetLanguage == "cpp")
			{

				throw new NotSupportedException($"Generating code in {targetLanguage} is out of scope of this paper.");
			}
			else
				throw new NotSupportedException($"Generating code in {targetLanguage} is out of scope of this paper.");
		}
	}
}
