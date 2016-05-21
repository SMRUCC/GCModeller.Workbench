﻿Imports System.Runtime.InteropServices

''' <summary>
''' 
''' </summary>
''' <remarks>似乎只能够x86编译</remarks>
Public Class DrawSVG

    ''' <summary>
    ''' The install directory of GIMP library/software
    ''' </summary>
    ''' <returns></returns>
    Public ReadOnly Property GIMP As String

    Sub New(bin As String)
        GIMP = bin
    End Sub

    Public Overrides Function ToString() As String
        Return GIMP
    End Function

    <DllImport("kernel32.dll", SetLastError:=True)>
    Public Shared Function SetDllDirectory(DIR As String) As Boolean
    End Function

    <DllImport("libgobject-2.0-0.dll", EntryPoint:="g_type_init", SetLastError:=True)>
    Public Shared Sub GraphicsTypeInit()
    End Sub

    ' http://stackoverflow.com/questions/2390407/pinvokestackimbalance-c-sharp-call-to-unmanaged-c-function

    ' It's good.I update function define as follow:
    ' [DllImport("mydll.dll", CallingConvention = CallingConvention.Cdecl)]
    ' It works well.

    <DllImport("librsvg-2-2.dll",
               EntryPoint:="rsvg_pixbuf_from_file_at_size",
               CallingConvention:=CallingConvention.Cdecl,
               CharSet:=CharSet.Ansi,
               SetLastError:=True)>
    Public Shared Function ReadsvgPixbufFromFileWithSize(path As String, width As Integer, height As Integer, <Out> ByRef [Error] As IntPtr) As IntPtr
    End Function

    <DllImport("libgdk_pixbuf-2.0-0.dll",
               EntryPoint:="gdk_pixbuf_save",
               CallingConvention:=CallingConvention.Cdecl,
               CharSet:=CharSet.Ansi)>
    Public Shared Function SaveGdkPixbuf(pixbuf As IntPtr, filename As String, type As String, <Out> ByRef [Error] As IntPtr, __arglist As Object) As Boolean
    End Function

    ''' <summary>
    ''' 
    ''' </summary>
    ''' <param name="svg">SVG inputs file path</param>
    ''' <param name="png">PNG output path</param>
    Public Sub RasterizeSvg(svg As String, png As String)
        Dim callSuccessful As Boolean = SetDllDirectory(GIMP)

        If Not callSuccessful Then
            Throw New Exception("Could not set DLL directory")
        End If

        Call GraphicsTypeInit()

        Dim [error] As IntPtr
        Dim result As IntPtr = ReadsvgPixbufFromFileWithSize(svg, -1, -1, [error])

        If [error] <> IntPtr.Zero Then
            Throw New Exception(Marshal.ReadInt32([error]).ToString())
        End If

        callSuccessful = SaveGdkPixbuf(result, png, "png", [error], Nothing)

        If Not callSuccessful Then
            Throw New Exception([error].ToInt32().ToString())
        End If
    End Sub
End Class
