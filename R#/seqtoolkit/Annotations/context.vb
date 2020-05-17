﻿Imports System.Text
Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.Scripting.MetaData
Imports SMRUCC.genomics.ComponentModel.Annotation
Imports SMRUCC.genomics.ComponentModel.Loci
Imports SMRUCC.genomics.SequenceModel
Imports SMRUCC.genomics.SequenceModel.FASTA
Imports SMRUCC.genomics.SequenceModel.NucleotideModels
Imports SMRUCC.Rsharp.Runtime
Imports SMRUCC.Rsharp.Runtime.Internal
Imports SMRUCC.Rsharp.Runtime.Internal.Object
Imports SMRUCC.Rsharp.Runtime.Interop
Imports IContext = SMRUCC.genomics.ContextModel.Context

<Package("annotation.genomics_context", Category:=APICategories.ResearchTools)>
Module context

    Sub New()
        Call generic.add("summary", GetType(IContext), AddressOf contextSummary)
    End Sub

    Private Function contextSummary(x As IContext, args As list, env As Environment) As Object
        Dim sb As New StringBuilder
        Dim nt As FastaSeq = args.getValue(Of FastaSeq)("nt", env)

        Call sb.AppendLine($"summary of {x.tag}:")
        Call sb.AppendLine($"current feature: {x.feature.ToString}")
        Call sb.AppendLine($"distance: {x.distance} bp")
        Call sb.AppendLine()
        Call sb.AppendLine($"upstream location in given distance: {x.upstream.ToString}")
        If Not nt Is Nothing Then
            Call sb.AppendLine(nt.CutSequenceLinear(x.upstream).SequenceData)
        End If
        Call sb.AppendLine($"downstream location in given distance: {x.downstream.ToString}")
        If Not nt Is Nothing Then
            Call sb.AppendLine(nt.CutSequenceLinear(x.downstream).SequenceData)
        End If
        Call sb.AppendLine($"complement strand location of current: {x.antisense.ToString}")
        If Not nt Is Nothing Then
            Call sb.AppendLine(nt.CutSequenceLinear(x.antisense).SequenceData)
        End If

        Return sb.ToString
    End Function

    <ExportAPI("location")>
    Public Function location(left As Integer, right As Integer, Optional strand As Object = Nothing) As Object
        Dim strVal As Strands

        If strand Is Nothing Then
            strVal = Strands.Unknown
        ElseIf TypeOf strand Is Strands Then
            strVal = strand
        Else
            strVal = Scripting.ToString(strand).GetStrand
        End If

        Return New NucleotideLocation(left, right, strVal)
    End Function

    <ExportAPI("context")>
    <RApiReturn(GetType(IContext))>
    Public Function context(loci As Object, distance As Integer, Optional note As String = Nothing, Optional env As Environment = Nothing) As Object
        Dim pos As NucleotideLocation

        If loci Is Nothing Then
            Return Nothing
        ElseIf TypeOf loci Is IGeneBrief Then
            pos = DirectCast(loci, IGeneBrief).Location
            note = If(note, loci.ToString)
        ElseIf TypeOf loci Is Contig Then
            pos = DirectCast(loci, Contig).MappingLocation
            note = If(note, loci.ToString)
        ElseIf TypeOf loci Is NucleotideLocation Then
            pos = loci
        Else
            Return debug.stop(New InvalidCastException(loci.GetType.FullName), env)
        End If

        Return New IContext(pos, distance, note)
    End Function

    Public Function getNtLocation(x As Object) As NucleotideLocation
        If x Is Nothing Then
            Return Nothing
        ElseIf TypeOf x Is IGeneBrief Then
            Return DirectCast(x, IGeneBrief).Location
        ElseIf TypeOf x Is Contig Then
            Return DirectCast(x, Contig).MappingLocation
        ElseIf TypeOf x Is NucleotideLocation Then
            Return x
        Else
            Return Nothing
        End If
    End Function

    <ExportAPI("relationship")>
    <RApiReturn(GetType(SegmentRelationships))>
    Public Function relationship(a As Object, b As Object, Optional env As Environment = Nothing) As Object
        Dim loci1 As NucleotideLocation = getNtLocation(a)
        Dim loci2 As NucleotideLocation = getNtLocation(b)

        If loci1 Is Nothing AndAlso Not a Is Nothing Then
            Return debug.stop(New InvalidCastException(a.GetType.FullName), env)
        ElseIf loci2 Is Nothing AndAlso Not b Is Nothing Then
            Return debug.stop(New InvalidCastException(b.GetType.FullName), env)
        End If

        If loci1 Is Nothing OrElse loci2 Is Nothing Then
            Return SegmentRelationships.Blank
        Else
            Return LocusExtensions.GetRelationship(loci1, loci2)
        End If
    End Function

End Module