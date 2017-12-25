Module Demo

    Sub Main()
        Dim url As String = "https://devweb-39812297.ap-northeast-1.elb.amazonaws.com/workflow/"

        Dim ws As New WorkflowService(url, "1111", "123456")

        'Call get token - example
        Dim getToken As Task(Of TokenResult) = ws.GetToken()
        getToken.Wait()
        Dim result = getToken.Result
        If Not result Is Nothing Then
            Console.Write("GET TOKEN: ")
            Console.WriteLine(result.access_token)
        End If

        'Call create request
        'Dim request = create(ws)
        'If Not request Is Nothing Then
        '    'Update
        '    update(ws, request)

        '    'Delete
        '    Dim deleteRequest As Task(Of Boolean) = ws.DeleteRequest(request.request.requestId)
        '    deleteRequest.Wait()
        '    If deleteRequest.Result Then
        '        Console.Write("DELETE REQUEST: ")
        '        Console.WriteLine(request.request.requestId)
        '    End If
        'End If

        'Update
        'Dim temp As New RequestInfo()
        'temp.request = New Request()
        'temp.request.requestId = 500
        'update(ws, temp)

        'Delete
        'Dim deleteRequest As Task(Of Boolean) = ws.DeleteRequest(500)
        'deleteRequest.Wait()
        'If deleteRequest.Result Then
        '    Console.Write("DELETE REQUEST: ")
        '    Console.WriteLine(500)
        'End If

        'Get list request
        Dim requestids As New List(Of Id)
        requestids.Add(New Id(1417))
        Dim getlistrequest As Task(Of ListRequest) = ws.GetRequestStatus(requestids)
        If Not getlistrequest Is Nothing Then
            Console.WriteLine("list request: ")
            For Each item As Request In getlistrequest.Result.list
                Console.WriteLine("requestid: " + item.requestId.ToString + "/ status: " + item.requestStatus.ToString)
            Next
        End If
        Console.ReadKey(True)
    End Sub

    Private Function create(ByRef ws As WorkflowService) As RequestInfo
        Dim requestCreation As New RequestCreation()
        requestCreation.requestFormDetailRelaId = "NGHIATEST"
        requestCreation.organizationRelaId = "ID11133"
        requestCreation.amount = 0
        requestCreation.conditionId = 0
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
        requestUpdate.formData = ""


        Dim process As New RequestProcess()
        process.requestStep = 1
        process.staffId = 10
        process.orgId = 1
        process.posId = 3
        requestUpdate.process = New List(Of RequestProcess)
        requestUpdate.process.Add(process)

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

End Module
