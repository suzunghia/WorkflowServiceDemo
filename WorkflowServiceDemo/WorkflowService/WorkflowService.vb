Imports System
Imports System.Text
Imports System.Net
Imports System.Net.Http
Imports System.Net.Http.Headers
Imports System.Threading.Tasks

Public Class WorkflowService
    Public apiServerUrl As String
    Public secretId As String
    Public secretKey As String
    Public accessToken As String
    Public expires As Date

    Private basicAuthen As String
    Private httpClient As HttpClient

    Public Sub New(ByVal apiServerUrl As String, ByVal secretId As String, ByVal secretKey As String)
        Me.apiServerUrl = apiServerUrl
        Me.secretId = secretId
        Me.secretKey = secretKey
        Try
            Dim bytes As Byte() = Encoding.UTF8.GetBytes(Me.secretId + ":" + Me.secretKey)
            Me.basicAuthen = Convert.ToBase64String(bytes)
        Catch ex As Exception
            Me.basicAuthen = ""
        End Try
        httpClient = New HttpClient()
        httpClient.BaseAddress = New Uri(Me.apiServerUrl)
    End Sub


    Public Async Function GetToken() As Task(Of TokenResult)
        If Not String.IsNullOrEmpty(Me.basicAuthen) Then
            httpClient.DefaultRequestHeaders.Clear()
            httpClient.DefaultRequestHeaders.Authorization = New AuthenticationHeaderValue("Basic", Me.basicAuthen)

            'Body
            Dim content As Dictionary(Of String, String) = New Dictionary(Of String, String)
            content.Add("grant_type", "client_credentials")

            Try
                'SSL error
                ServicePointManager.ServerCertificateValidationCallback = AddressOf Me.SetSSL
                'Send
                Dim result As HttpResponseMessage = Await httpClient.PostAsync("oauth/token", New FormUrlEncodedContent(content))
                If (result.IsSuccessStatusCode) Then
                    Dim tokenResult = Await result.Content.ReadAsAsync(Of TokenResult)
                    Me.accessToken = tokenResult.access_token
                    Me.expires = Date.Now.AddSeconds(tokenResult.expires_in)
                    Return tokenResult
                End If
            Catch ex As Exception
                Console.WriteLine(ex)
                Return Nothing
            End Try
        End If
        Return Nothing
    End Function

    Public Async Function CreateRequest(ByVal requestCreation As RequestCreation) As Task(Of RequestInfo)
        'Get token
        If String.IsNullOrEmpty(Me.accessToken) Or Me.expires.CompareTo(Date.Now) < 0 Then
            Dim tokenResult = Me.GetToken()
            If tokenResult Is Nothing Then
                Return Nothing
            End If
        End If

        'Create request
        httpClient.DefaultRequestHeaders.Clear()
        httpClient.DefaultRequestHeaders.Authorization = New AuthenticationHeaderValue("Bearer", Me.accessToken)
        httpClient.DefaultRequestHeaders.Accept.Add(New MediaTypeWithQualityHeaderValue("application/json"))

        Try
            'SSL error
            ServicePointManager.ServerCertificateValidationCallback = AddressOf Me.SetSSL
            'Send
            Dim result = Await httpClient.PostAsJsonAsync("workflow", requestCreation)
            result.EnsureSuccessStatusCode()
            Return Await result.Content.ReadAsAsync(Of RequestInfo)
        Catch ex As Exception
            Console.WriteLine(ex)
            Return Nothing
        End Try
        Return Nothing
    End Function

    Public Async Function UpdateRequest(ByVal requestId As Long, ByVal request As RequestUpdate) As Task(Of RequestInfo)
        'Get token
        If String.IsNullOrEmpty(Me.accessToken) Or Me.expires.CompareTo(Date.Now) < 0 Then
            Dim tokenResult = Me.GetToken()
            If tokenResult Is Nothing Then
                Return Nothing
            End If
        End If


        'Update request
        httpClient.DefaultRequestHeaders.Clear()
        httpClient.DefaultRequestHeaders.Authorization = New AuthenticationHeaderValue("Bearer", Me.accessToken)
        httpClient.DefaultRequestHeaders.Accept.Add(New MediaTypeWithQualityHeaderValue("application/json"))

        Try
            'SSL error
            ServicePointManager.ServerCertificateValidationCallback = AddressOf Me.SetSSL
            'Send
            Dim result = Await httpClient.PutAsJsonAsync("workflow/" + requestId.ToString, request)
            result.EnsureSuccessStatusCode()
            Return Await result.Content.ReadAsAsync(Of RequestInfo)
        Catch ex As Exception
            Console.WriteLine(ex)
            Return Nothing
        End Try
        Return Nothing
    End Function

    Public Async Function DeleteRequest(ByVal requestId As Long) As Task(Of Boolean)
        'Get token
        If String.IsNullOrEmpty(Me.accessToken) Or Me.expires.CompareTo(Date.Now) < 0 Then
            Dim tokenResult = Me.GetToken()
            If tokenResult Is Nothing Then
                Return Nothing
            End If
        End If

        'Delete request
        httpClient.DefaultRequestHeaders.Clear()
        httpClient.DefaultRequestHeaders.Authorization = New AuthenticationHeaderValue("Bearer", Me.accessToken)

        Try
            'SSL error
            ServicePointManager.ServerCertificateValidationCallback = AddressOf Me.SetSSL
            'Send
            Dim result = Await httpClient.DeleteAsync("workflow/admin/" + requestId.ToString)
            result.EnsureSuccessStatusCode()
            Return True
        Catch ex As Exception
            Console.WriteLine(ex)
            Return False
        End Try
        Return False
    End Function

    Public Async Function GetRequestStatus(ByVal requestIds As List(Of Id)) As Task(Of ListRequest)
        'Get token
        If String.IsNullOrEmpty(Me.accessToken) Or Me.expires.CompareTo(Date.Now) < 0 Then
            Dim tokenResult = Me.GetToken()
            If tokenResult Is Nothing Then
                Return Nothing
            End If
        End If

        'Get request
        httpClient.DefaultRequestHeaders.Clear()
        httpClient.DefaultRequestHeaders.Authorization = New AuthenticationHeaderValue("Bearer", Me.accessToken)
        httpClient.DefaultRequestHeaders.Accept.Add(New MediaTypeWithQualityHeaderValue("application/json"))

        Try
            'SSL error
            ServicePointManager.ServerCertificateValidationCallback = AddressOf Me.SetSSL
            'Send
            Dim result = Await httpClient.PostAsJsonAsync("workflows_e2move", requestIds)
            result.EnsureSuccessStatusCode()
            Return Await result.Content.ReadAsAsync(Of ListRequest)
        Catch ex As Exception
            Console.WriteLine(ex)
            Return Nothing
        End Try
        Return Nothing
    End Function

    Public Function SetSSL() As Boolean
        Return True
    End Function


End Class
