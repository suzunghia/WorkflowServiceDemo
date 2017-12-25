Public Class RequestProcess
    Public requestProcessId As Long?
    Public requestRouteDetailId As Long?
    Public requestStep As Integer?
    Public approveStatus As Integer?
    Public allowSkip As Integer?
    Public skipByUser As Integer?
    Public approvalType As Integer?
    Public delegateApprovalFlag As Integer?
    Public label As String
    Public description As String

    Public skipFeeFrom As Long?
    Public skipFeeTo As Long?

    Public staffId As Long?
    Public orgId As Long?
    Public posId As Long?
    Public delegateStaffId As Long?
    Public delegateOrgId As Long?
    Public delegatePosId As Long?

    Public staffs As List(Of Staff)
End Class
