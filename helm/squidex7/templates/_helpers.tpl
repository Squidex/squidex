{{/* vim: set filetype=mustache: */}}

{{- define "squidex.selectors" }}
{{- if .Values.selectors.component }}
app.kubernetes.io/component: {{ .Values.selectors.component }}
{{- end}}
app.kubernetes.io/name: {{ include "squidex.name" . }}
app.kubernetes.io/instance: {{ .Release.Name }}  
{{- if .Values.selectors.partOf }}
app.kubernetes.io/part-of: {{ .Values.selectors.partOf }}
{{- end }}
{{- if .Values.selectors.version }}
app.kubernetes.io/version: {{ .Values.selectors.version | quote }}
{{- end -}}
{{- end -}}

{{- define "squidex.labels" }}
{{- include "squidex.selectors" . }}
helm.sh/chart: {{ include "squidex.chart" . }}
app.kubernetes.io/managed-by: {{ .Release.Service }}
{{- if .Values.labels }}
{{- toYaml .Values.labels | nindent 4 }}
{{- end -}}
{{- end -}}

{{- define "squidex.mongoDefaultHostname" }}
  {{- $release := .Release}}
  {{- $clusterSuffix := .Values.clusterSuffix}}
  {{- range $index, $i := until (int (index .Values "mongodb-replicaset").replicas) }}
    {{- $replica := printf "%s-mongodb-replicaset-%d.%s-mongodb-replicaset.%s.svc.%s" $release.Name $i $release.Name $release.Namespace $clusterSuffix }}
    {{- if eq $i 0}}
      {{- $replica }}
    {{- else -}}
      ,{{ $replica }}
    {{- end }}
  {{- end }}
{{- end }}

{{- define "squidex.internalDnsName" }}
  {{- .Release.Name }}-
  {{- include "squidex.name" . }}.
  {{- .Release.Namespace }}.svc.
  {{- .Values.clusterSuffix}}
{{- end }}

{{/*
Create chart name and version as used by the chart label.
*/}}
{{- define "squidex.chart" -}}
{{- printf "%s-%s" .Chart.Name .Chart.Version | replace "+" "_" | trunc 63 | trimSuffix "-" -}}
{{- end -}}

{{- define "squidex.name" -}}
{{- default .Chart.Name .Values.nameOverride | trunc 63 | trimSuffix "-" -}}
{{- end -}}

{{/*
Create a default fully qualified app name.
We truncate at 63 chars because some Kubernetes name fields are limited to this (by the DNS naming spec).
If release name contains chart name it will be used as a full name.
*/}}
{{- define "squidex.fullname" -}}
{{- if .Values.fullnameOverride -}}
{{- .Values.fullnameOverride | trunc 63 | trimSuffix "-" -}}
{{- else -}}
{{- $name := default .Chart.Name .Values.nameOverride -}}
{{- if contains $name .Release.Name -}}
{{- .Release.Name | trunc 63 | trimSuffix "-" -}}
{{- else -}}
{{- printf "%s-%s" .Release.Name $name | trunc 63 | trimSuffix "-" -}}
{{- end -}}
{{- end -}}
{{- end -}}