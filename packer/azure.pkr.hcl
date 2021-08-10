variable "squidex_version" {
  type = string
  default = "5.8.0"
}

variable "subscription_id" {
  type = string
  default = "${env("AZURE_SUBSCRIPTION_ID")}"
}

variable "client_id" {
  type = string
  default = "${env("AZURE_CLIENT_ID")}"
}

variable "client_secret" {
  type = string
  default = "${env("AZURE_CLIENT_SECRET")}"
}

variable "tenant_id" {
  type = string
  default = "${env("AZURE_TENANT_ID")}"
}

source "azure-arm" "squidex" {
  subscription_id = "${var.subscription_id}"
  client_id = "${var.client_id}"
  client_secret = "${var.client_secret}"
  tenant_id = "${var.tenant_id}"

  os_type = "Linux"
  image_publisher = "Canonical"
  image_offer = "0001-com-ubuntu-server-focal"
  image_sku = "20_04-lts"

  managed_image_name = "squidex-${replace(var.squidex_version, ".", "-")}-{{ timestamp }}"
  managed_image_resource_group_name = "squidex"

  location = "West US"
  vm_size = "Standard_A2"
}

build {
  sources = [
    "source.azure-arm.squidex"
  ]

  provisioner "ansible" {
    ansible_env_vars = [
      "ANSIBLE_HOST_KEY_CHECKING=False",
      "ANSIBLE_SSH_ARGS='-F /dev/null -o ForwardAgent=no -o ControlMaster=auto -o ControlPersist=60s'",
      "ANSIBLE_NOCOLOR=True"
    ]
    extra_arguments = [
      "--extra-vars",
      "squidex_version=${var.squidex_version}"]
    playbook_file = "./ansible/playbook.yml"
    use_proxy = false
  }

  provisioner "shell" {
    execute_command = "chmod +x {{ .Path }}; {{ .Vars }} sudo -E sh '{{ .Path }}'"
    inline = [
      "/usr/sbin/waagent -force -deprovision+user && export HISTSIZE=0 && sync"
    ]
    inline_shebang = "/bin/sh -x"
    skip_clean = true
  }
}