$pfxPassword = ConvertTo-SecureString -String "password" -Force -AsPlainText

# import the pfx certificate
Import-PfxCertificate -FilePath ./squidex-dev.pfx Cert:\LocalMachine\My -Password $pfxPassword -Exportable

# trust the certificate by importing the pfx certificate into your trusted root
Import-Certificate -FilePath ./squidex-dev.cer -CertStoreLocation Cert:\CurrentUser\Root