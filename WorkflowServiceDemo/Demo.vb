Imports Newtonsoft.Json
Imports Amazon.S3
Imports System.Net
Imports System.IO

Module Demo

    Sub Main()
        'Url của server API 
        Dim url As String = "https://dev.e2-cloud.jp/workflow/"
        url = "http://localhost:8080"
        'Secret_id/secret key, được lưu trong bảng oauth_client_details
        'Secret_id chính là company code
        Dim ws As New WorkflowService(url, "1234", "123456")

        'Call get token - example
        'Lấy token tương ứng với secret_id/secret_key
        Dim getToken As Task(Of TokenResult) = ws.GetToken()
        getToken.Wait()
        Dim result = getToken.Result
        If Not result Is Nothing Then
            Console.Write("GET TOKEN: ")
            Console.WriteLine(result.access_token)
        End If

        ''Call create request
        ''Tạo request, bỏ cmt là chạy.
        'create(ws)

        'Dim requestInfo As Task(Of RequestInfo)

        ''Update
        ''Update request, trước khi update thì lấy thông tin request tương ứng
        ''569 ở đây là id của request cần lấy thông tin
        'requestInfo = ws.GetRequest(569)
        'If Not requestInfo.Result Is Nothing Then
        ''Update thông tin request
        '    update(ws, requestInfo.Result)
        'End If

        'Apply
        ''Apply request, trước khi apply cũng lấy thông tin request tương ứng
        'requestInfo = ws.GetRequest(571)
        'If Not requestInfo.Result Is Nothing Then
        ''Apply request, có sử dụng formData
        '    apply(ws, requestInfo.Result)
        'End If

        'Delete
        ''Delete request với id truyền vào.
        'Dim deleteRequest As Task(Of Boolean) = ws.DeleteRequest(569)
        'deleteRequest.Wait()
        'If deleteRequest.Result Then
        '    Console.Write("DELETE REQUEST: ")
        '    Console.WriteLine(569)
        'End If

        'Get list request
        ''Lấy status của list request
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
    ''Upload file gồm 2 bước:
    ''Bước 1: Get signedurl
    ''Bước 2: upload file sử dụng signedurl
        uploadFile(570, "attach-file-preview", "nghiant12345", "20180227_e2move連携API説明15.txt", "D:\20180227_e2move連携API説明15.txt", ws)

        'View file
        'viewFile(570, "attach-file-preview", "nghiant12345", "mitani_aws.txt", "D:\mitani_aws.txt", ws)

        'Delete file
        'deleteFile(570, "attach-file-preview", "nghiant12345", "mitani_aws.txt", "D:\mitani_aws.txt", ws)

        Console.ReadKey(True)
    End Sub

    Private Function create(ByRef ws As WorkflowService) As RequestInfo
        Dim requestCreation As New RequestCreation()
    'Set trị các tham số khi tạo request
        requestCreation.requestFormDetailRelaId = "e2move-detail"
        requestCreation.organizationRelaId = "組織連携123"
        requestCreation.amount = 1000
        requestCreation.conditionNumber = 1
        requestCreation.userName = "akimizu"

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
    'Set trị các tham số khi update request
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
'Set trị các tham số khi apply request
        requestUpdate.requestStatus = 2
        requestUpdate.subject = "TEST"
'Form data được truyền lên theo format: [{"name":"control-name1","value":"value1"}, {"name":"control-name2","value":"value2"}]
'Xử lý gán trị khi lấy thông tin control name từ formDesign
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
'Parse formDesign thành list, mỗi phần tử có name và value
        Dim formDataControls = JsonConvert.DeserializeObject(Of List(Of FormDataControl))(formData)
        For Each control As FormDataControl In formDataControls
'Gán value cho control tương ứng
            control.value = "test-value" + Rnd().ToString
        Next
'Parse lại thành string
        Return JsonConvert.SerializeObject(formDataControls)
    End Function

    Private Sub uploadFile(ByVal requestId As Long, ByVal controlName As String, ByVal userName As String, ByVal fileName As String, ByVal filePath As String, ByRef ws As WorkflowService)
        Dim signedUrl As Task(Of String)
'Get signedurl
        signedUrl = ws.GetSignedUrl(requestId, controlName, fileName, userName, "1")
        signedUrl.Wait()
        If Not signedUrl Is Nothing Then
'Upload file sử dụng signedurl
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
'Get signedurl
        signedUrl = ws.GetSignedUrl(requestId, controlName, fileName, userName, "0")
        signedUrl.Wait()
        If Not signedUrl Is Nothing Then
            Console.WriteLine("LINK FILE: " + signedUrl.Result)
        End If
    End Sub

    Private Sub deleteFile(ByVal requestId As Long, ByVal controlName As String, ByVal userName As String, ByVal fileName As String, ByVal filePath As String, ByRef ws As WorkflowService)
        Dim signedUrl As Task(Of String)
'Get signedurl
        signedUrl = ws.GetSignedUrl(requestId, controlName, fileName, userName, "2")
        signedUrl.Wait()
        If Not signedUrl Is Nothing Then
'Delete file sử dụng signedurl
            Dim httpRequest As HttpWebRequest = CType(WebRequest.Create(signedUrl.Result), HttpWebRequest)
            httpRequest.Method = "DELETE"
            Dim response As HttpWebResponse = CType(httpRequest.GetResponse(), HttpWebResponse)
            Console.WriteLine("DELETE FILE: " + response.StatusCode.ToString)
        End If
    End Sub

End Module
