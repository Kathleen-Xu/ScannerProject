# ScannerProject

### 技术栈
- 前端：`React` + `Material-UI`
- 后算：`ASP.NET`

### 运行步骤
1. 确保当前环境已经安装好了`Yarn`和`VS2022`
2. 运行后端
    1. 用`VS2022`打开项目文件`backend\backend.sln`
    2. 使用`IIS Express`启动项目，弹出Edge浏览器页面，记录端口号
3. 运行前端
    1. 找到文件`frontend/src/components/MainView`，将常量`server`中的端口号更改为后端页面的端口号。
    2. 以管理员方式运行命令行
    3. 进入项目文件夹
   ``` powershell
    $ cd ScannerProject
    $ cd frontend
    ```
    3. 安装依赖
   ``` powershell
    $ yarn
    ```
    4. 运行
   ``` powershell
    $ yarn start
    ```
   
### 功能介绍
#### 代码迁移
- C#代码迁移
- Xaml代码迁移
  ![g2](https://github.com/Kathleen-Xu/ScannerProject/assets/73981758/d9f2182f-0ff2-42c9-8557-87e9b950894a)

#### 规则管理（仅供参考外观）

- 规则顺序调整
- 规则禁用和启用
- 规则自定义

![g1](https://github.com/Kathleen-Xu/ScannerProject/assets/73981758/470ddebd-4f4e-4d61-984a-fdb92593fb4b)