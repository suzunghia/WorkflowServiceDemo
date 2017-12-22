﻿Module Demo

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
        Dim request = create(ws)
        If Not request Is Nothing Then
            'Call update
            'update(ws, request)

            'Call delete
            'Dim deleteRequest As Task(Of Boolean) = ws.DeleteRequest(request.request.requestId)
            'deleteRequest.Wait()
            'If deleteRequest.Result Then
            'Console.Write("DELETE REQUEST: ")
            '   Console.WriteLine(request.request.requestId)
            'End If
        End If

        'Get list request
        Dim requestIds As New List(Of Id)
        requestIds.Add(New Id(1234))
        Dim getListRequest As Task(Of List(Of Request)) = ws.GetRequestStatus(requestIds)
        If Not getListRequest Is Nothing Then
            Console.WriteLine("LIST REQUEST: ")
            For Each item As Request In getListRequest.Result
                Console.WriteLine("requestId: " + item.requestId + "/status:" + item.requestStatus)
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
        requestUpdate.process = request.process
        requestUpdate.process(0).staffId = 10
        requestUpdate.process(0).orgId = 1
        requestUpdate.process(0).posId = 3

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