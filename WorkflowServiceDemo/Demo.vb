Imports Newtonsoft.Json

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
        Dim listRequest As Task(Of List(Of Request)) = ws.GetRequestStatus(requestids)
        If Not listRequest Is Nothing Then
            Console.WriteLine("List request: ")
            For Each item As Request In listRequest.Result
                Console.WriteLine("requestid: " + item.requestId.ToString + "/ status: " + item.requestStatus.ToString)
            Next
        End If
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


End Module
