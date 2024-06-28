
1. generate the bash test file, named 'program00.sh'.
```
dotnet build
```
在build后的/bin/Debug/net8.0目录下
```
dotnet TestCodeGeneration.dll /home/liushuaixue/liulixue/testProject/coroutine-program/RequirementAnalysisTests/cocome.remodel
```


2. change the file execution permission
run command:
```
chmod +x program00.sh
```


3. run the bash test file to execute the hyperlydger (now: cocome contract)
run command:
```
GITHUB_WORKSPACE=~/liulixue/testProject/cocome-hyperledger /home/liushuaixue/TestCodeGeneration/bin/Debug/net8.0/program00.sh
```
