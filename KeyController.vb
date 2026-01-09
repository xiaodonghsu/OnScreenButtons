Imports System.Windows
Imports System.Runtime.InteropServices
Imports System.Threading
Imports System.Diagnostics
Imports System.Text

Public Class KeyController

    ' API声明，用于模拟键盘输入
    <DllImport("user32.dll")>
    Private Shared Sub keybd_event(ByVal bVk As Byte, ByVal bScan As Byte, ByVal dwFlags As Integer, ByVal dwExtraInfo As Integer)
    End Sub

    <StructLayout(LayoutKind.Sequential)>
    Public Structure INPUT
        Public type As Integer
        Public ki As KEYBDINPUT
    End Structure

    <StructLayout(LayoutKind.Sequential)>
    Public Structure KEYBDINPUT
        Public wVk As Short
        Public wScan As Short
        Public dwFlags As Integer
        Public time As Integer
        Public dwExtraInfo As IntPtr
    End Structure

    <DllImport("user32.dll", SetLastError:=True)>
    Private Shared Function SendInput(ByVal nInputs As Integer, ByRef pInputs As INPUT, ByVal cbSize As Integer) As Integer
    End Function

    ' API声明，用于获取和设置前台窗口
    <DllImport("user32.dll")>
    Private Shared Function GetForegroundWindow() As IntPtr
    End Function

    <DllImport("user32.dll")>
    Private Shared Function SetForegroundWindow(ByVal hWnd As IntPtr) As Boolean
    End Function

    <DllImport("user32.dll")>
    Private Shared Function ShowWindow(ByVal hWnd As IntPtr, ByVal nCmdShow As Integer) As Boolean
    End Function

    <DllImport("user32.dll")>
    Private Shared Function IsIconic(ByVal hWnd As IntPtr) As Boolean
    End Function

    <DllImport("user32.dll")>
    Private Shared Function IsZoomed(ByVal hWnd As IntPtr) As Boolean
    End Function

    <DllImport("user32.dll")>
    Private Shared Function AttachThreadInput(ByVal idAttach As Integer, ByVal idAttachTo As Integer, ByVal fAttach As Boolean) As Boolean
    End Function

    <DllImport("user32.dll")>
    Private Shared Function GetWindowThreadProcessId(ByVal hWnd As IntPtr, ByRef lpdwProcessId As Integer) As Integer
    End Function

    <DllImport("user32.dll")>
    Private Shared Function PostMessage(ByVal hWnd As IntPtr, ByVal Msg As UInteger, ByVal wParam As IntPtr, ByVal lParam As IntPtr) As Boolean
    End Function

    <System.Security.SuppressUnmanagedCodeSecurity()>
    <DllImport("user32.dll", CharSet:=CharSet.Auto)>
    Private Shared Function GetClassName(ByVal hWnd As IntPtr, ByVal lpClassName As StringBuilder, ByVal nMaxCount As Integer) As Integer
    End Function

    ' 常量定义
    Private Const KEYEVENTF_KEYDOWN As Integer = &H0
    Private Const KEYEVENTF_KEYUP As Integer = &H2
    Private Const SW_RESTORE As Integer = 9
    Private Const INPUT_KEYBOARD As Integer = 1

    ' 虚拟键码
    Private Const VK_LEFT As Integer = &H25
    Private Const VK_RIGHT As Integer = &H27
    Private Const VK_F5 As Integer = &H74
    Private Const VK_PAGEUP As Integer = &H21
    Private Const VK_PAGEDOWN As Integer = &H22
    Private Const VK_MENU As Integer = &H12  ' Alt 键

    ' 窗口消息
    Private Const WM_KEYDOWN As UInteger = &H100
    Private Const WM_KEYUP As UInteger = &H101

    ' 构造函数（不需要参数，因为不再枚举窗口）
    Public Sub New()
    End Sub

    ' 获取前台窗口的类名
    Public Function GetForegroundWindowClassName() As String
        Dim hWnd As IntPtr = GetForegroundWindow()
        If hWnd = IntPtr.Zero Then
            Return ""
        End If
        Return GetWindowClassName(hWnd)
    End Function

    ' 根据按键操作类型执行按键（接收窗口句柄参数）
    Public Sub ExecuteKeyAction(ByVal keyAction As String, ByVal targetWindowHandle As IntPtr)
        If targetWindowHandle = IntPtr.Zero Then
            Return
        End If

        ' 激活目标窗口
        ActivateWindow(targetWindowHandle)
        Thread.Sleep(150)

        Select Case keyAction.ToLower()
            Case "left"
                SendKeyPress(VK_LEFT, targetWindowHandle)
            Case "right"
                SendKeyPress(VK_RIGHT, targetWindowHandle)
            Case "f5"
                SendKeyPress(VK_F5, targetWindowHandle)
            Case "pageup"
                SendKeyPress(VK_PAGEUP, targetWindowHandle)
            Case "pagedown"
                SendKeyPress(VK_PAGEDOWN, targetWindowHandle)
            Case "alt_left"
                SendKeyPressAlt(VK_LEFT, targetWindowHandle)
            Case "alt_right"
                SendKeyPressAlt(VK_RIGHT, targetWindowHandle)
            Case "none"
                ' 不执行按键操作，用于数字人等功能
        End Select
    End Sub

    ' 激活指定窗口
    Private Sub ActivateWindow(ByVal hWnd As IntPtr)
        If hWnd = IntPtr.Zero Then
            Return
        End If

        Try
            ' 获取当前前台窗口的线程ID
            Dim foregroundThread As Integer = GetWindowThreadProcessId(GetForegroundWindow(), IntPtr.Zero)
            Dim currentThread As Integer = Thread.CurrentThread.ManagedThreadId

            ' 附加线程以绕过限制
            AttachThreadInput(currentThread, foregroundThread, True)

            ' 检查窗口状态，只对最小化的窗口使用SW_RESTORE
            Dim isMinimized As Boolean = IsIconic(hWnd)

            If isMinimized Then
                ShowWindow(hWnd, SW_RESTORE)
            End If

            ' 设置为前台窗口
            SetForegroundWindow(hWnd)

            ' 分离线程
            AttachThreadInput(currentThread, foregroundThread, False)

            ' 等待窗口激活
            Thread.Sleep(200)
        Catch ex As Exception
            ' 如果出错，至少尝试基本的激活
            Dim isMinimized As Boolean = IsIconic(hWnd)
            If isMinimized Then
                ShowWindow(hWnd, SW_RESTORE)
            End If
            SetForegroundWindow(hWnd)
        End Try
    End Sub

    ' 发送 Alt + Key 组合键
    Private Sub SendKeyPressAlt(ByVal vk As Short, ByVal hWnd As IntPtr)
        Dim inputs(3) As INPUT
        Dim inputSize As Integer = Marshal.SizeOf(GetType(INPUT))

        ' Alt 按下
        inputs(0).type = INPUT_KEYBOARD
        inputs(0).ki.wVk = VK_MENU
        inputs(0).ki.dwFlags = 0
        inputs(0).ki.wScan = 0
        inputs(0).ki.time = 0
        inputs(0).ki.dwExtraInfo = IntPtr.Zero

        ' 目标键按下
        inputs(1).type = INPUT_KEYBOARD
        inputs(1).ki.wVk = vk
        inputs(1).ki.dwFlags = 0
        inputs(1).ki.wScan = 0
        inputs(1).ki.time = 0
        inputs(1).ki.dwExtraInfo = IntPtr.Zero

        ' 目标键释放
        inputs(2).type = INPUT_KEYBOARD
        inputs(2).ki.wVk = vk
        inputs(2).ki.dwFlags = 2 ' KEYEVENTF_KEYUP
        inputs(2).ki.wScan = 0
        inputs(2).ki.time = 0
        inputs(2).ki.dwExtraInfo = IntPtr.Zero

        ' Alt 释放
        inputs(3).type = INPUT_KEYBOARD
        inputs(3).ki.wVk = VK_MENU
        inputs(3).ki.dwFlags = 2 ' KEYEVENTF_KEYUP
        inputs(3).ki.wScan = 0
        inputs(3).ki.time = 0
        inputs(3).ki.dwExtraInfo = IntPtr.Zero

        Dim result As Integer = SendInput(4, inputs(0), inputSize)
    End Sub

    ' 发送按键 - 使用PostMessage直接发送到目标窗口
    Private Sub SendKeyPress(ByVal vk As Short, ByVal hWnd As IntPtr)
        ' 方法1: 使用PostMessage发送WM_KEYDOWN和WM_KEYUP
        Dim keyDownResult As Boolean = PostMessage(hWnd, WM_KEYDOWN, New IntPtr(vk), IntPtr.Zero)
        Dim keyUpResult As Boolean = PostMessage(hWnd, WM_KEYUP, New IntPtr(vk), IntPtr.Zero)

        ' 如果PostMessage成功，就不需要用SendInput了
        If keyDownResult AndAlso keyUpResult Then
            Return
        End If

        ' 方法2: 使用SendInput（备用）
        Dim inputs(1) As INPUT
        Dim inputSize As Integer = Marshal.SizeOf(GetType(INPUT))

        ' 按键按下
        inputs(0).type = INPUT_KEYBOARD
        inputs(0).ki.wVk = vk
        inputs(0).ki.dwFlags = 0
        inputs(0).ki.wScan = 0
        inputs(0).ki.time = 0
        inputs(0).ki.dwExtraInfo = IntPtr.Zero

        ' 按键释放
        inputs(1).type = INPUT_KEYBOARD
        inputs(1).ki.wVk = vk
        inputs(1).ki.dwFlags = 2 ' KEYEVENTF_KEYUP
        inputs(1).ki.wScan = 0
        inputs(1).ki.time = 0
        inputs(1).ki.dwExtraInfo = IntPtr.Zero

        Dim result As Integer = SendInput(2, inputs(0), inputSize)
    End Sub

    ' 获取窗口类名
    Private Shared Function GetWindowClassName(ByVal hWnd As IntPtr) As String
        If hWnd = IntPtr.Zero Then
            Return ""
        End If

        Try
            Dim className As New StringBuilder(256)
            Dim result As Integer = GetClassName(hWnd, className, className.Capacity)
            If result = 0 Then
                Return ""
            End If
            Return className.ToString()
        Catch ex As Exception
            Return ""
        End Try
    End Function

End Class
