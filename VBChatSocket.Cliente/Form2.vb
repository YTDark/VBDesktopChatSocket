Imports System.Net.Sockets
Imports System.Text
Imports System.Threading

Public Class Form2
    Dim clientSocket As New TcpClient
    Dim serverStream As NetworkStream
    Dim readData As String
    Private Async Sub Button1_Click(sender As Object, e As EventArgs) Handles Button1.Click
        readData = "Conectado ao servidor"
        Dim ctlThread As New Thread(New ThreadStart(AddressOf msg))
        ctlThread.Start()
        Await clientSocket.ConnectAsync("127.0.0.1", 8888)
        serverStream = clientSocket.GetStream()
        Dim outStream As Byte() = Encoding.UTF8.GetBytes(TextBox1.Text + "$")
        serverStream.Write(outStream, 0, outStream.Length)
        Await serverStream.FlushAsync()
        Dim ctThread As New Thread(New ThreadStart(AddressOf getMessage))
        ctThread.Start()
        TextBox1.ReadOnly = True
        TextBox3.Enabled = True
        Button2.Enabled = True
        Button1.Enabled = False
        Me.AcceptButton = Button2
        TextBox3.Focus()
        Button1.Text = "Conectado"
    End Sub

    Private Async Sub getMessage()
        While True
            Try
                serverStream = clientSocket.GetStream()
                Dim buffSize As Integer = 0
                Dim inStream(100025) As Byte
                buffSize = clientSocket.ReceiveBufferSize
                Await serverStream.ReadAsync(inStream, 0, buffSize)
                Dim returnData = Encoding.UTF8.GetString(inStream)
                readData = "" + returnData
                Dim ctThread As New Thread(New ThreadStart(AddressOf msg))
                ctThread.Start()
            Catch ex As Exception

            End Try
        End While
    End Sub

    Private Sub msg()
        If (Me.InvokeRequired) Then
            Invoke(New MethodInvoker(AddressOf msg))
        Else
            TextBox2.Text = TextBox2.Text + Environment.NewLine + " >> " + readData
        End If
    End Sub

    Private Sub Form2_Load(sender As Object, e As EventArgs) Handles MyBase.Load

    End Sub

    Private Async Sub Button2_Click(sender As Object, e As EventArgs) Handles Button2.Click
        Dim outStream As Byte() = Encoding.UTF8.GetBytes(TextBox3.Text + "$")
        If outStream.Length <> 1 Then
            Await serverStream.WriteAsync(outStream, 0, outStream.Length)
            Await serverStream.FlushAsync()
            TextBox3.Text = ""
            TextBox3.Focus()
        End If
    End Sub
End Class