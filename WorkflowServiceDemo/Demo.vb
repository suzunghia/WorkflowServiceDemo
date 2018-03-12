Imports Newtonsoft.Json
Imports Amazon.S3
Imports System.Net
Imports System.IO

Module Demo

    Sub Main()
        Dim url As String = "https://dev.e2-cloud.jp/workflow/"
        url = "http://localhost:8080"
        Dim ws As New WorkflowService(url, "1234", "123456")

        'Call get token - example
        Dim getToken As Task(Of TokenResult) = ws.GetToken()
        getToken.Wait()
        Dim result = getToken.Result
        If Not result Is Nothing Then
            Console.Write("GET TOKEN: ")
            Console.WriteLine(result.access_token)
        End If

        'Call create request
        'create(ws)

        'Dim requestInfo As Task(Of RequestInfo)

        'Update
        'requestInfo = ws.GetRequest(569)
        'If Not requestInfo.Result Is Nothing Then
        '    update(ws, requestInfo.Result)
        'End If

        'Apply
        'requestInfo = ws.GetRequest(571)
        'If Not requestInfo.Result Is Nothing Then
        '    apply(ws, requestInfo.Result)
        'End If

        'Delete
        'Dim deleteRequest As Task(Of Boolean) = ws.DeleteRequest(569)
        'deleteRequest.Wait()
        'If deleteRequest.Result Then
        '    Console.Write("DELETE REQUEST: ")
        '    Console.WriteLine(569)
        'End If

        'Get list request
        Dim requestids As New List(Of Id)
        requestids.Add(New Id(569))
        requestids.Add(New Id(570))
        requestids.Add(New Id(571))
        Dim listrequest As Task(Of List(Of Request)) = ws.GetRequestStatus(requestids)
        If Not listrequest Is Nothing Then
            Console.WriteLine("list request: ")
            For Each item As Request In listrequest.Result
                Console.WriteLine("requestid: " + item.requestId.ToString + "/ status: " + item.requestStatus.ToString)
            Next
        End If

        'Upload file
        uploadFile(570, "attach-file-preview", "nghiant12345", "20180227_e2move連携API説明15.txt", "D:\20180227_e2move連携API説明15.txt", ws)

        'View file
        'viewFile(570, "attach-file-preview", "nghiant12345", "mitani_aws.txt", "D:\mitani_aws.txt", ws)

        'Delete file
        'deleteFile(570, "attach-file-preview", "nghiant12345", "mitani_aws.txt", "D:\mitani_aws.txt", ws)

        Console.ReadKey(True)
    End Sub

    Private Function create(ByRef ws As WorkflowService) As RequestInfo
        Dim requestCreation As New RequestCreation()
        requestCreation.requestFormDetailRelaId = "e2move-detail"
        requestCreation.organizationRelaId = "組織連携123"
        requestCreation.amount = 1000
        requestCreation.conditionNumber = 1
        requestCreation.userName = "akimizu"

        requestCreation.requestFormDetailRelaId = "e2move-detail"
        requestCreation.organizationRelaId = "E2-MOVE-ORG-2"
        requestCreation.amount = 1000
        requestCreation.conditionNumber = 1
        requestCreation.userName = "nghiant12345"

        Dim createRequest As Task(Of RequestInfo) = ws.CreateRequest(requestCreation)
        createRequest.Wait()
        Dim result = createRequest.Result
        If Not result Is Nothing Then
            Console.Write("CREATE REQUEST: ")
            Console.WriteLine(result.request.requestId)
            Return result
        End If
        Return Nothing
    End Function

    Private Function update(ByRef ws As WorkflowService, ByVal request As RequestInfo) As RequestInfo
        Dim requestUpdate As New RequestUpdate()
        requestUpdate.requestStatus = 2
        requestUpdate.subject = "TEST"
        requestUpdate.formData = updateFormData(request.request.formDesign)

        Dim temp As Staff
        For Each item As RequestProcess In request.process
            temp = item.staffs(0)
            item.staffId = temp.staffId
            item.orgId = temp.orgId
            item.posId = temp.posId
        Next
        requestUpdate.process = request.process

        Dim updateRequest As Task(Of RequestInfo) = ws.UpdateRequest(request.request.requestId, requestUpdate)
        updateRequest.Wait()
        Dim result = updateRequest.Result
        If Not result Is Nothing Then
            Console.Write("UPDATE REQUEST: ")
            Console.WriteLine(result.request.requestId)
            Return result
        End If
        Return Nothing
    End Function

    Private Function apply(ByRef ws As WorkflowService, ByVal request As RequestInfo) As Request
        Dim requestUpdate As New RequestUpdate()
        requestUpdate.requestStatus = 2
        requestUpdate.subject = "TEST"
        requestUpdate.formData = updateFormData(request.request.formDesign)

        Dim temp As Staff
        For Each item As RequestProcess In request.process
            temp = item.staffs(0)
            item.staffId = temp.staffId
            item.orgId = temp.orgId
            item.posId = temp.posId
        Next
        requestUpdate.process = request.process

        Dim updateRequest As Task(Of Request) = ws.ApplyRequest(request.request.requestId, requestUpdate)
        updateRequest.Wait()
        Dim result = updateRequest.Result
        If Not result Is Nothing Then
            Console.Write("APPLY REQUEST: ")
            Console.WriteLine(result.requestId)
            Return result
        End If
        Return Nothing
    End Function

    Private Function updateFormData(ByVal formData As String) As String
        Dim formDataControls = JsonConvert.DeserializeObject(Of List(Of FormDataControl))(formData)
        For Each control As FormDataControl In formDataControls
            control.value = "test-value" + Rnd().ToString
        Next
        Return JsonConvert.SerializeObject(formDataControls)
    End Function

    Private Sub uploadFile(ByVal requestId As Long, ByVal controlName As String, ByVal userName As String, ByVal fileName As String, ByVal filePath As String, ByRef ws As WorkflowService)
        Dim signedUrl As Task(Of String)
        signedUrl = ws.GetSignedUrl(requestId, controlName, fileName, userName, "1")
        signedUrl.Wait()
        If Not signedUrl Is Nothing Then
            Dim httpRequest As HttpWebRequest = CType(WebRequest.Create(signedUrl.Result), HttpWebRequest)
            httpRequest.Method = "PUT"
            Using dataStream As Stream = httpRequest.GetRequestStream()
                Dim buffer(8000) As Byte
                Using fileStream As FileStream = New FileStream(filePath, FileMode.Open, FileAccess.Read)
                    Dim bytesRead As Int32 = 0
                    Do
                        bytesRead = fileStream.Read(buffer, 0, buffer.Length)
                        If bytesRead > 0 Then
                            dataStream.Write(buffer, 0, bytesRead)
                        End If
                    Loop Until bytesRead = 0
                End Using
            End Using
            Dim response As HttpWebResponse = CType(httpRequest.GetResponse(), HttpWebResponse)
            Console.WriteLine("UPLOAD FILE: " + response.StatusCode.ToString)
        End If
    End Sub

    Private Sub viewFile(ByVal requestId As Long, ByVal controlName As String, ByVal userName As String, ByVal fileName As String, ByVal filePath As String, ByRef ws As WorkflowService)
        Dim signedUrl As Task(Of String)
        signedUrl = ws.GetSignedUrl(requestId, controlName, fileName, userName, "0")
        signedUrl.Wait()
        If Not signedUrl Is Nothing Then
            Console.WriteLine("LINK FILE: " + signedUrl.Result)
        End If
    End Sub

    Private Sub deleteFile(ByVal requestId As Long, ByVal controlName As String, ByVal userName As String, ByVal fileName As String, ByVal filePath As String, ByRef ws As WorkflowService)
        Dim signedUrl As Task(Of String)
        signedUrl = ws.GetSignedUrl(requestId, controlName, fileName, userName, "2")
        signedUrl.Wait()
        If Not signedUrl Is Nothing Then
            Dim httpRequest As HttpWebRequest = CType(WebRequest.Create(signedUrl.Result), HttpWebRequest)
            httpRequest.Method = "DELETE"
            Dim response As HttpWebResponse = CType(httpRequest.GetResponse(), HttpWebResponse)
            Console.WriteLine("DELETE FILE: " + response.StatusCode.ToString)
        End If
    End Sub

End Module
