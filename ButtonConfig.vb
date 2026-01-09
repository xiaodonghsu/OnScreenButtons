Imports System.IO
Imports System.Text.Json
Imports System.Text.Json.Serialization

Public Class ButtonConfig
    Public Property Configs As List(Of WindowConfig)
    Public Property DefaultButtons As List(Of ButtonInfo)

    Public Class WindowConfig
        Public Property WindowClassName As String
        Public Property Buttons As List(Of ButtonInfo)
    End Class

    Public Class ButtonInfo
        Public Property Name As String
        Public Property DisplayName As String
        Public Property KeyAction As String
    End Class

    Public Shared Function LoadFromFile(filePath As String) As ButtonConfig
        Try
            If Not File.Exists(filePath) Then
                ' 创建默认配置文件
                CreateDefaultConfigFile(filePath)
            End If

            Dim json As String = File.ReadAllText(filePath)

            ' 配置JsonSerializerOptions以支持大小写不敏感
            Dim options As New JsonSerializerOptions()
            options.PropertyNameCaseInsensitive = True

            Dim config As ButtonConfig = JsonSerializer.Deserialize(Of ButtonConfig)(json, options)
            ' 如果反序列化失败或返回 Nothing，使用默认配置
            If config Is Nothing Then
                ' 创建默认配置文件
                CreateDefaultConfigFile(filePath)
                Return LoadFromFile(filePath) ' 重新加载
            End If

            ' 确保默认按钮不为空
            If config.DefaultButtons Is Nothing Then
                config.DefaultButtons = GetDefaultButtonList()
            End If

            ' 确保配置列表不为空
            If config.Configs Is Nothing Then
                config.Configs = New List(Of WindowConfig)()
            End If

            Return config
        Catch ex As Exception
            ' 如果加载失败，创建默认配置文件并重新加载
            CreateDefaultConfigFile(filePath)
            Return LoadFromFile(filePath)
        End Try
    End Function

    Public Function GetButtonsForWindow(windowClassName As String) As List(Of ButtonInfo)
        If Configs IsNot Nothing Then
            For Each config As WindowConfig In Configs
                If config.WindowClassName IsNot Nothing AndAlso
                   config.WindowClassName.Equals(windowClassName, StringComparison.OrdinalIgnoreCase) Then
                    If config.Buttons IsNot Nothing Then
                        Return config.Buttons
                    End If
                End If
            Next
        End If

        ' 返回默认按钮，如果默认按钮为空则返回空列表
        If DefaultButtons IsNot Nothing Then
            Return DefaultButtons
        End If

        Return New List(Of ButtonInfo)()
    End Function

    Private Shared Function GetDefaultButtonList() As List(Of ButtonInfo)
        Return New List(Of ButtonInfo) From {
            New ButtonInfo With {
                .Name = "prev",
                .DisplayName = "向左",
                .KeyAction = "left"
            },
            New ButtonInfo With {
                .Name = "next",
                .DisplayName = "向右",
                .KeyAction = "right"
            },
            New ButtonInfo With {
                .Name = "digitalhuman",
                .DisplayName = "数字人",
                .KeyAction = "none"
            }
        }
    End Function

    Private Shared Sub CreateDefaultConfigFile(filePath As String)
        Dim defaultConfig As New ButtonConfig()
        defaultConfig.Configs = New List(Of WindowConfig)()
        defaultConfig.DefaultButtons = GetDefaultButtonList()

        Dim options As New JsonSerializerOptions()
        options.WriteIndented = True
        options.PropertyNamingPolicy = JsonNamingPolicy.CamelCase ' 使用驼峰命名
        Dim json As String = JsonSerializer.Serialize(defaultConfig, options)

        ' 确保目录存在
        Dim directory As String = Path.GetDirectoryName(filePath)
        If Not String.IsNullOrEmpty(directory) Then
            Dim dirInfo As New System.IO.DirectoryInfo(directory)
            If Not dirInfo.Exists Then
                dirInfo.Create()
            End If
        End If

        File.WriteAllText(filePath, json)
    End Sub
End Class
