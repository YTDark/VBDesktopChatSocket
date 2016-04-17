Imports System.Net.Sockets
Imports System.Text
Imports System.Threading

Module Module1
    Public clientsList As New Hashtable()
    Sub Main()
        Dim IP As New Net.IPAddress(New Byte() {127, 0, 0, 1})
        Dim serverSocket As New TcpListener(IP, 8888)
        Dim clientSocket As New TcpClient()
        Dim counter As Integer = 0
        serverSocket.Start()
        Console.WriteLine("Servidor de chat inicializado")
        counter = 0
        While True
            startServer(counter, clientSocket, serverSocket)
        End While
        clientSocket.Close()
        serverSocket.Stop()
        Console.WriteLine("Encerrado")
        Console.ReadLine()
    End Sub

    Private Async Sub startServer(counter As Integer, clientSocket As TcpClient, serverSocket As TcpListener)
        counter += 1
        clientSocket = Await serverSocket.AcceptTcpClientAsync()
        Dim bytesFrom(100025) As Byte
        Dim networkStream = clientSocket.GetStream()
        Await networkStream.ReadAsync(bytesFrom, 0, Convert.ToInt32(clientSocket.ReceiveBufferSize))
        Dim dataFromClient As String = Encoding.UTF8.GetString(bytesFrom)
        dataFromClient = dataFromClient.Substring(0, dataFromClient.IndexOf("$"))
        If NomeUnico(clientsList, dataFromClient) = True Then
            clientsList.Add(dataFromClient, clientSocket)
        Else
            Dim rnd As New Random
            dataFromClient = dataFromClient + rnd.Next(1, 2046).ToString()
            clientsList.Add(dataFromClient, clientSocket)
        End If
        Dim broadcaster(3) As Object
        broadcaster(0) = dataFromClient + " Entrou"
        broadcaster(1) = dataFromClient
        broadcaster(2) = False
        Dim client As New handleClient()
        Dim ctThread As New Thread(AddressOf Broadcast)
        ctThread.Start(broadcaster)
        Console.WriteLine(dataFromClient + " entrou na sala de chat")
        client.startClient(clientSocket, dataFromClient, clientsList)
        Await networkStream.FlushAsync()
    End Sub

    Private Function NomeUnico(clientsList As Hashtable, dataFromClient As String) As Boolean
        Dim count As Integer = 0
        For Each client As DictionaryEntry In clientsList
            If (client.Key = dataFromClient) Then
                count += 1
            End If
        Next
        If (count > 0) Then
            Return False
        Else
            Return True
        End If
        Return True
    End Function

    Private Async Sub Broadcast(ByVal broadcaster As Object)
        For Each Item As DictionaryEntry In clientsList
            Dim broadcastSocket As TcpClient = Item.Value
            Dim broadcastStream = broadcastSocket.GetStream()
            Dim broadcastBytes As Byte()
            If (broadcaster(2)) Then
                broadcastBytes = Encoding.UTF8.GetBytes(broadcaster(1) + " diz " + "às " + Date.Now.ToLongTimeString() + ": " + broadcaster(0))
            Else
                broadcastBytes = Encoding.UTF8.GetBytes(broadcaster(0))
            End If
            Await broadcastStream.WriteAsync(broadcastBytes, 0, broadcastBytes.Length)
            Await broadcastStream.FlushAsync()
        Next
    End Sub
    Class handleClient
        Dim clientSocket As TcpClient
        Dim clNo As String
        Dim clientslist As Hashtable
        Friend Sub startClient(inClientSocket As TcpClient, clineNo As String, cList As Hashtable)
            clientSocket = inClientSocket
            clNo = clineNo
            clientslist = cList
            Dim ctThread As New Thread(New ThreadStart(AddressOf doChat))
            ctThread.Start()
        End Sub

        Private Async Sub doChat()
            Dim requestCount As Integer = 0
            Dim bytesFrom(100025) As Byte
            Dim dataFromClient As String
            Dim rCount As String
            While clientSocket.Connected
                Try
                    requestCount += 1
                    Dim networkStream = clientSocket.GetStream()
                    Await networkStream.ReadAsync(bytesFrom, 0, Convert.ToInt32(clientSocket.ReceiveBufferSize))
                    dataFromClient = Encoding.UTF8.GetString(bytesFrom)
                    dataFromClient = dataFromClient.Substring(0, dataFromClient.IndexOf("$"))
                    Console.WriteLine("Do cliente - " + clNo + " : " + dataFromClient)
                    rCount = requestCount.ToString()
                    Dim broadcaster(3) As Object
                    broadcaster(0) = dataFromClient
                    broadcaster(1) = clNo
                    broadcaster(2) = True
                    Dim ctThread As New Thread(AddressOf broadcast)
                    ctThread.Start(broadcaster)
                Catch ex As Exception
                    Console.WriteLine(ex.ToString())
                End Try
            End While
        End Sub
    End Class
End Module
