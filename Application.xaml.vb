Imports System.Windows.Threading
Imports System.Runtime.InteropServices

Class Application
    ' Win32 API声明
    <DllImport("user32.dll", CharSet:=CharSet.Auto)>
    Private Shared Function GetClassName(hWnd As IntPtr, lpClassName As System.Text.StringBuilder, nMaxCount As Integer) As Integer
    End Function

    <DllImport("user32.dll")>
    Private Shared Function GetForegroundWindow() As IntPtr
    End Function

    ' 全局公共变量
    Public Property LeftWindow As MainWindow
    Public Property RightWindow As MainWindow
    Public Property LeftWindowHandle As IntPtr = IntPtr.Zero
    Public Property RightWindowHandle As IntPtr = IntPtr.Zero
    Public Property CurrentForegroundWindow As IntPtr = IntPtr.Zero
    Public Property CurrentForegroundWindowClassName As String = ""
    Private leftWindowClassName As String = ""
    Private rightWindowClassName As String = ""

    ' 统一定时器
    Private detectionTimer As DispatcherTimer

    Protected Overrides Sub OnStartup(e As StartupEventArgs)
        MyBase.OnStartup(e)

        ' 初始化定时器
        detectionTimer = New DispatcherTimer()
        detectionTimer.Interval = TimeSpan.FromSeconds(1)
        AddHandler detectionTimer.Tick, AddressOf DetectionTimer_Tick

        ' 创建左侧窗口并订阅Loaded事件获取句柄
        LeftWindow = New MainWindow(WindowPlacement.Left)
        LeftWindow.Title = "触屏控制器 - 左侧"
        AddHandler LeftWindow.Loaded, Sub(sender, args)
                                          Dim helper As New System.Windows.Interop.WindowInteropHelper(LeftWindow)
                                          LeftWindowHandle = helper.Handle
                                          leftWindowClassName = GetWindowClassName(LeftWindowHandle)
                                          Debug.WriteLine($"[Application] 左侧窗口类名: {leftWindowClassName}, 句柄: {LeftWindowHandle}")
                                          Console.WriteLine($"[Application] 左侧窗口类名: {leftWindowClassName}, 句柄: {LeftWindowHandle}")

                                          ' 两个窗口都加载完成后启动定时器
                                          StartTimerIfNeeded()
                                      End Sub
        LeftWindow.Show()

        ' 创建右侧窗口并订阅Loaded事件获取句柄
        RightWindow = New MainWindow(WindowPlacement.Right)
        RightWindow.Title = "触屏控制器 - 右侧"
        AddHandler RightWindow.Loaded, Sub(sender, args)
                                           Dim helper As New System.Windows.Interop.WindowInteropHelper(RightWindow)
                                           RightWindowHandle = helper.Handle
                                           rightWindowClassName = GetWindowClassName(RightWindowHandle)
                                           Debug.WriteLine($"[Application] 右侧窗口类名: {rightWindowClassName}, 句柄: {RightWindowHandle}")
                                           Console.WriteLine($"[Application] 右侧窗口类名: {rightWindowClassName}, 句柄: {RightWindowHandle}")

                                           ' 两个窗口都加载完成后启动定时器
                                           StartTimerIfNeeded()
                                       End Sub
        RightWindow.Show()
    End Sub

    Private Sub StartTimerIfNeeded()
        If LeftWindowHandle <> IntPtr.Zero AndAlso RightWindowHandle <> IntPtr.Zero AndAlso Not detectionTimer.IsEnabled Then
            ' 初始化CurrentForegroundWindow为当前前台窗口（排除控制器窗口）
            Dim foregroundHwnd As IntPtr = GetForegroundWindow()
            Dim initialClassName As String = GetWindowClassName(foregroundHwnd)

            ' 确保不是控制器窗口本身
            If Not initialClassName.Equals(leftWindowClassName, StringComparison.OrdinalIgnoreCase) AndAlso
               Not initialClassName.Equals(rightWindowClassName, StringComparison.OrdinalIgnoreCase) AndAlso
               foregroundHwnd <> IntPtr.Zero Then
                CurrentForegroundWindow = foregroundHwnd
                CurrentForegroundWindowClassName = initialClassName
                ' 通知所有窗口更新按钮布局
                NotifyAllWindowsUpdate()
            End If
            detectionTimer.Start()
        End If
    End Sub

    Private Sub DetectionTimer_Tick(sender As Object, e As EventArgs)
        Dim foregroundHwnd As IntPtr = GetForegroundWindow()
        Dim newClassName As String = GetWindowClassName(foregroundHwnd)

        ' 检查是否是控制器窗口本身
        Dim isControllerWindow As Boolean = newClassName.Equals(leftWindowClassName, StringComparison.OrdinalIgnoreCase) OrElse
                                             newClassName.Equals(rightWindowClassName, StringComparison.OrdinalIgnoreCase)

        ' 检查窗口句柄是否改变，并且不是控制器窗口本身的类名
        If Not isControllerWindow AndAlso
           foregroundHwnd <> IntPtr.Zero AndAlso
           (foregroundHwnd <> CurrentForegroundWindow OrElse
            Not newClassName.Equals(CurrentForegroundWindowClassName, StringComparison.OrdinalIgnoreCase)) Then
            Debug.WriteLine($"[Application] 前台窗口改变, 句柄: {foregroundHwnd}, 类名: '{newClassName}'")
            Console.WriteLine($"[Application] 前台窗口改变, 句柄: {foregroundHwnd}, 类名: '{newClassName}'")
            CurrentForegroundWindow = foregroundHwnd
            CurrentForegroundWindowClassName = newClassName
            NotifyAllWindowsUpdate()
        End If
    End Sub

    ' 辅助方法：获取窗口类名
    Private Function GetWindowClassName(hWnd As IntPtr) As String
        If hWnd = IntPtr.Zero Then
            Return ""
        End If
        Dim className As New System.Text.StringBuilder(256)
        GetClassName(hWnd, className, className.Capacity)
        Return className.ToString()
    End Function

    Private Sub NotifyAllWindowsUpdate()
        Debug.WriteLine($"[Application] 通知所有窗口更新按钮布局, 类名: '{CurrentForegroundWindowClassName}'")
        Console.WriteLine($"[Application] 通知所有窗口更新按钮布局, 类名: '{CurrentForegroundWindowClassName}'")

        If LeftWindow IsNot Nothing Then
            LeftWindow.UpdateButtonLayoutFromGlobal(CurrentForegroundWindowClassName)
        End If
        If RightWindow IsNot Nothing Then
            RightWindow.UpdateButtonLayoutFromGlobal(CurrentForegroundWindowClassName)
        End If
    End Sub

    Protected Overrides Sub OnExit(e As ExitEventArgs)
        If detectionTimer IsNot Nothing Then
            detectionTimer.Stop()
        End If
        MyBase.OnExit(e)
    End Sub

End Class
