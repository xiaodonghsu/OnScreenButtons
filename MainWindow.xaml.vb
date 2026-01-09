Imports System.Windows
Imports System.Windows.Threading
Imports System.IO

' 窗口位置枚举
Public Enum WindowPlacement
    Left
    Right
End Enum

Class MainWindow

    Public Property Controller As KeyController
    Private buttonConfig As ButtonConfig
    Private position As WindowPlacement

    Public Sub New(position As WindowPlacement)
        InitializeComponent()

        ' 设置窗口位置
        SetWindowPosition(position)
        Me.position = position

        ' 初始化控制器
        Controller = New KeyController()

        ' 加载按钮配置
        Dim configPath As String = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ButtonConfig.json")
        buttonConfig = ButtonConfig.LoadFromFile(configPath)
    End Sub

    Private Sub SetWindowPosition(position As WindowPlacement)
        Select Case position
            Case WindowPlacement.Left
                Me.Left = 0
            Case WindowPlacement.Right
                Me.Left = SystemParameters.PrimaryScreenWidth - Me.Width
        End Select

        ' 垂直居中
        Me.Top = (SystemParameters.PrimaryScreenHeight - Me.Height) / 2
    End Sub

    Private Sub AdjustWindowHeight()
        ' 获取按钮面板的实际高度
        Dim desiredHeight As Double = ButtonPanel.ActualHeight + 80 ' 添加一些额外的空间

        ' 设置最大高度限制（屏幕高度的80%）
        Dim maxHeight As Double = SystemParameters.PrimaryScreenHeight * 0.8

        ' 限制窗口高度
        If desiredHeight > maxHeight Then
            desiredHeight = maxHeight
        ElseIf desiredHeight < 200 Then
            desiredHeight = 200 ' 最小高度
        End If

        ' 调整窗口高度
        Me.Height = desiredHeight

        ' 重新垂直居中
        Me.Top = (SystemParameters.PrimaryScreenHeight - Me.Height) / 2
    End Sub

    ' 从全局变量更新按钮布局（被Application调用）
    Public Sub UpdateButtonLayoutFromGlobal(windowClassName As String)
        UpdateButtonLayout(windowClassName)
    End Sub

    Private Sub UpdateButtonLayout(windowClassName As String)
        ' 清空现有按钮
        ButtonPanel.Children.Clear()

        ' 检查配置是否加载成功
        If buttonConfig Is Nothing Then
            Return
        End If

        ' 获取当前窗口的按钮配置
        Dim buttons = buttonConfig.GetButtonsForWindow(windowClassName)

        ' 更新文本显示当前检测的类名（便于调试）
        If String.IsNullOrEmpty(windowClassName) Then
            ClassNameText.Text = "未检测到窗口"
            Me.Title = "触屏控制器 - 未检测到窗口"
        Else
            ' 显示窗口类名（如果太长就截断）
            Dim displayClassName As String = windowClassName
            If displayClassName.Length > 15 Then
                displayClassName = String.Concat(displayClassName.Substring(0, 12), "...")
            End If
            ClassNameText.Text = displayClassName
            Me.Title = $"触屏控制器 - {windowClassName}"
        End If

        If buttons Is Nothing OrElse buttons.Count = 0 Then
            Return
        End If

        ' 动态创建按钮
        For Each btnInfo As ButtonConfig.ButtonInfo In buttons
            Dim button As New Button() With {
                .Content = btnInfo.DisplayName,
                .Style = Me.Resources("FloatingButtonStyle"),
                .Tag = btnInfo
             }

            AddHandler button.Click, AddressOf Button_Click

            ButtonPanel.Children.Add(button)
        Next

        ' 调整窗口高度
        AdjustWindowHeight()
    End Sub

    Private Sub Button_Click(sender As Object, e As RoutedEventArgs)
        Dim button = DirectCast(sender, Button)
        Dim btnInfo = DirectCast(button.Tag, ButtonConfig.ButtonInfo)

        ' 禁用所有按钮防止重复点击
        For Each child As UIElement In ButtonPanel.Children
            If TypeOf child Is Button Then
                CType(child, Button).IsEnabled = False
            End If
        Next

        ' 获取当前前台窗口句柄
        Dim app = DirectCast(Application.Current, Application)
        Dim foregroundWindowHandle As IntPtr = app.CurrentForegroundWindow

        ' 执行按键操作，传递窗口句柄
        Controller.ExecuteKeyAction(btnInfo.KeyAction, foregroundWindowHandle)

        ' 重新启用所有按钮
        For Each child As UIElement In ButtonPanel.Children
            If TypeOf child Is Button Then
                CType(child, Button).IsEnabled = True
            End If
        Next
    End Sub

    Protected Overrides Sub OnClosed(e As EventArgs)
        MyBase.OnClosed(e)
    End Sub

End Class
