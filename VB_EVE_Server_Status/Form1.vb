Option Strict On
Option Explicit On

Imports System.Net
Imports System.Xml
Imports System.Net.NetworkInformation
Imports System.IO

Public Class Form1

    Dim xmlData As XElement
    Dim bStatus As Boolean
    Dim intPlayers As Integer

    Dim host As String = "srv200-g.ccp.cc"
    Dim pingreq As Ping = New Ping()
    Dim timeout As Int32 = 2000

    Dim web As New WebClient()

    Private Sub btnQuery_Click(sender As Object, e As EventArgs) Handles btnQuery.Click
        GetData()
    End Sub

    Private Sub Timer1_Tick(sender As Object, e As EventArgs) Handles Timer1.Tick
        GetData()
    End Sub

    Public Sub GetData()
        Timer1.Enabled = False
        Timer1.Enabled = True

        AddHandler web.DownloadStringCompleted, AddressOf DataGrab
        AddHandler pingreq.PingCompleted, AddressOf EventPingCompleted

        Try
            web.DownloadStringAsync(New Uri("https://api.eveonline.com/server/ServerStatus.xml.aspx/"))
            pingreq.SendAsync(host, timeout)
        Catch ex As Exception
            txbPing.Text = "N/A"
            txbPlayers.Text = "N/A"
            txbStatus.Text = "N/A"
        End Try
    End Sub

    Private Sub btnExit_Click(sender As Object, e As EventArgs) Handles btnExit.Click
        Me.Close()
    End Sub

    Public Sub DataGrab(ByVal sender As Object, ByVal e As DownloadStringCompletedEventArgs)
        RemoveHandler web.DownloadStringCompleted, AddressOf DataGrab

        Dim writer As New StreamWriter("ccp_server.csv", True)
        intPlayers = 0

        Try
            xmlData = XElement.Parse(e.Result)
            bStatus = CBool(xmlData...<serverOpen>.Value)

            If bStatus = True Then
                intPlayers = CInt(xmlData...<onlinePlayers>.Value)
                txbStatus.Text = "Online"
                txbStatus.BackColor = Color.LawnGreen
            Else
                txbStatus.Text = "Offline"
                txbStatus.BackColor = Color.Red
            End If
        Catch ex As Exception
            txbStatus.Text = "Error handling server status data."
        End Try

        txbPlayers.Text = intPlayers.ToString
        writer.WriteLine(Date.Now & "," & intPlayers.ToString())
        writer.Close()
    End Sub

    Public Sub EventPingCompleted(ByVal sender As Object, ByVal e As PingCompletedEventArgs)
        RemoveHandler pingreq.PingCompleted, AddressOf EventPingCompleted

        Try
            If CInt(e.Reply.RoundtripTime.ToString) > 0 Then
                txbPing.Text = e.Reply.RoundtripTime.ToString
            Else
                txbPing.Text = "No Reply"
            End If
        Catch ex As Exception
            txbPing.Text = "Error handling ping data."
        End Try
    End Sub

    Private Sub Form1_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Timer1.Interval = 180000
        txbPlayers.Text = "0"
        web.Proxy() = Nothing
    End Sub
End Class
