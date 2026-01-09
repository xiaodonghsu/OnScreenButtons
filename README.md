# OnScreenButtons

一个基于WPF的触摸屏控制器应用，支持动态配置和自动化按键操作。

## 功能特性

- **双窗口支持**：左右两个独立的控制器窗口
- **智能窗口检测**：自动检测前台窗口并动态更新按钮布局
- **灵活配置**：基于JSON的配置文件，为不同窗口类名设置自定义按钮
- **自动调整**：窗口高度根据按钮数量自动调整
- **直接控制**：使用Win32 API直接激活目标窗口并发送按键

## 项目结构

```
OnScreenKeys/
├── Application.xaml          # 应用程序入口
├── Application.xaml.vb       # 全局窗口检测和定时器管理
├── MainWindow.xaml           # 主窗口UI定义
├── MainWindow.xaml.vb        # 主窗口逻辑
├── KeyController.vb          # 按键执行控制器
├── ButtonConfig.vb            # 按钮配置管理
├── ButtonConfig.json         # 按钮配置文件
├── OnScreenKeys.sln          # 解决方案文件
└── OnScreenKeys.vbproj       # 项目文件
```

## 配置说明

按钮配置存储在 `ButtonConfig.json` 文件中，格式如下：

```json
{
  "defaultButtons": [
    {
      "text": "按钮名称",
      "action": "按键动作"
    }
  ],
  "configs": [
    {
      "targetWindowClass": "窗口类名",
      "buttons": [
        {
          "text": "按钮名称",
          "action": "按键动作"
        }
      ]
    }
  ]
}
```

### 配置字段说明

- **defaultButtons**: 默认按钮配置，当窗口类名不匹配任何配置时使用
- **configs**: 特定窗口的按钮配置
  - **targetWindowClass**: 目标窗口的类名（可通过Spy++等工具获取）
  - **buttons**: 按钮列表
    - **text**: 按钮显示文本
    - **action**: 按键动作（如 "Ctrl+C", "Alt+Tab" 等）

## 使用方法

1. **编译运行**：使用Visual Studio打开 `OnScreenKeys.sln` 并编译运行
2. **配置按钮**：编辑 `ButtonConfig.json` 文件，为需要控制的窗口添加按钮配置
3. **使用控制器**：点击按钮即可将按键发送到当前活动窗口

## 按键动作支持

支持常见的组合键，例如：
- `Ctrl+C`: 复制
- `Ctrl+V`: 粘贴
- `Alt+Tab`: 切换窗口
- `F5`: 刷新
- 等等...

## 获取窗口类名

使用工具如 Spy++ 或 Process Explorer 可以获取目标窗口的类名：
1. 打开 Spy++
2. 选择 "Find Window"
3. 将工具拖动到目标窗口上
4. 记录显示的 "Class" 字段

## 技术栈

- **框架**: .NET Framework (WPF)
- **语言**: VB.NET
- **UI**: XAML
- **序列化**: System.Text.Json
- **系统交互**: Win32 API (GetForegroundWindow, GetClassName, SetForegroundWindow, SendInput等)

## 开发环境

- Visual Studio
- .NET Framework 4.7.2 或更高版本

## 注意事项

- 控制器窗口本身不会触发按钮更新
- 切换窗口时会自动检测并更新按钮布局
- 窗口高度会根据按钮数量自动调整（最大为屏幕高度的80%）

## 许可证

MIT License

## 作者

xiaodonghsu
